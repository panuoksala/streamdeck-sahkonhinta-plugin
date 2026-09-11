using System;
using System.Text.Json.Serialization;

namespace StreamDeckSahkonhinta.Models
{
    /// <summary>
    /// A single spot-hinta.fi price slot (15 minutes by default).
    /// Mirrors the "priceModel" schema from https://api.spot-hinta.fi/swagger.json.
    /// </summary>
    public class PriceEntry
    {
        [JsonPropertyName("Rank")]
        public int? Rank { get; set; }

        [JsonPropertyName("DateTime")]
        public DateTimeOffset? DateTime { get; set; }

        [JsonPropertyName("PriceNoTax")]
        public double? PriceNoTax { get; set; }

        [JsonPropertyName("PriceWithTax")]
        public double? PriceWithTax { get; set; }

        /// <summary>
        /// The slot length is fixed by the requested priceResolution (15 minutes for this plugin).
        /// </summary>
        public static readonly TimeSpan SlotLength = TimeSpan.FromMinutes(15);

        /// <summary>
        /// True if the given point in time falls within this slot's [DateTime, DateTime + SlotLength) range.
        /// </summary>
        public bool Covers(DateTimeOffset pointInTime)
        {
            if (DateTime == null)
            {
                return false;
            }

            return pointInTime >= DateTime.Value && pointInTime < DateTime.Value + SlotLength;
        }
    }
}
