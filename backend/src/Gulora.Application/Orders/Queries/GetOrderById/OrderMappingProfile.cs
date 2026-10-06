using AutoMapper;
using Gulora.Application.Common.Extensions;
using Gulora.Contracts.Orders;
using Gulora.Domain.Entities;
using Gulora.Domain.ValueObjects;

namespace Gulora.Application.Orders.Queries.GetOrderById;

public sealed class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        // Record hedeflerde .ForMember ctor parametrelerini doldurmaz; enum→string ve AddressSnapshot→Address
        // için ForCtorParam gerekir (aksi halde "needs to have a constructor with 0 args" yanlış uyarısı).
        CreateMap<Order, OrderResponse>()
            .ForCtorParam(nameof(OrderResponse.Status), o => o.MapFrom(s => s.Status.ToString()))
            .ForCtorParam(nameof(OrderResponse.StatusLabel), o => o.MapFrom(s => s.Status.ToDisplayString()))
            .ForCtorParam(nameof(OrderResponse.DeliveryType), o => o.MapFrom(s => s.DeliveryType.ToString()))
            .ForCtorParam(nameof(OrderResponse.PaymentMethod), o => o.MapFrom(s => s.PaymentMethod.ToString()))
            .ForCtorParam(nameof(OrderResponse.PaymentStatus), o => o.MapFrom(s => s.PaymentStatus.ToString()))
            .ForCtorParam(nameof(OrderResponse.Address), o => o.MapFrom(s => s.AddressSnapshot))
            .ForCtorParam(nameof(OrderResponse.GiftCardMessage), o => o.MapFrom(s => s.GiftCardMessage))
            .ForCtorParam(nameof(OrderResponse.GiftCardSenderName), o => o.MapFrom(s => s.GiftCardSenderName))
            .ForCtorParam(nameof(OrderResponse.GiftCardOccasion), o => o.MapFrom(s => s.GiftCardOccasion))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.StatusLabel, o => o.MapFrom(s => s.Status.ToDisplayString()))
            .ForMember(d => d.DeliveryType, o => o.MapFrom(s => s.DeliveryType.ToString()))
            .ForMember(d => d.PaymentMethod, o => o.MapFrom(s => s.PaymentMethod.ToString()))
            .ForMember(d => d.PaymentStatus, o => o.MapFrom(s => s.PaymentStatus.ToString()))
            .ForMember(d => d.Address, o => o.MapFrom(s => s.AddressSnapshot))
            .ForMember(d => d.GiftCardMessage, o => o.MapFrom(s => s.GiftCardMessage))
            .ForMember(d => d.GiftCardSenderName, o => o.MapFrom(s => s.GiftCardSenderName))
            .ForMember(d => d.GiftCardOccasion, o => o.MapFrom(s => s.GiftCardOccasion))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items))
            .ForMember(d => d.Trackings, o => o.MapFrom(s => s.Trackings));

        CreateMap<AddressSnapshot, AddressSnapshotResponse>();

        CreateMap<OrderItem, OrderItemResponse>()
            .ForMember(d => d.ProductSku, o => o.MapFrom(s => s.ProductSku));

        CreateMap<OrderTracking, OrderTrackingResponse>()
            .ForCtorParam(nameof(OrderTrackingResponse.Status), o => o.MapFrom(s => s.Status.ToString()))
            .ForCtorParam(nameof(OrderTrackingResponse.StatusLabel), o => o.MapFrom(s => s.Status.ToDisplayString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.StatusLabel, o => o.MapFrom(s => s.Status.ToDisplayString()));
    }
}
