using StreamDeckSahkonhinta.Models;
using StreamDeckLib;
using StreamDeckLib.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StreamDeckSahkonhinta
{
    public abstract class BaseAction : BaseStreamDeckActionWithSettingsModel<Models.SahkonhintaSettingsModel>
    {
        /// <summary>
        /// How often the background loop ticks. Actual network refreshes happen far less often
        /// (see <see cref="PriceWindow.RefreshInterval"/>); this only controls how quickly the
        /// display can pick up a new cached 15-minute slot or a due background refresh.
        /// </summary>
        private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(30);

        private CancellationTokenSource _backgroundTaskToken;

        public override async Task OnKeyUp(StreamDeckEventPayload args)
        {
            try
            {
                // Any press (tap or long-press) triggers the same immediate refresh.
                await OnTap(args);

                // Settings seems to be updated frequently when actions are performed.
                await Manager.SetSettingsAsync(args.context, SettingsModel);
            }
            catch (Exception ex)
            {
                await OnError(args, ex);
            }
        }

        public override async Task OnWillAppear(StreamDeckEventPayload args)
        {
            await base.OnWillAppear(args);

            try
            {
                await UpdateDisplay(args);
                StartBackgroundTask(args);
            }
            catch (Exception ex)
            {
                await OnError(args, ex);
            }
        }

        public override async Task OnDidReceiveSettings(StreamDeckEventPayload args)
        {
            await base.OnDidReceiveSettings(args);

            try
            {
                // Threshold/color settings just changed in the Property Inspector; re-render
                // immediately so the icon reflects them without waiting for the next tick/press.
                await UpdateDisplay(args);
            }
            catch (Exception ex)
            {
                await OnError(args, ex);
            }
        }

        public override async Task OnWillDisappear(StreamDeckEventPayload args)
        {
            try
            {
                StopBackgroundTask();
            }
            catch (Exception ex)
            {
                await OnError(args, ex);
            }
        }

        public abstract Task UpdateDisplay(StreamDeckEventPayload args);
        public abstract Task OnTap(StreamDeckEventPayload args);
        public abstract Task OnLongPress(StreamDeckEventPayload args);
        public abstract Task OnError(StreamDeckEventPayload args, Exception ex);

        protected void StartBackgroundTask(StreamDeckEventPayload args)
        {
            _backgroundTaskToken?.Cancel();
            _backgroundTaskToken = new CancellationTokenSource();

            _ = Task.Run(() => BackgroundTask(args, _backgroundTaskToken.Token));
        }

        protected void StopBackgroundTask()
        {
            if (_backgroundTaskToken != null)
            {
                _backgroundTaskToken.Cancel();
                _backgroundTaskToken = null;
            }
        }

        private async Task BackgroundTask(StreamDeckEventPayload args, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                // Cancellation exception is expected.
                await Task.Delay(TickInterval, ct);

                try
                {
                    await UpdateDisplay(args);
                }
                catch (Exception ex)
                {
                    await OnError(args, ex);
                }
            }
        }
    }
}
