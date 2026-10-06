using FluentValidation;
using MediatR;
using ValidationException = Gulora.Domain.Exceptions.ValidationException;

namespace Gulora.Application.Common.Behaviors;

/// <summary>
/// Pipeline davranışı — FluentValidation çalıştırır; hata varsa handler'a ulaşmadan
/// <see cref="ValidationException"/> fırlatır.
/// Kanonik sıra: LoggingBehavior → ValidationBehavior → Handler.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .ToList();

        if (failures.Count > 0)
        {
            var errors = failures
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            throw new ValidationException(errors);
        }

        return await next();
    }
}
