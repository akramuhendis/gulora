namespace Gulora.Contracts.Orders;

/// <summary>
/// Sipariş anındaki değişmez adres anlık görüntüsü.
/// Domain <c>AddressSnapshot</c> value object'inden eşlenir; canlı adres tablosuyla bağlantısı yoktur.
/// </summary>
public sealed record AddressSnapshotResponse(
    string RecipientName,
    string PhoneNumber,
    string City,
    string District,
    string FullLine,
    string? PostalCode,
    string? CountryCode);
