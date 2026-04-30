namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// Bir veya birden çok değişikliği, tek veritabanı transaksiyonu (atomik iş) içinde
/// sarmalayan uygulama sözleşmesi. <see cref="TransactionBehavior{TRequest, TResponse}"/>
/// tüm ICommand yürütmelerini buna yönlendirebilir.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Değer dönen asenkron işi transaksiyonla çalıştırır (ör. cevabı üret, sonra commit).</summary>
    /// <param name="operation">Çalıştırılacak <see cref="Func{TResult}"/>. Genelde "SaveChanges" tek seferde biter.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    Task<T> RunAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);

    /// <summary>Değer dönmeyen asenkron işi (ör. yalnız side-effect) transaksiyonla yürütür.</summary>
    /// <param name="operation">Çalıştırılacak eylem.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    Task RunAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}
