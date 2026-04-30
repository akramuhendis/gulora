using AutoMapper;
using Gulora.Contracts.Orders;
using Gulora.Domain.Entities;
using Gulora.Domain.ValueObjects;

namespace Gulora.Application.Mappings;

/// <summary>
/// Sipariş aggregate'ı: <see cref="Order"/>, <see cref="OrderItem"/>, <see cref="OrderTracking"/>,
/// teslimat anlık görüntüsü <see cref="AddressSnapshot"/> için <c>Contracts</c> DTO eşleştirme
/// (AutoMapper profil tanımı).
/// </summary>
public sealed class OrderMappingProfile : Profile
{
    /// <summary>DTO tarafı sabit/hesap alanlarını, domain türlerinden türetir; koleksiyonlar maddeler/iz sütun listelerine açıkça map edilir.</summary>
    public OrderMappingProfile()
    {
        // Teslimat anında "snapshot" adres, API'de aynı yapı ile döner; ekstra mapping gerekmez
        CreateMap<AddressSnapshot, AddressSnapshotDto>();
        // Satır toplamı, domain sınıfı üzerinde hesaplanan özellikten
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.LineTotal, o => o.MapFrom(s => s.LineTotal));
        // Zaman çizelgesi satırı: durumun hem enum hem "etiket" dizesi
        CreateMap<OrderTracking, OrderTrackingDto>()
            .ForMember(d => d.StatusLabel, o => o.MapFrom(s => s.Status.ToString()));
        // Ana sipariş: özet rozet, alt öğe ve geçmiş koleksiyonlarını açıkça listelere
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.StatusLabel, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items.ToList()))
            .ForMember(d => d.Tracking, o => o.MapFrom(s => s.Tracking.ToList()));
    }
}
