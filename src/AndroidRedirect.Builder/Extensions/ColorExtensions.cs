namespace AndroidRedirect.Builder.Extensions;

public static class ColorExtensions
{
    public static SystemColor ToSystemColor(this MauiColor color, bool maintainAlpha = false)
    {
        return SystemColor.FromArgb(
            maintainAlpha ? (int)(color.Alpha * 255) : 255,
            (int)(color.Red * 255),
            (int)(color.Green * 255),
            (int)(color.Blue * 255)
        );
    }

    public static SystemColor Alpha(this SystemColor color, byte alpha)
    {
        return SystemColor.FromArgb(
            alpha,
            color.R,
            color.G,
            color.B
        );
    }

    public static SystemColor MuteColor(this SystemColor color)
    {
        var (h, s, l) = color.ToHsl();

        s *= 0.2f;
        l *= 0.25f;

        return HslToRgb(h, s, l);
    }

    public static (float h, float s, float l) ToHsl(this SystemColor color)
    {
        var r = color.R / 255f;
        var g = color.G / 255f;
        var b = color.B / 255f;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));

        var h = 0f;
        var s = 0f;
        var l = (max + min) / 2f;

        if (max != min)
        {
            var d = max - min;
            s = l > 0.5f ? d / (2f - max - min) : d / (max + min);

            if (max == r)
                h = (g - b) / d + (g < b ? 6f : 0f);
            else if (max == g)
                h = (b - r) / d + 2f;
            else
                h = (r - g) / d + 4f;

            h /= 6f;
        }

        return (h, s, l);
    }

    private static SystemColor HslToRgb(float h, float s, float l)
    {
        float r, g, b;

        if (s == 0f)
        {
            r = g = b = l; // achromatic
        }
        else
        {
            var q = l < 0.5f ? l * (1f + s) : l + s - l * s;
            var p = 2f * l - q;

            r = HueToRgb(p, q, h + 1f / 3f);
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - 1f / 3f);
        }

        return SystemColor.FromArgb(
            255,
            (int)(r * 255),
            (int)(g * 255),
            (int)(b * 255)
        );
    }

    private static float HueToRgb(float p, float q, float t)
    {
        if (t < 0f) t += 1f;
        if (t > 1f) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }
}