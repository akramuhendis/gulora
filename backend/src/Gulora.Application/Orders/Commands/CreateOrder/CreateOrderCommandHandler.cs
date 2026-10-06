using Gulora.Domain.Entities;
using Gulora.Domain.Enums;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using Gulora.Domain.Services;
using Gulora.Domain.ValueObjects;
using MediatR;

namespace Gulora.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IDiscountCouponRepository _couponRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderNumberService _orderNumberService;
    private readonly IDeliveryFeeCalculator _deliveryFeeCalculator;
    private readonly IDiscountCalculator _discountCalculator;
    private readonly IPaymentRepository _paymentRepository;

    public CreateOrderCommandHandler(
        IUserRepository userRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IDiscountCouponRepository couponRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IOrderNumberService orderNumberService,
        IDeliveryFeeCalculator deliveryFeeCalculator,
        IDiscountCalculator discountCalculator)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _couponRepository = couponRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _orderNumberService = orderNumberService;
        _deliveryFeeCalculator = deliveryFeeCalculator;
        _discountCalculator = discountCalculator;
    }

    public async Task<Guid> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(cmd.UserId, ct)
            ?? throw new NotFoundException("Kullanıcı", cmd.UserId);

        var address = user.Addresses.FirstOrDefault(a => a.Id == cmd.AddressId)
            ?? throw new NotFoundException("Adres", cmd.AddressId);

        var addressSnapshot = AddressSnapshot.Create(
            address.RecipientName,
            address.PhoneNumber,
            address.City,
            address.District,
            address.FullLine,
            address.PostalCode);

        // Ürünleri bir kez getir, önbelleğe al; stok kontrolü ve sipariş kalemi oluşturma aynı geçişte
        var productCache = new Dictionary<Guid, Domain.Entities.Product>(cmd.Items.Count);
        var orderItems = new List<(Guid ProductId, string ProductName, string? Sku, decimal UnitPrice, int Quantity)>(cmd.Items.Count);

        foreach (var item in cmd.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, ct)
                ?? throw new NotFoundException("Ürün", item.ProductId);

            if (!product.IsAvailableForPurchase)
                throw new BusinessRuleException($"'{product.Name}' ürünü şu anda satışta değil.");

            if (product.StockQuantity < item.Quantity)
                throw new BusinessRuleException(
                    $"'{product.Name}' için yeterli stok yok. Mevcut: {product.StockQuantity}, İstenen: {item.Quantity}.");

            productCache[product.Id] = product;
            orderItems.Add((product.Id, product.Name, product.Sku, product.EffectivePrice, item.Quantity));
        }

        // Ara toplam
        var subTotal = Money.Of(orderItems.Sum(i => i.UnitPrice * i.Quantity));

        // Kargo ücreti
        var deliveryFee = _deliveryFeeCalculator.Calculate(cmd.DeliveryType, subTotal);

        // Kupon indirimi
        var discountAmount = Money.Zero;
        if (!string.IsNullOrWhiteSpace(cmd.CouponCode))
        {
            var coupon = await _couponRepository.GetByCodeAsync(cmd.CouponCode, ct)
                ?? throw new NotFoundException("Kupon bulunamadı veya geçersiz.");

            discountAmount = await _discountCalculator.CalculateDiscountAsync(coupon, subTotal, cmd.UserId, ct);
        }

        var orderNumber = await _orderNumberService.GenerateAsync(ct);

        var order = Order.Create(
            cmd.UserId,
            orderNumber.Value,
            addressSnapshot,
            cmd.DeliveryType,
            cmd.PaymentMethod,
            orderItems,
            deliveryFee.Amount,
            discountAmount.Amount,
            cmd.CouponCode,
            cmd.GiftCardMessage,
            cmd.GiftCardSenderName,
            cmd.GiftCardOccasion);

        // Önbellekten al — ikinci DB turu yok
        foreach (var item in cmd.Items)
            productCache[item.ProductId].DecrementStock(item.Quantity);

        _orderRepository.Add(order);

        var idempotencyKey = $"order-{order.Id:N}";
        var payment = Payment.Create(
            order.Id,
            cmd.UserId,
            cmd.PaymentMethod,
            order.TotalAmount,
            idempotencyKey);

        if (cmd.PaymentMethod == PaymentMethod.CashOnDelivery)
        {
            // Kapıda ödeme: tahsilat teslimatta; kayıt Pending kalır.
        }

        _paymentRepository.Add(payment);
        await _unitOfWork.SaveChangesAsync(ct);

        return order.Id;
    }
}
