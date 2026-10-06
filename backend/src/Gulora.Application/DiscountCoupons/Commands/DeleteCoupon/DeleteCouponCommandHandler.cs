using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.DiscountCoupons.Commands.DeleteCoupon;

public sealed class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand>
{
    private readonly IDiscountCouponRepository _couponRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCouponCommandHandler(IDiscountCouponRepository couponRepository, IUnitOfWork unitOfWork)
    {
        _couponRepository = couponRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCouponCommand cmd, CancellationToken ct)
    {
        var coupon = await _couponRepository.GetByIdAsync(cmd.CouponId, ct)
            ?? throw new NotFoundException("Kupon", cmd.CouponId);

        coupon.Deactivate();
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
