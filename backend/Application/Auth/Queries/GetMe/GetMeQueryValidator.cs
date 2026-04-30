using FluentValidation;

namespace Gulora.Application.Auth.Queries.GetMe;

/// <summary>
/// <see cref="GetMeQuery"/> için FluentValidation: kullanıcı kimliği boş olamaz.
/// </summary>
public sealed class GetMeQueryValidator : AbstractValidator<GetMeQuery>
{
    /// <summary>
    /// <c>UserId</c> alanının dolu olmasını zorunlu kılar.
    /// </summary>
    public GetMeQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
