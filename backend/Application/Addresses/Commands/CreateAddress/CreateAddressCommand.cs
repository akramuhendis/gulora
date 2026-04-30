using Gulora.Application.Common.Abstractions;
using Gulora.Contracts.Addresses;
using MediatR;

namespace Gulora.Application.Addresses.Commands.CreateAddress;

/// <summary>
/// Kullanıcı için yeni bir adres kaydı oluşturmayı isteyen komut.
/// </summary>
/// <param name="UserId">Adresin bağlanacağı kullanıcı kimliği.</param>
/// <param name="Title">Adres kartında gösterilecek kısa başlık (ör. Ev, İş).</param>
/// <param name="RecipientName">Teslim alacak kişinin adı soyadı.</param>
/// <param name="RecipientPhone">Teslimat için iletişim telefonu.</param>
/// <param name="City">Şehir bilgisi.</param>
/// <param name="District">İlçe bilgisi.</param>
/// <param name="Line">Açık adres satırı (sokak, bina, daire vb.).</param>
/// <param name="PostalCode">İsteğe bağlı posta kodu.</param>
/// <param name="IsDefault">Bu adresin kullanıcı için varsayılan olup olmayacağı; varsayılan <c>false</c>.</param>
public sealed record CreateAddressCommand(
    Guid UserId,
    string Title,
    string RecipientName,
    string RecipientPhone,
    string City,
    string District,
    string Line,
    string? PostalCode,
    bool IsDefault = false) : IRequest<AddressDto>, ICommand;
