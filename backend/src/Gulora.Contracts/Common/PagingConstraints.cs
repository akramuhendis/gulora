namespace Gulora.Contracts.Common;

/// <summary>
/// Sayfalama üst sınırı — Vite (mağaza/satıcı) və Expo mobil client ilə eyni olmalıdır.
/// </summary>
public static class PagingConstraints
{
    public const int MaxPageSize = 100;
    public const int DefaultPageSize = 20;
}
