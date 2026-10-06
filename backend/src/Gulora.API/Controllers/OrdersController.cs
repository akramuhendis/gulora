using Gulora.API.Extensions;
using Gulora.Application.Orders.Commands.CancelOrder;
using Gulora.Application.Orders.Commands.CompleteReturn;
using Gulora.Application.Orders.Commands.CreateOrder;
using Gulora.Application.Orders.Commands.RequestReturn;
using Gulora.Application.Orders.Queries.GetOrderById;
using Gulora.Application.Orders.Queries.GetUserOrders;
using Gulora.Contracts.Common;
using Gulora.Contracts.Orders;
using Gulora.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

/// <summary>
/// Müşteri sipariş uçları — listeleme, oluşturma, iptal ve iade.
/// Yönetim uçları için bkz. <c>api/v1/admin/orders</c>.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly ISender _mediator;

    public OrdersController(ISender mediator) => _mediator = mediator;

    /// <summary>Oturum açmış kullanıcının siparişlerini sayfali listeler.</summary>
    /// <response code="200">Sayfalı sipariş listesi.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] GetOrdersRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(
            new GetUserOrdersQuery(userId, req.Page, req.PageSize), ct);
        return Ok(result);
    }

    /// <summary>Sipariş detayını getirir (sipariş sahibine özel).</summary>
    /// <response code="200">Sipariş detayı.</response>
    /// <response code="403">Bu siparişi görüntüleme yetkiniz yok.</response>
    /// <response code="404">Sipariş bulunamadı.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(
            new GetOrderByIdQuery(id, userId, IsAdmin: false), ct);
        return Ok(result);
    }

    /// <summary>Yeni sipariş oluşturur.</summary>
    /// <response code="201">Oluşturulan siparişin ID'si.</response>
    /// <response code="400">Doğrulama hatası veya stok yetersizliği.</response>
    /// <response code="404">Adres veya ürün bulunamadı.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<DeliveryType>(req.DeliveryType, ignoreCase: true, out var deliveryType))
            return BadRequest(new ProblemDetails
            {
                Title = "Geçersiz teslimat türü",
                Detail = $"Geçerli değerler: {string.Join(", ", Enum.GetNames<DeliveryType>())}",
                Status = StatusCodes.Status400BadRequest
            });

        if (!Enum.TryParse<PaymentMethod>(req.PaymentMethod, ignoreCase: true, out var paymentMethod))
            return BadRequest(new ProblemDetails
            {
                Title = "Geçersiz ödeme yöntemi",
                Detail = $"Geçerli değerler: {string.Join(", ", Enum.GetNames<PaymentMethod>())}",
                Status = StatusCodes.Status400BadRequest
            });

        var userId = User.GetUserId();
        var items = req.Items
            .Select(i => new OrderItemInput(i.ProductId, i.Quantity))
            .ToList();

        var id = await _mediator.Send(new CreateOrderCommand(
            userId,
            req.AddressId,
            deliveryType,
            paymentMethod,
            items,
            req.CouponCode,
            req.GiftCardMessage,
            req.GiftCardSenderName,
            req.GiftCardOccasion), ct);

        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Siparişi iptal eder (satıcı təsdiqi gözləyən Pending sifarişlər üçün).</summary>
    /// <response code="204">İptal başarılı.</response>
    /// <response code="400">İptal edilemeyen durum.</response>
    /// <response code="403">Bu siparişe erişim yetkiniz yok.</response>
    /// <response code="404">Sipariş bulunamadı.</response>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(
        Guid id, [FromBody] CancelOrderRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _mediator.Send(new CancelOrderCommand(
            id, userId, req.Reason, IsAdmin: false), ct);
        return NoContent();
    }

    /// <summary>Teslim edilmiş sipariş için iade talebi açar.</summary>
    /// <response code="204">İade talebi alındı.</response>
    /// <response code="400">İade edilemeyen durum.</response>
    /// <response code="403">Bu siparişe erişim yetkiniz yok.</response>
    /// <response code="404">Sipariş bulunamadı.</response>
    [HttpPost("{id:guid}/return")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestReturn(
        Guid id, [FromBody] RequestReturnRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _mediator.Send(new RequestReturnCommand(id, userId, req.Reason), ct);
        return NoContent();
    }

    /// <summary>Bekleyen iade talebini tamamlar (Admin / Seller).</summary>
    /// <response code="204">İade tamamlandı.</response>
    [HttpPost("{id:guid}/complete-return")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteReturn(
        Guid id, [FromBody] CompleteReturnRequest req, CancellationToken ct)
    {
        var operatorId = User.GetUserId();
        Guid? sellerScope =
            User.IsInRole("Seller") && !User.IsInRole("Admin") ? operatorId : null;
        await _mediator.Send(
            new CompleteReturnCommand(id, operatorId, req.Note, sellerScope),
            ct);
        return NoContent();
    }
}
