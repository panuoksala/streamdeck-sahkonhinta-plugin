namespace StreamDeckSahkonhinta.Models
{
    public class SahkonhintaSettingsModel
    {
        /// <summary>
        /// Price (c/kWh) at or below which the icon is shown using <see cref="LowPriceColor"/>.
        /// </summary>
        public double LowPriceThreshold { get; set; } = 5;

        /// <summary>
        /// Price (c/kWh) at or above which the icon is shown using <see cref="HighPriceColor"/>.
        /// </summary>
        public double HighPriceThreshold { get; set; } = 15;

        /// <summary>
        /// Icon color (hex, e.g. "#2ecc71") used when the price is at or below <see cref="LowPriceThreshold"/>.
        /// </summary>
        public string LowPriceColor { get; set; } = "#2ecc71";

        /// <summary>
        /// Icon color (hex) used when the price is between the low and high thresholds.
        /// </summary>
        public string MediumPriceColor { get; set; } = "#f1c40f";

        /// <summary>
        /// Icon color (hex) used when the price is at or above <see cref="HighPriceThreshold"/>.
        /// </summary>
        public string HighPriceColor { get; set; } = "#e74c3c";

        /// <summary>
        /// Possible error message that occurred during the last price refresh.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
