using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Exceptions;

namespace Gulora.Application.SellerCampaignPlans;

internal static class SellerCampaignAuth
{
    internal static Guid RequireSellerUserId(ICurrentUserService cur)
    {
        if (!cur.IsAuthenticated || cur.UserId is null)
            throw new UnauthorizedException();
        if (!string.Equals(cur.Role, "Seller", StringComparison.Ordinal))
            throw new ForbiddenException("Bu əməliyyat yalnız satıcı hesabı üçündür.");
        return cur.UserId.Value;
    }
}
