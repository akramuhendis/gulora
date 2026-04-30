using Gulora.Domain.Entities;
using Gulora.Domain.Enums;
using Gulora.Domain.Exceptions;
using Gulora.Domain.ValueObjects;

namespace Gulora.Domain.Tests;

public class DomainRulesTests
{
    [Fact]
    public void Money_rejects_negative_amount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Money(-1m));
    }

    [Fact]
    public void Money_rejects_empty_currency()
    {
        Assert.Throws<ArgumentException>(() => new Money(0m, "  "));
    }

    [Fact]
    public void Money_Subtract_rejects_negative_result()
    {
        var a = new Money(5m);
        var b = new Money(10m);
        Assert.Throws<DomainException>(() => a.Subtract(b));
    }

    [Fact]
    public void Money_Multiply_rejects_negative_factor()
    {
        var a = new Money(5m);
        Assert.Throws<ArgumentOutOfRangeException>(() => a.Multiply(-1));
    }

    [Fact]
    public void Order_Create_rejects_empty_user_id()
    {
        var lines = new[] { new OrderLine(Guid.NewGuid(), "Rose", "https://x.test/a.png", 1, 10m) };
        var address = new AddressSnapshot("A", "B", "City", "Dist", "Line", null);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Order.Create(Guid.Empty, lines, address, DeliveryType.Standard, PaymentMethod.Card, 0));
    }

    [Fact]
    public void Order_Create_computes_total_via_money()
    {
        var lines = new[]
        {
            new OrderLine(Guid.NewGuid(), "A", "https://x.test/a.png", 2, 10m),
            new OrderLine(Guid.NewGuid(), "B", "https://x.test/b.png", 1, 5m),
        };
        var address = new AddressSnapshot("A", "B", "City", "Dist", "Line", null);
        var order = Order.Create(
            Guid.NewGuid(),
            lines,
            address,
            DeliveryType.Standard,
            PaymentMethod.Card,
            deliveryFee: 3m,
            discountAmount: 5m);

        Assert.Equal(23m, order.TotalAmount);
    }

    [Fact]
    public void Review_Create_rejects_empty_product_id()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Review.Create(Guid.Empty, Guid.NewGuid(), 5, "Yaxşı"));
    }

    [Fact]
    public void Review_IsListedInCatalog_respects_flags()
    {
        var r = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 4, "Ok");
        Assert.True(r.IsListedInCatalog());
        r.HideByModeration();
        Assert.False(r.IsListedInCatalog());
    }

    [Fact]
    public void Coupon_UpdateDiscountTerms_rejects_limit_below_used_count()
    {
        var c = Coupon.Create("SAVE10", DiscountType.Fixed, 10m, usageLimit: 5);
        for (var i = 0; i < 3; i++)
            c.IncrementUsage();

        Assert.Throws<DomainException>(() =>
            c.UpdateDiscountTerms(DiscountType.Fixed, 10m, 0, usageLimit: 2, expiresAt: null));
    }

    [Fact]
    public void CorporateInquiry_Create_trims_strings()
    {
        var q = CorporateInquiry.Create(
            "  Acme  ",
            "  Ali  ",
            "  a@b.co  ",
            "  +994  ",
            "  Mesaj  ");

        Assert.Equal("Acme", q.Company);
        Assert.Equal("Ali", q.ContactName);
        Assert.Equal("Mesaj", q.Message);
    }

    [Fact]
    public void Favorite_Create_rejects_duplicate_when_ids_passed()
    {
        var pid = Guid.NewGuid();
        Assert.Throws<DomainException>(() =>
            Favorite.Create(Guid.NewGuid(), pid, [pid]));
    }

    [Fact]
    public void SellerApplication_Reject_stores_reason_after_start_review()
    {
        var app = SellerApplication.Create("A", "B", "T", "e@test.com", "1", "Bakı", "Cat", "Msg");
        app.StartReview();
        app.Reject("  Uyğun deyil  ");
        Assert.Equal(SellerApplicationStatus.Rejected, app.Status);
        Assert.Equal("Uyğun deyil", app.RejectionReason);
    }

    [Fact]
    public void RefreshToken_Revoke_is_idempotent()
    {
        var t = RefreshToken.Create(Guid.NewGuid(), "token-value", 7);
        t.Revoke();
        t.Revoke();
        Assert.True(t.IsRevoked);
    }
}
