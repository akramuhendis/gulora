using Gulora.Application.Common.Abstractions;
using Gulora.Application.Common.Interfaces;
using MediatR;

namespace Gulora.Application.Common.Behaviors;

/// <summary>
/// Yalnızca <see cref="ICommand"/> ile işaretlenen yazma isteklerini
/// <see cref="IUnitOfWork"/> üzerinden veritabanı transaksiyonu içinde çalıştırır.
/// Okuma sorguları veya ICommand uygulamayan istekler doğrudan ilerletilir (transaction açılmaz).
/// </summary>
/// <typeparam name="TRequest">MediatR istek tipi.</typeparam>
/// <typeparam name="TResponse">Cevap tipi.</typeparam>
public sealed class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>İstek ICommand değilse; aksi halde tüm “next” pipeline’ı tek UoW/transaction sınırında yürütür.</summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICommand)
            return await next();

        return await unitOfWork.RunAsync(() => next(), cancellationToken).ConfigureAwait(false);
    }
}
