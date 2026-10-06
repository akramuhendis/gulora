using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.DiscountCoupons.Commands.CreateCoupon;

public sealed class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, Guid>
{
    private readonly IDiscountCouponRepository _couponRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCouponCommandHandler(IDiscountCouponRepository couponRepository, IUnitOfWork unitOfWork)
    {
        _couponRepository = couponRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCouponCommand cmd, CancellationToken ct)
    {
        var existing = await _couponRepository.GetByCodeAsync(cmd.Code, ct);
        if (existing is not null)
            throw new ConflictException($"'{cmd.Code}' kupon kodu zaten kullanılıyor.");

        var coupon = DiscountCoupon.Create(
            cmd.Code,
            cmd.Type,
            cmd.Value,
            cmd.Description,
            cmd.MinOrderAmount,
            cmd.MaxDiscountAmount,
            cmd.UsageLimit,
            cmd.StartsAt,
            cmd.ExpiresAt,
            cmd.RestrictedUserId);

        _couponRepository.Add(coupon);
        await _unitOfWork.SaveChangesAsync(ct);

        return coupon.Id;
    }
}
