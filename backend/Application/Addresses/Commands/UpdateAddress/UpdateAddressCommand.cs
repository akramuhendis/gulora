using Gulora.Application.Common.Abstractions;
using Gulora.Contracts.Addresses;
using MediatR;

namespace Gulora.Application.Addresses.Commands.UpdateAddress;

/// <summary>
/// Var olan bir adres kaydının alanlarını ve varsayılan durumunu güncellemeyi isteyen komut.
/// </summary>
/// <param name="UserId">İşlemi yapan kullanıcı kimliği (sahiplik kontrolü).</param>
/// <param name="AddressId">Güncellenecek adresin kimliği.</param>
/// <param name="Title">Adres başlığı.</param>
/// <param name="RecipientName">Alıcı adı soyadı.</param>
/// <param name="RecipientPhone">Alıcı telefonu.</param>
/// <param name="City">Şehir.</param>
/// <param name="District">İlçe.</param>
/// <param name="Line">Açık adres satırı.</param>
/// <param name="PostalCode">İsteğe bağlı posta kodu.</param>
/// <param name="IsDefault">Varsayılan adres olarak işaretlenip işaretlenmeyeceği; varsayılan <c>false</c>.</param>
public sealed record UpdateAddressCommand(
    Guid UserId,
    Guid AddressId,
    string Title,
    string RecipientName,
    string RecipientPhone,
    string City,
    string District,
    string Line,
    string? PostalCode,
    bool IsDefault = false) : IRequest<AddressDto>, ICommand;
