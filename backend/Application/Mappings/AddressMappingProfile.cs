using AutoMapper;
using Gulora.Contracts.Addresses;
using Gulora.Domain.Entities;

namespace Gulora.Application.Mappings;

/// <summary>
/// Kullanıcı kayıtlı <see cref="Address"/> (teslimat adresi) ile <see cref="AddressDto"/>
/// arasında bire bir eşleme; sütun isimleri hizalı olduğundan üye bazlı <c>ForMember</c> gerekmez.
/// </summary>
public sealed class AddressMappingProfile : Profile
{
    /// <summary>Adres listesi/ CRUD cevaplarında kullanılır.</summary>
    public AddressMappingProfile()
    {
        CreateMap<Address, AddressDto>();
    }
}
