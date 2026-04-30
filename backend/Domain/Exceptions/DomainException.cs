namespace Gulora.Domain.Exceptions;

/// <summary>
/// İş kurallarının ihlali durumunda fırlatılır (örn. geçersiz sipariş durumu geçişi, yetersiz stok).
/// Altyapı katmanı bu tür istisnaları genelde 400 Bad Request veya 422 ile eşler.
/// </summary>
/// <param name="message">İstemciye veya loga yansıyabilecek açıklama (tercihen sabit, güvenli metin).</param>
public class DomainException(string message) : Exception(message);
