using Gulora.Domain.Entities;
using Gulora.Domain.Enums;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Tests.Entities;

public class PaymentTests
{
    [Fact]
    public void Create_WithValidAmount_SetsPendingStatus()
    {
        var payment = Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            PaymentMethod.CreditCard,
            99.50m,
            "order-abc");

        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(99.50m, payment.Amount);
        Assert.Equal("order-abc", payment.IdempotencyKey);
    }

    [Fact]
    public void Create_WithZeroAmount_Throws()
    {
        Assert.Throws<BusinessRuleException>(() =>
            Payment.Create(Guid.NewGuid(), Guid.NewGuid(), PaymentMethod.CreditCard, 0m, "k1"));
    }

    [Fact]
    public void MarkCompleted_FromPending_Succeeds()
    {
        var payment = Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), PaymentMethod.CreditCard, 10m, "k2");

        payment.MarkCompleted("txn-1");

        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Equal("txn-1", payment.ProviderTransactionId);
        Assert.NotNull(payment.PaidAt);
    }
}

public class SellerApplicationTests
{
    [Fact]
    public void Approve_FromPending_SetsApproved()
    {
        var app = SellerApplication.Submit(
            "Ali Veli", "Gülora LLC", "1234567890",
            "ali@test.az", "+994501234567", "Bakı", "Çiçək", null);

        app.Approve();

        Assert.Equal(SellerApplicationStatus.Approved, app.Status);
    }

    [Fact]
    public void Reject_RequiresReason()
    {
        var app = SellerApplication.Submit(
            "Ali Veli", "Gülora LLC", "1234567890",
            "ali@test.az", "+994501234567", "Bakı", "Çiçək", null);

        Assert.Throws<ArgumentException>(() => app.Reject(" "));
    }
}

public class UserRoleTests
{
    [Fact]
    public void PromoteToSeller_FromCustomer_UpdatesRole()
    {
        var user = User.Create("A", "B", "test@gulora.az", "hash");

        user.PromoteToSeller();

        Assert.Equal(UserRole.Seller, user.Role);
    }

    [Fact]
    public void PromoteToSeller_FromAdmin_Throws()
    {
        var user = User.Create("Admin", "User", "admin@gulora.az", "hash", UserRole.Admin);

        Assert.Throws<BusinessRuleException>(() => user.PromoteToSeller());
    }
}
