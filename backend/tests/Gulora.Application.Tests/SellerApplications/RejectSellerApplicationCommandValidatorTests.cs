using FluentValidation.TestHelper;
using Gulora.Application.SellerApplications.Commands.RejectSellerApplication;

namespace Gulora.Application.Tests.SellerApplications;

public class RejectSellerApplicationCommandValidatorTests
{
    private readonly RejectSellerApplicationCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Reason_Empty()
    {
        var result = _validator.TestValidate(
            new RejectSellerApplicationCommand(Guid.NewGuid(), ""));

        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Valid()
    {
        var result = _validator.TestValidate(
            new RejectSellerApplicationCommand(Guid.NewGuid(), "Uyğun deyil"));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
