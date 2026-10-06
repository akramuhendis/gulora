using Gulora.Domain.Common;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>
/// Satıcı kampaniya planı — vitrin qiymətinə avtomatik tətbiq ayrı motor; hal-hazırda plan saxlanması və API.
/// </summary>
public sealed class SellerCampaignPlan : BaseEntity
{
    public Guid SellerUserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    /// <summary>discount | shipping (vitrin ilə uyğun dəyərlər).</summary>
    public string Kind { get; private set; } = "discount";
    public int Percent { get; private set; }
    public DateTime StartsAtUtc { get; private set; }
    public DateTime EndsAtUtc { get; private set; }
    public decimal? MinCartAmount { get; private set; }
    /// <summary>JSON — əhatə: type all | category | products.</summary>
    public string ScopeJson { get; private set; } = "{\"type\":\"all\"}";
    public string? Notes { get; private set; }

    private SellerCampaignPlan() { }

    public static SellerCampaignPlan Create(
        Guid sellerUserId,
        string name,
        string kind,
        int percent,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        decimal? minCartAmount,
        string scopeJson,
        string? notes)
    {
        ValidateCommon(name, kind, percent, startsAtUtc, endsAtUtc, minCartAmount, scopeJson);

        return new SellerCampaignPlan
        {
            SellerUserId = sellerUserId,
            Name = name.Trim(),
            Kind = NormalizeKind(kind),
            Percent = percent,
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = endsAtUtc,
            MinCartAmount = minCartAmount,
            ScopeJson = scopeJson,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };
    }

    public void Update(
        string name,
        string kind,
        int percent,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        decimal? minCartAmount,
        string scopeJson,
        string? notes)
    {
        ValidateCommon(name, kind, percent, startsAtUtc, endsAtUtc, minCartAmount, scopeJson);

        Name = name.Trim();
        Kind = NormalizeKind(kind);
        Percent = percent;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        MinCartAmount = minCartAmount;
        ScopeJson = scopeJson;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        MarkUpdated();
    }

    private static void ValidateCommon(
        string name,
        string kind,
        int percent,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        decimal? minCartAmount,
        string scopeJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeJson);

        if (name.Trim().Length > 200)
            throw new BusinessRuleException("Kampaniya adı 200 simvoldan uzun ola bilməz.");

        _ = NormalizeKind(kind);

        if (percent is < 1 or > 100)
            throw new BusinessRuleException("Faiz 1–100 aralığında olmalıdır.");

        if (endsAtUtc <= startsAtUtc)
            throw new BusinessRuleException("Bitmə tarixi başlanğıcdan sonra olmalıdır.");

        if (minCartAmount is < 0)
            throw new BusinessRuleException("Minimum səbət məbləği mənfi ola bilməz.");

        if (scopeJson.Length > 120_000)
            throw new BusinessRuleException("Əhatə JSON çox böyükdür.");
    }

    private static string NormalizeKind(string kind)
    {
        var k = kind.Trim().ToLowerInvariant();
        if (k is not ("discount" or "shipping"))
            throw new BusinessRuleException("Kampaniya növü yalnız 'discount' və ya 'shipping' ola bilər.");
        return k;
    }
}
