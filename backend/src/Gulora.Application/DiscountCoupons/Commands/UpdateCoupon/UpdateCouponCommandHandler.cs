using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.DiscountCoupons.Commands.UpdateCoupon;

public sealed class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand>
{
    private readonly IDiscountCouponRepository _couponRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCouponCommandHandler(
        IDiscountCouponRepository couponRepository,
        IUnitOfWork unitOfWork)
    {
        _couponRepository = couponRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCouponCommand cmd, CancellationToken ct)
    {
        var coupon = await _couponRepository.GetByIdAsync(cmd.CouponId, ct)
            ?? throw new Gulora.Domain.Exceptions.NotFoundException("İndirim kuponu", cmd.CouponId);

        coupon.UpdateMetadata(
            cmd.Description,
            cmd.MinOrderAmount,
            cmd.MaxDiscountAmount,
            cmd.UsageLimit,
            cmd.StartsAt,
            cmd.ExpiresAt,
            cmd.IsActive);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
