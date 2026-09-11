using Microsoft.Extensions.Logging;
using StreamDeckSahkonhinta.Models;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StreamDeckSahkonhinta.Services
{
    /// <summary>
    /// Fetches the Finnish electricity spot price from the free https://api.spot-hinta.fi API
    /// (no API key required). A single call to TodayAndDayForward (15-minute resolution) is used
    /// per refresh, rather than multiple JustNow calls, to stay well within the API's
    /// ~1 request/minute rate limit (see https://api.spot-hinta.fi/swagger.json).
    /// </summary>
    public class ElectricPriceService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.spot-hinta.fi/"),
        };

        private readonly ILogger _logger;

        public ElectricPriceService(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Fetches today's (and tomorrow's, once published) prices at 15-minute resolution and
        /// returns a <see cref="PriceWindow"/> containing the entries covering "now" through
        /// "now + 4 hours".
        /// </summary>
        public async Task<PriceWindow> FetchPriceWindowAsync()
        {
            using var response = await _httpClient.GetAsync("TodayAndDayForward?priceResolution=15");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(CancellationToken.None);
            var allEntries = JsonSerializer.Deserialize<PriceEntry[]>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            }) ?? Array.Empty<PriceEntry>();

            var now = DateTimeOffset.UtcNow;
            var windowEntries = PriceWindow.SelectWindow(allEntries, now);

            return new PriceWindow(windowEntries, now);
        }

        public string GetPriceStatusColor(double? price, SahkonhintaSettingsModel settings)
        {
            if (price == null)
            {
                return null;
            }

            if (price <= settings.LowPriceThreshold)
            {
                return settings.LowPriceColor;
            }

            if (price >= settings.HighPriceThreshold)
            {
                return settings.HighPriceColor;
            }

            return settings.MediumPriceColor;
        }
    }
}
