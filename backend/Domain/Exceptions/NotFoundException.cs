namespace Gulora.Domain.Exceptions;

/// <summary>
/// İstenen kayıt veri deposunda bulunamadığında kullanılır; genelde HTTP 404 ile eşlenir.
/// </summary>
/// <param name="entity">Kaynak türünün adı (örn. "Order", "Product").</param>
/// <param name="key">Aranan kimlik veya anahtar değeri.</param>
public class NotFoundException(string entity, object key)
    : Exception($"{entity} (id: {key}) tapılmadı.");
