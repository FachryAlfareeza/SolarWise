using System.Globalization;

namespace SolarWise;

public static class Theme
{
    public static readonly Color ClassicBlue = ColorTranslator.FromHtml("#000080");
    public static readonly Color TitleBlue = ColorTranslator.FromHtml("#0000AA");
    public static readonly Color WindowGray = ColorTranslator.FromHtml("#C0C0C0");
    public static readonly Color LightGray = ColorTranslator.FromHtml("#DFDFDF");
    public static readonly Color SolarYellow = ColorTranslator.FromHtml("#FFD700");
    public static readonly Color SuccessGreen = ColorTranslator.FromHtml("#008000");
    public static readonly Color ErrorRed = ColorTranslator.FromHtml("#CC0000");

    public static readonly Font UiFont = new("Tahoma", 8.25f);
    public static readonly Font BoldFont = new("Tahoma", 8.25f, FontStyle.Bold);
    public static readonly Font HeadingFont = new("Tahoma", 11f, FontStyle.Bold);
    public static readonly Font NumberFont = new("Tahoma", 14f, FontStyle.Bold);

    public static readonly CultureInfo IdCulture = new("id-ID");

    public static string Num(double value, string format = "N0") => value.ToString(format, IdCulture);

    public static string Idr(double value) => "Rp " + Num(value);

    // Small solar icon: a sun above a solar panel, drawn at any size.
    public static Bitmap SolarIcon(int size = 16)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        float s = size / 16f;
        g.FillEllipse(Brushes.Gold, 0, 0, 8 * s, 8 * s);
        g.FillRectangle(Brushes.Navy, 6 * s, 8 * s, 10 * s, 7 * s);
        g.DrawRectangle(Pens.White, 6 * s, 8 * s, 10 * s, 7 * s);
        return bmp;
    }
}
