namespace Gulora.Application.Common.Media;

/// <summary>API kablo formatı: /uploads/... (xarici CDN toxunulmaz).</summary>
public static class MediaUrlNormalizer
{
    private const string UploadsPrefix = "/uploads/";

    public static string? ToPublicPath(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var t = url.Trim();
        if (t.StartsWith(UploadsPrefix, StringComparison.OrdinalIgnoreCase))
            return t;

        if (Uri.TryCreate(t, UriKind.Absolute, out var uri)
            && uri.AbsolutePath.StartsWith(UploadsPrefix, StringComparison.OrdinalIgnoreCase))
            return uri.AbsolutePath;

        return t;
    }
}
