using FluentValidation;
using MediatR;

namespace Gulora.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline davranışı. İstek, ilgili <see cref="IValidator{T}"/> (FluentValidation) ile
/// doğrulanır; hata varsa <see cref="ValidationException"/> fırlatılır, böylece handler çalışmaz.
/// Validasyon, controller veya tekil handler içine dağıtılmadan merkezileştirilmiş olur.
/// </summary>
/// <typeparam name="TRequest">MediatR istek tipi.</typeparam>
/// <typeparam name="TResponse">Cevap tipi.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Kayıtlı tüm validatörleri çalıştırır; hata yoksa bir sonraki pipeline/izleyici adıma geçer.
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        // Bu istek türü için hiç IValidator yoksa doğrulamayı atla
        if (!validators.Any()) return await next();

        // FluentValidation bağlamı: isteğin kendisi "model"dir
        var context = new ValidationContext<TRequest>(request);

        // Tüm validatörleri paralel koştur; sonuçları tek hata listesinde birleştir
        var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
