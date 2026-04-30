namespace Gulora.Application.Common.Abstractions;

/// <summary>
/// Durumsuz (marker) arayüz. Bu arayüzü uygulayan <c>IRequest</c> tipleri, MediatR pipeline'ında
/// <see cref="Gulora.Application.Common.Behaviors.TransactionBehavior{TRequest, TResponse}"/>
/// tarafından tek bir veritabanı işlemi (transaction) sınırı içinde çalıştırılmak üzere işaretlenir.
/// Sorgular (okuma) genellikle <c>ICommand</c> uygulamaz; böylece transaction gereksiz açılmaz.
/// </summary>
public interface ICommand;
