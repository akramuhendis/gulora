namespace Gulora.Application.SellerCampaignPlans;

internal static class DateTimeUtc
{
    public static DateTime AsUtc(DateTime dt) =>
        dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };
}
