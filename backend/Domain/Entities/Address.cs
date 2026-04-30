using Gulora.Domain.Common;
using Gulora.Domain.Events;

namespace Gulora.Domain.Entities;

/// <summary>
/// Kullanıcının kayıtlı teslimat adresi. “Varsayılan adres” birden fazla olmamalıdır;
/// bu kural uygulama katmanında diğer adreslerin <see cref="UnsetDefault"/> ile temizlenmesiyle sağlanır.
/// </summary>
public class Address : BaseEntity
{
    /// <summary>Adresin sahibi olan kullanıcı.</summary>
    public Guid UserId { get; private set; }
    /// <summary>Ev, iş gibi kullanıcıya görünen kısa başlık.</summary>
    public string Title { get; private set; } = default!;
    /// <summary>Teslim alacak kişinin adı soyadı.</summary>
    public string RecipientName { get; private set; } = default!;
    /// <summary>Teslimat için aranacak telefon.</summary>
    public string RecipientPhone { get; private set; } = default!;
    /// <summary>Şehir.</summary>
    public string City { get; private set; } = default!;
    /// <summary>İlçe / semt.</summary>
    public string District { get; private set; } = default!;
    /// <summary>Mahalle, sokak, kapı no vb. açık adres satırı.</summary>
    public string Line { get; private set; } = default!;
    /// <summary>Posta kodu (isteğe bağlı).</summary>
    public string? PostalCode { get; private set; }
    /// <summary>Checkout’ta öncelikli önerilen adres.</summary>
    public bool IsDefault { get; private set; }

    private Address() { }

    /// <summary>Yeni adres kaydı.</summary>
    public static Address Create(
        Guid userId, string title,
        string recipientName, string recipientPhone,
        string city, string district, string line,
        string? postalCode = null, bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientPhone);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(district);
        ArgumentException.ThrowIfNullOrWhiteSpace(line);
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);

        var address = new Address
        {
            UserId = userId, Title = title.Trim(),
            RecipientName = recipientName.Trim(), RecipientPhone = recipientPhone.Trim(),
            City = city.Trim(), District = district.Trim(), Line = line.Trim(),
            PostalCode = string.IsNullOrWhiteSpace(postalCode) ? null : postalCode.Trim(),
            IsDefault = isDefault
        };
        address.RegisterDomainEvent(new AddressCreatedDomainEvent(address.Id, userId));
        return address;
    }

    /// <summary>Formdan gelen değerlerle tüm adres alanlarını günceller.</summary>
    public void Update(string title, string recipientName, string recipientPhone,
        string city, string district, string line, string? postalCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientPhone);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(district);
        ArgumentException.ThrowIfNullOrWhiteSpace(line);
        Title = title.Trim();
        RecipientName = recipientName.Trim();
        RecipientPhone = recipientPhone.Trim();
        City = city.Trim();
        District = district.Trim();
        Line = line.Trim();
        PostalCode = string.IsNullOrWhiteSpace(postalCode) ? null : postalCode.Trim();
        SetUpdated();
    }

    /// <summary>Bu adresi varsayılan olarak işaretler (diğer adresler uygulama tarafında sıfırlanmalıdır).</summary>
    public void SetAsDefault() { IsDefault = true; SetUpdated(); }
    /// <summary>Varsayılan bayrağını kaldırır.</summary>
    public void UnsetDefault() { IsDefault = false; SetUpdated(); }
}
