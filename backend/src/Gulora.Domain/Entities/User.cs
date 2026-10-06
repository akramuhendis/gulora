using Gulora.Domain.Common;
using Gulora.Domain.Enums;
using Gulora.Domain.Events;
using Gulora.Domain.Exceptions;
using EmailVO = Gulora.Domain.ValueObjects.Email;

namespace Gulora.Domain.Entities;

/// <summary>
/// Kullanıcı aggregate root'u.
/// Kimlik doğrulama, yetkilendirme ve profil yönetimini kapsar.
/// Soft delete: <see cref="IsActive"/> false yapılır — fiziksel silme yoktur.
/// </summary>
public sealed class User : BaseAggregateRoot
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool EmailVerified { get; private set; }
    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationExpiresAt { get; private set; }
    public bool PhoneVerified { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private readonly List<Address> _addresses = [];
    public IReadOnlyList<Address> Addresses => _addresses.AsReadOnly();

    /// <summary>EF Core navigation için özel yapıcı.</summary>
    private User() { }

    /// <summary>
    /// Yeni kullanıcı oluşturur ve <see cref="UserRegisteredEvent"/> yayınlar.
    /// Parola bu noktada zaten hash'lenmiş olmalıdır.
    /// </summary>
    public static User Create(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        UserRole role = UserRole.Customer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        var emailVo = EmailVO.Of(email);

        var user = new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = emailVo.Value,
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true,
            EmailVerified = false,
            PhoneVerified = false
        };

        user.AddDomainEvent(new UserRegisteredEvent(
            user.Id,
            user.Email,
            user.FullName,
            user.Role));

        return user;
    }

    /// <summary>Tam ad (FirstName + LastName).</summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>Profil bilgilerini günceller.</summary>
    public void UpdateProfile(string firstName, string lastName, string? phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber?.Trim();
        MarkUpdated();
    }

    /// <summary>
    /// Parolayı değiştirir ve <see cref="UserPasswordChangedEvent"/> yayınlar.
    /// Eski hash doğrulaması Application handler'da yapılır; burada yeni hash kabul edilir.
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);
        PasswordHash = newPasswordHash;
        MarkUpdated();
        AddDomainEvent(new UserPasswordChangedEvent(Id, Email));
    }

    /// <summary>Kayıt sonrası e-postaya gönderilen tek kullanımlık doğrulama jetonunu ayarlar.</summary>
    public void SetPendingEmailVerification(string token, DateTime expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        if (expiresAtUtc <= DateTime.UtcNow)
            throw new BusinessRuleException("Doğrulama jetonunun bitiş zamanı geçmişte olamaz.");

        EmailVerificationToken = token.Trim();
        EmailVerificationExpiresAt = expiresAtUtc;
        MarkUpdated();
    }

    /// <summary>E-posta doğrulamasını jeton ile tamamlar.</summary>
    public void CompleteEmailVerification(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        if (EmailVerified)
            throw new BusinessRuleException("E-posta adresi zaten doğrulanmış.");

        if (string.IsNullOrWhiteSpace(EmailVerificationToken)
            || !string.Equals(
                EmailVerificationToken,
                token.Trim(),
                StringComparison.Ordinal))
            {
            throw new UnauthorizedException("E-posta doğrulama bağlantısı geçersiz.");
        }

        if (EmailVerificationExpiresAt < DateTime.UtcNow)
            throw new BusinessRuleException("E-posta doğrulama bağlantısının süresi dolmuş.");

        EmailVerified = true;
        EmailVerificationToken = null;
        EmailVerificationExpiresAt = null;
        MarkUpdated();
    }

    /// <summary>SMS OTP doğrulamasından sonra telefonu doğrulanmış olarak işaretler.</summary>
    public void VerifyPhoneNumber(string normalizedPhone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedPhone);
        PhoneNumber = normalizedPhone.Trim();
        PhoneVerified = true;
        MarkUpdated();
    }

    /// <summary>Son giriş zamanını günceller.</summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        MarkUpdated();
    }

    /// <summary>Hesabı devre dışı bırakır (soft delete).</summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new BusinessRuleException("Kullanıcı zaten pasif durumda.");

        IsActive = false;
        MarkUpdated();
    }

    /// <summary>Hesabı yeniden aktif eder.</summary>
    public void Activate()
    {
        if (IsActive)
            throw new BusinessRuleException("Kullanıcı zaten aktif durumda.");

        IsActive = true;
        MarkUpdated();
    }

    /// <summary>Müşteri hesabını satıcı rolüne yükseltir (admin onayı sonrası).</summary>
    public void PromoteToSeller()
    {
        if (Role == UserRole.Seller)
            return;

        if (Role == UserRole.Admin)
            throw new BusinessRuleException("Admin hesabı satıcı rolüne dönüştürülemez.");

        Role = UserRole.Seller;
        MarkUpdated();
    }

    /// <summary>Yeni refresh token ekler ve eski aktif token'ları iptal eder (login token rotasyonu).</summary>
    public void AddRefreshToken(RefreshToken token)
    {
        var existing = _refreshTokens.Where(t => t.IsActive).ToList();
        foreach (var old in existing)
            old.Revoke(token.Token);

        _refreshTokens.Add(token);
        MarkUpdated();
    }

    /// <summary>
    /// Parola sıfırlama token'ı ekler; mevcut aktif oturumlar iptal edilmez.
    /// Token kısa ömürlüdür (1 saat). Parola sıfırlandıktan sonra tüm token'lar iptal edilir.
    /// </summary>
    public void AddPasswordResetToken(RefreshToken token)
    {
        _refreshTokens.Add(token);
        MarkUpdated();
    }

    /// <summary>Tüm aktif refresh token'ları iptal eder (şifre değişimi vb.).</summary>
    public void RevokeAllRefreshTokens()
    {
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
            token.Revoke();

        MarkUpdated();
    }

    /// <summary>Yeni adres ekler (maksimum 5 adres limiti).</summary>
    public void AddAddress(Address address)
    {
        if (_addresses.Count >= 5)
            throw new BusinessRuleException("Bir kullanıcı en fazla 5 adres tanımlayabilir.");

        _addresses.Add(address);
        MarkUpdated();
    }

    /// <summary>Adresi siler.</summary>
    public void RemoveAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new NotFoundException(nameof(Address), addressId);

        _addresses.Remove(address);
        MarkUpdated();
    }
}
