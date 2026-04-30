namespace Gulora.Domain.ValueObjects;

/// <summary>
/// Sipariş oluşturulduğu andaki teslimat adresinin kopyası.
/// Kullanıcının kayıtlı <see cref="Gulora.Domain.Entities.Address"/> kaydı sonradan değişse bile
/// sipariş üzerindeki bu anlık görüntü değişmez (denetim ve hukuki tutarlılık).
/// </summary>
/// <param name="RecipientName">Teslim alacak kişinin adı.</param>
/// <param name="RecipientPhone">İletişim telefonu.</param>
/// <param name="City">Şehir.</param>
/// <param name="District">İlçe / semt.</param>
/// <param name="Line">Sokak, bina, daire vb. açık adres.</param>
/// <param name="PostalCode">Posta kodu; bilinmiyorsa null.</param>
public record AddressSnapshot(
    string RecipientName,
    string RecipientPhone,
    string City,
    string District,
    string Line,
    string? PostalCode)
{
    /// <summary>Sipariş oluşturma öncesi zorunlu alanların dolu olduğunu doğrular.</summary>
    /// <exception cref="ArgumentException">Boş veya yalnızca boşluk içeren alan varsa.</exception>
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(RecipientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(RecipientPhone);
        ArgumentException.ThrowIfNullOrWhiteSpace(City);
        ArgumentException.ThrowIfNullOrWhiteSpace(District);
        ArgumentException.ThrowIfNullOrWhiteSpace(Line);
    }
}
