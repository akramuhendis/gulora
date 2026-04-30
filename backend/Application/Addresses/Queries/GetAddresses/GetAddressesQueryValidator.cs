using FluentValidation;

namespace Gulora.Application.Addresses.Queries.GetAddresses;

/// <summary>
/// <see cref="GetAddressesQuery"/> için FluentValidation: kullanıcı kimliği boş olamaz.
/// </summary>
public sealed class GetAddressesQueryValidator : AbstractValidator<GetAddressesQuery>
{
    /// <summary>
    /// <c>UserId</c> alanının dolu olmasını zorunlu kılar.
    /// </summary>
    public GetAddressesQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
