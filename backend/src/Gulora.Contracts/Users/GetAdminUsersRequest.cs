using Gulora.Contracts.Common;

namespace Gulora.Contracts.Users;

/// <summary>GET api/v1/admin/users sorgu parametreleri (Admin).</summary>
public sealed record GetAdminUsersRequest : PagedRequest
{
    /// <summary>Ad, soyad veya e-posta ile arama.</summary>
    public string? Search { get; init; }

    /// <summary>
    /// Rol filtresi. Geçerli değerler: Customer | Admin | Seller.
    /// </summary>
    public string? Role { get; init; }

    /// <summary>Yalnızca aktif / pasif kullanıcılar.</summary>
    public bool? IsActive { get; init; }
}
