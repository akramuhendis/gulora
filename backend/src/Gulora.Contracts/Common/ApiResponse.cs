namespace Gulora.Contracts.Common;

/// <summary>
/// Opsiyonel zarflama tipi — standart başarı/hata yanıtı için kullanılabilir.
/// Projede doğrudan veri döndürme tercih ediliyorsa bu zarf opsiyoneldir;
/// ProblemDetails hata yolunda ayrıca <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> kullanılır.
/// </summary>
public sealed record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message = null);

/// <summary>Veri içermeyen başarı/hata bildirimi.</summary>
public sealed record ApiResponse(
    bool Success,
    string? Message = null);
