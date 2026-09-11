using Microsoft.Extensions.Logging;
using StreamDeckSahkonhinta.Models;
using StreamDeckSahkonhinta.Services;
using StreamDeckLib;
using StreamDeckLib.Messages;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace StreamDeckSahkonhinta
{
    [ActionUuid(Uuid = "net.oksala.sahkonhinta.price")]
    [SupportedOSPlatform("windows")]
    public class SahkonhintaPriceAction : BaseAction
    {
        private readonly ElectricPriceService _service;
        private readonly object _cacheLock = new object();
        private PriceWindow _cache;

        public SahkonhintaPriceAction()
        {
            _service = new ElectricPriceService(Logger);
        }

        public override async Task UpdateDisplay(StreamDeckEventPayload args)
        {
            await RefreshAndRender(args.context, forceRefresh: false);
        }

        public override async Task OnTap(StreamDeckEventPayload args)
        {
            // Any press immediately reloads the current price + next 4 hours.
            await RefreshAndRender(args.context, forceRefresh: true);
            await Manager.ShowOkAsync(args.context);
        }

        public override async Task OnLongPress(StreamDeckEventPayload args)
        {
            await OnTap(args);
        }

        public override async Task OnError(StreamDeckEventPayload args, Exception ex)
        {
            try
            {
                SettingsModel.ErrorMessage = ex.Message;

                await Manager.ShowAlertAsync(args.context);

                // Only fall back to the "unknown" icon if we have never had a good price to show.
                if (_cache == null)
                {
                    await SetImageFromTempFileAsync(args.context, IconRenderer.RenderUnknownIconFile());
                }

                await Manager.SetSettingsAsync(args.context, SettingsModel);
            }
            catch (Exception handlingException)
            {
                Logger.LogError(handlingException, $"Failed to handle error: {ex.Message}");
            }
        }

        private async Task RefreshAndRender(string context, bool forceRefresh)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                bool needsRefresh;
                lock (_cacheLock)
                {
                    needsRefresh = forceRefresh || _cache == null || _cache.IsRefreshDue(now);
                }

                if (needsRefresh)
                {
                    var window = await _service.FetchPriceWindowAsync();
                    lock (_cacheLock)
                    {
                        _cache = window;
                    }

                    SettingsModel.ErrorMessage = string.Empty;
                }

                PriceWindow cache;
                lock (_cacheLock)
                {
                    cache = _cache;
                }

                var currentEntry = cache?.GetCurrentEntry(now);

                // The API returns EUR/kWh (e.g. 0.06272); thresholds/display are in c/kWh.
                var price = currentEntry?.PriceWithTax * 100;

                if (price.HasValue)
                {
                    var color = _service.GetPriceStatusColor(price, SettingsModel) ?? IconRenderer.UnknownColor;
                    var iconPath = IconRenderer.RenderPriceIconFile(price.Value.ToString("F2"), string.Empty, color);
                    await SetImageFromTempFileAsync(context, iconPath);
                }
                else if (cache == null)
                {
                    // Never successfully loaded a price yet.
                    await SetImageFromTempFileAsync(context, IconRenderer.RenderUnknownIconFile());
                }

                // The price/unit are baked into the icon itself, so no title overlay is used.
                await Manager.SetTitleAsync(context, string.Empty);

                await Manager.SetSettingsAsync(context, SettingsModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to update price.");

                // Keep showing the last known good price/icon; only surface the error.
                SettingsModel.ErrorMessage = ex.Message;
                await Manager.SetSettingsAsync(context, SettingsModel);
            }
        }

        /// <summary>
        /// Sends the icon at <paramref name="iconPath"/> to the given context, then deletes the
        /// temporary file (StreamDeckLib's SetImageAsync reads the image from disk rather than
        /// accepting an inline data URI, so icons are rendered to a temp PNG file first).
        /// </summary>
        private async Task SetImageFromTempFileAsync(string context, string iconPath)
        {
            try
            {
                await Manager.SetImageAsync(context, iconPath);
            }
            finally
            {
                try
                {
                    System.IO.File.Delete(iconPath);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to delete temporary icon file {IconPath}", iconPath);
                }
            }
        }
    }
}
