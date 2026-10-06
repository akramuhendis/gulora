using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Gulora.Domain.Enums;
using Gulora.Domain.Exceptions;

namespace Gulora.Application.Common.Authorization;

/// <summary>
/// Satıcıların yalnızca kendi ürünlerini yönetmesini; admin'in tümünü yönetmesini sağlar.
/// </summary>
internal static class ProductMutationAuthorization
{
    internal static Guid? ResolveSellerIdForCreate(ICurrentUserService user)
    {
        if (IsAdmin(user))
            return null;

        if (IsSeller(user))
        {
            if (user.UserId is null)
                throw new UnauthorizedException("Satıcı kimliği doğrulanamadı.");
            return user.UserId;
        }

        throw new ForbiddenException("Ürün oluşturma yetkiniz yok.");
    }

    internal static void EnsureCanModifyProduct(ICurrentUserService user, Product product)
    {
        if (IsAdmin(user))
            return;

        if (IsSeller(user))
        {
            if (user.UserId is null)
                throw new UnauthorizedException("Satıcı kimliği doğrulanamadı.");

            if (product.SellerId != user.UserId)
                throw new ForbiddenException("Bu ürünü yönetme yetkiniz yok.");

            return;
        }

        throw new ForbiddenException("Bu ürünü yönetme yetkiniz yok.");
    }

    private static bool IsAdmin(ICurrentUserService user) =>
        string.Equals(user.Role, nameof(UserRole.Admin), StringComparison.OrdinalIgnoreCase);

    private static bool IsSeller(ICurrentUserService user) =>
        string.Equals(user.Role, nameof(UserRole.Seller), StringComparison.OrdinalIgnoreCase);
}
