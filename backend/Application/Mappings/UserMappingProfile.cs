using AutoMapper;
using Gulora.Contracts.Users;
using Gulora.Domain.Entities;

namespace Gulora.Application.Mappings;

/// <summary>
/// <see cref="User"/> (domain) &lt;-&gt; <see cref="UserDto"/> (API) eşleştirme. Rol, enum
/// sınıf dışına string olarak açıkça dönüştürülür (client bağımsız sözleşme).
/// </summary>
public sealed class UserMappingProfile : Profile
{
    /// <summary>Profil ve oturum açan cevapları doldurmak için temel eşleme; hassas sır alan map edilmez.</summary>
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));
    }
}
