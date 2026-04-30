using Gulora.Application.Common.Abstractions;
using MediatR;

namespace Gulora.Application.Addresses.Commands.DeleteAddress;

/// <summary>
/// Belirtilen kullanıcı bağlamında bir adresin silinmesini isteyen komut.
/// </summary>
/// <param name="AddressId">Silinecek adresin benzersiz kimliği.</param>
/// <param name="UserId">İşlemi başlatan kullanıcının kimliği (sahiplik kontrolü için).</param>
public sealed record DeleteAddressCommand(Guid AddressId, Guid UserId) : IRequest<Unit>, ICommand;
