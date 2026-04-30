using Gulora.Domain.Common;
using Gulora.Domain.Enums;
using Gulora.Domain.Events;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>
/// Uygulamaya kayıtlı müşteri veya yönetici kullanıcıyı temsil eder.
/// Kimlik bilgileri ve profil alanları dış dünyadan doğrudan değiştirilemez;
/// yalnızca aşağıdaki domain metotları üzerinden güncellenir (DDD kuralı).
/// </summary>
public class User : BaseEntity
{
    /// <summary>Kullanıcının adı.</summary>
    public string FirstName { get; private set; } = default!;
    /// <summary>Kullanıcının soyadı.</summary>
    public string LastName { get; private set; } = default!;
    /// <summary>Giriş ve bildirimler için benzersiz e-posta (küçük harfe normalize edilir).</summary>
    public string Email { get; private set; } = default!;
    /// <summary>İsteğe bağlı telefon numarası; OTP ile doğrulanabilir.</summary>
    public string? PhoneNumber { get; private set; }
    /// <summary>Ham şifre saklanmaz; yalnızca hash (ör. BCrypt) tutulur.</summary>
    public string PasswordHash { get; private set; } = default!;
    /// <summary>Sistem içi yetki: müşteri, admin veya satıcı.</summary>
    public UserRole Role { get; private set; } = UserRole.Customer;
    /// <summary>Hesap silindiğinde veya yönetici pasifleştirdiğinde false olur (soft delete).</summary>
    public bool IsActive { get; private set; } = true;
    /// <summary>Telefon OTP ile doğrulandıysa true.</summary>
    public bool IsPhoneVerified { get; private set; }
    /// <summary>Profil fotoğrafı URL’si; isteğe bağlı.</summary>
    public string? ProfileImageUrl { get; private set; }

    /// <summary>EF Core ve serileştirme için parametresiz ctor; uygulama kodu kullanmamalıdır.</summary>
    private User() { }

    /// <summary>Yeni kullanıcı kaydı oluşturur (fabrika metodu).</summary>
    /// <param name="firstName">Boş olamaz.</param>
    /// <param name="lastName">Boş olamaz.</param>
    /// <param name="email">Boş olamaz; trim + küçük harf uygulanır.</param>
    /// <param name="passwordHash">Önceden hashlenmiş şifre; boş olamaz.</param>
    public static User Create(string firstName, string lastName, string email, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var user = new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            // E-posta karşılaştırmaları için tutarlı küçük harf
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash
        };
        user.RegisterDomainEvent(new UserRegisteredDomainEvent(user.Id, user.Email));
        return user;
    }

    /// <summary>Ad, soyad ve e-posta bilgisini günceller.</summary>
    public void UpdateProfile(string firstName, string lastName, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        SetUpdated();
    }

    /// <summary>Şifre değişikliğinde hash’i günceller; düz metin buraya gelmemelidir.</summary>
    public void UpdatePassword(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);
        PasswordHash = newPasswordHash;
        SetUpdated();
    }

    /// <summary>Telefon numarasını atar veya temizler; yeni numara için doğrulama sıfırlanır.</summary>
    public void SetPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            PhoneNumber = null;
            IsPhoneVerified = false;
            SetUpdated();
            return;
        }

        PhoneNumber = phone.Trim();
        IsPhoneVerified = false;
        SetUpdated();
    }

    /// <summary>Profil fotoğrafı URL’sini günceller veya kaldırır (null = sil).</summary>
    public void SetProfileImageUrl(string? url)
    {
        ProfileImageUrl = string.IsNullOrWhiteSpace(url) ? null : url.Trim();
        SetUpdated();
    }

    /// <summary>
    /// Yönetici və ya sistem siyasəti ilə rol dəyişdirir; eyni rol üçün əməliyyat yoxdur.
    /// <see cref="PromoteToSeller"/> müştəri hesabından satıcı roluna keçid üçündür.
    /// </summary>
    public void SetRole(UserRole newRole)
    {
        if (Role == newRole) return;
        var previous = Role;
        Role = newRole;
        RegisterDomainEvent(new UserRoleChangedDomainEvent(Id, previous, Role));
        SetUpdated();
    }

    /// <summary>Yalnız müştəri hesabından satıcı roluna yüksəltmə (daxildən <see cref="SetRole"/>).</summary>
    public void PromoteToSeller()
    {
        if (Role != UserRole.Customer)
            throw new DomainException("Yalnız müştəri hesabı satıcı roluna keçirilə bilər.");
        SetRole(UserRole.Seller);
    }

    /// <summary>Pasifleştirilmiş hesabı yeniden etkinleştirir.</summary>
    /// <exception cref="DomainException">Hesap zaten aktifse.</exception>
    public void Activate()
    {
        if (IsActive) throw new DomainException("Hesab artıq aktivdir.");
        IsActive = true;
        SetUpdated();
    }

    /// <summary>Telefonun OTP ile doğrulandığını işaretler.</summary>
    public void VerifyPhone()
    {
        IsPhoneVerified = true;
        SetUpdated();
    }

    /// <summary>Hesabı pasifleştirir; fiziksel silme yerine soft delete kullanılır.</summary>
    /// <exception cref="DomainException">Hesap zaten pasifse fırlatılır.</exception>
    public void Deactivate()
    {
        if (!IsActive) throw new DomainException("Hesab artıq deaktivdir.");
        IsActive = false;
        SetUpdated();
    }
}
