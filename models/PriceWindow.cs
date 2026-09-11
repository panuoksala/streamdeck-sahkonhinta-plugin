using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamDeckSahkonhinta.Models
{
    /// <summary>
    /// Holds the cached price entries covering "now" through "now + 4 hours" at the time of the
    /// last successful fetch, plus the pure (no I/O) logic needed to decide what to show/refresh.
    /// Kept free of any HTTP/StreamDeck dependencies so it can be unit tested in isolation.
    /// </summary>
    public class PriceWindow
    {
        public static readonly TimeSpan ForecastHorizon = TimeSpan.FromHours(4);
        public static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(4);

        public IReadOnlyList<PriceEntry> Entries { get; }
        public DateTimeOffset FetchedAtUtc { get; }

        public PriceWindow(IEnumerable<PriceEntry> entries, DateTimeOffset fetchedAtUtc)
        {
            Entries = entries?.Where(e => e.DateTime != null).OrderBy(e => e.DateTime).ToList() ?? new List<PriceEntry>();
            FetchedAtUtc = fetchedAtUtc;
        }

        /// <summary>
        /// Filters a full price list (e.g. from TodayAndDayForward) down to the entries covering
        /// "now" through "now + 4 hours", suitable for constructing a new <see cref="PriceWindow"/>.
        /// </summary>
        public static IEnumerable<PriceEntry> SelectWindow(IEnumerable<PriceEntry> allEntries, DateTimeOffset now)
        {
            var horizonEnd = now + ForecastHorizon;

            return allEntries
                .Where(e => e.DateTime != null && e.DateTime + PriceEntry.SlotLength > now && e.DateTime < horizonEnd)
                .OrderBy(e => e.DateTime);
        }

        /// <summary>
        /// Returns the cached entry whose slot covers the given point in time, or null if the
        /// cache does not cover it (e.g. it is stale and a refresh is needed).
        /// </summary>
        public PriceEntry GetCurrentEntry(DateTimeOffset now)
        {
            return Entries.FirstOrDefault(e => e.Covers(now));
        }

        /// <summary>
        /// True if a full refresh should be triggered: either the fixed refresh interval has
        /// elapsed since the last fetch, or "now" is no longer covered by any cached entry.
        /// </summary>
        public bool IsRefreshDue(DateTimeOffset now)
        {
            if (now - FetchedAtUtc >= RefreshInterval)
            {
                return true;
            }

            return GetCurrentEntry(now) == null;
        }
    }
}
