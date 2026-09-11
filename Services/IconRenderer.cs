using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.Versioning;

namespace StreamDeckSahkonhinta.Services
{
    /// <summary>
    /// Renders the Stream Deck key icon entirely in code: the icon_small.png artwork as a
    /// rounded background, with the price and unit text baked directly into the image (rather
    /// than relying on Stream Deck's title overlay). Produces a PNG file on disk for
    /// SetImageAsync. Windows-only (System.Drawing.Common), matching this plugin's current OS
    /// support.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class IconRenderer
    {
        private const int Size = 144;
        private const string BackgroundColor = "#12141a";

        private static readonly string BackgroundImagePath = Path.Combine(AppContext.BaseDirectory, "images", "icon_small.png");

        /// <summary>
        /// Neutral gray accent, used before any price has ever been successfully loaded.
        /// </summary>
        public const string UnknownColor = "#808080";

        /// <summary>
        /// Renders the badge with a price + unit baked in, using <paramref name="accentColorHex"/>
        /// for the price text color based on the configured low/medium/high thresholds.
        /// </summary>
        public static string RenderPriceIconFile(string priceText, string unitText, string accentColorHex)
        {
            var accent = ColorTranslator.FromHtml(string.IsNullOrWhiteSpace(accentColorHex) ? UnknownColor : accentColorHex);

            using var bitmap = new Bitmap(Size, Size);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                DrawBackground(g);
                DrawPriceText(g, priceText, unitText, accent);
            }

            return SaveToTempFile(bitmap);
        }

        /// <summary>
        /// Renders the badge in the neutral "unknown" state (no price ever successfully loaded).
        /// </summary>
        public static string RenderUnknownIconFile()
        {
            return RenderPriceIconFile("--", string.Empty, UnknownColor);
        }

        private static void DrawBackground(Graphics g)
        {
            using var path = RoundedRect(new Rectangle(0, 0, Size, Size), 26);

            if (File.Exists(BackgroundImagePath))
            {
                using var backgroundImage = Image.FromFile(BackgroundImagePath);
                using var clippedRegion = new Region(path);
                var previousClip = g.Clip;
                g.Clip = clippedRegion;
                g.DrawImage(backgroundImage, new Rectangle(0, 0, Size, Size));
                g.Clip = previousClip;
            }
            else
            {
                using var brush = new SolidBrush(ColorTranslator.FromHtml(BackgroundColor));
                g.FillPath(brush, path);
            }
        }

        private static void DrawPriceText(Graphics g, string priceText, string unitText, Color accent)
        {
            using var priceFont = new Font("Segoe UI", 42, FontStyle.Bold, GraphicsUnit.Pixel);
            using var priceBrush = new SolidBrush(accent);
            var priceSize = g.MeasureString(priceText, priceFont);
            g.DrawString(priceText, priceFont, priceBrush, (Size - priceSize.Width) / 2f, 63f);

            if (string.IsNullOrEmpty(unitText))
            {
                return;
            }

            using var unitFont = new Font("Segoe UI", 13, FontStyle.Regular, GraphicsUnit.Pixel);
            using var unitBrush = new SolidBrush(Color.LightGray);
            var unitSize = g.MeasureString(unitText, unitFont);
            g.DrawString(unitText, unitFont, unitBrush, (Size - unitSize.Width) / 2f, 96f);
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var diameter = radius * 2;
            var path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        private static string SaveToTempFile(Bitmap bitmap)
        {
            var path = Path.Combine(Path.GetTempPath(), $"sahkonhinta_icon_{Guid.NewGuid():N}.png");
            bitmap.Save(path, ImageFormat.Png);
            return path;
        }
    }
}
