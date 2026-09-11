# Design: Replace Sahkonhinta price action with spot-hinta.fi data

Date: 2026-09-10
Author: Panu Oksala (with GitHub Copilot CLI)

## Background

The existing `net.oksala.sahkonhinta.price` action fetches the current Finnish
electricity spot price from `api.porssisahko.net` once per configured
interval. This design replaces that data source and behavior with
`api.spot-hinta.fi`, adds 15-minute price resolution, and removes most user
configuration in favor of a fixed, predictable refresh schedule.

## Goals

- Show the current Finnish electricity spot price (15-minute resolution) as
  text on the Stream Deck button.
- On startup, load the current price and the next 4 hours ahead.
- Automatically refresh that window every 4 hours.
- Let the user force an immediate refresh by pressing the button.
- Let the user configure low/medium/high price colors and the low/high
  thresholds that select them (this is the only configuration kept).

## Non-goals

- No tap vs. long-press distinction (any press = immediate refresh).
- No configurable refresh interval, decimal count, or "open URL" action.
- No display of the forecast values themselves (only the current price is
  shown); the forecast window exists purely so the display can stay accurate
  between network refreshes without extra API calls.

## API usage

`api.spot-hinta.fi` enforces roughly 1 request/minute per IP (max
1,440/day) on most endpoints, including `/JustNow`. Since `/JustNow` only
returns a single price value per call (current, or one hourly average via
`lookForwardHours`), fetching current + 4 distinct forecast hours would
require 5 rapid calls, risking 429s / a temporary ban.

Instead, the plugin makes **one** HTTP call per refresh:

```
GET https://api.spot-hinta.fi/TodayAndDayForward?priceResolution=15
```

This returns a JSON array of `priceModel` entries (`DateTime`, `PriceNoTax`,
`PriceWithTax`, `Rank`) covering today (and tomorrow, once published) at
15-minute resolution. `TodayAndDayForward` is used (rather than `/Today`
alone) so the 4-hour-ahead window keeps working correctly near midnight.

From the response, the plugin keeps the entries spanning from "now" through
"now + 4 hours" in memory, tagged with the fetch time.

This satisfies the "use spot-hinta.fi, get price per 15 minutes, load
current + next 4 hours" requirements while staying well under the rate
limit, since a full refresh only happens on startup, every 4 hours, or on a
user button press.

## Refresh timing

- **`OnWillAppear`** (plugin/button becomes visible): fetch immediately,
  populate the cache, start a background loop.
- **Background loop** ticks every ~30 seconds:
  - Finds the cached entry whose `[DateTime, DateTime + 15min)` range
    contains "now" and updates the displayed price. This is a local,
    in-memory lookup — no network call.
  - If 4 hours have passed since the last successful fetch, triggers a full
    re-fetch (replacing the whole cached window).
  - If "now" falls outside the entire cached window (e.g. the machine was
    asleep), triggers an immediate re-fetch as a safety net.
- **Button press** (`OnTap` / `OnLongPress`, now identical): triggers an
  immediate full re-fetch, refreshes the display, and resets the 4-hour
  timer so the next automatic refresh is 4 hours after the press.

## Display

- **Title**: current price only, fixed at 2 decimals, no unit (e.g. `4.52`).
- **Icon**: a solid-color square image generated at runtime (via
  `System.Drawing.Common`, encoded as a base64 PNG data URI and passed to
  `SetImageAsync`). The color is chosen by comparing the displayed price to
  the user's configured thresholds:
  - `price <= LowPriceThreshold` → `LowPriceColor`
  - `price >= HighPriceThreshold` → `HighPriceColor`
  - otherwise → `MediumPriceColor`
  - A neutral gray icon is used while a value has never successfully loaded
    (`unknown` state). While a network refresh is in-flight, the last known
    price/icon remains displayed (no "updating" flicker) so a slow or failed
    request doesn't blank the button.
- On fetch failure: log the error, store it in `ErrorMessage` (visible in
  the property inspector), and keep showing the last known good price. If
  there has never been a successful fetch, show the neutral/unknown icon
  and no price text.

## Settings model

```csharp
public class SahkonhintaSettingsModel
{
    public double LowPriceThreshold { get; set; } = 5;
    public double HighPriceThreshold { get; set; } = 15;
    public string LowPriceColor { get; set; } = "#2ecc71";
    public string MediumPriceColor { get; set; } = "#f1c40f";
    public string HighPriceColor { get; set; } = "#e74c3c";
    public string ErrorMessage { get; set; }
}
```

Removed: `Decimals`, `TapAction`, `LongPressAction`,
`UpdateStatusEverySecond`, and the `KeyPressAction` / `StatusUpdateFrequency`
enums (all unused after this change).

## Property inspector

Minimal UI with:
- Low price threshold (number, c/kWh)
- High price threshold (number, c/kWh)
- Low / Medium / High color pickers (native `<input type="color">`, no
  external JS dependency needed)
- Read-only error message display

Removed: decimals field, tap/long-press action selects, update-frequency
select.

## Components touched

- `Services/ElectricPriceService.cs` — rewritten to call
  `TodayAndDayForward` on `api.spot-hinta.fi`, parse the response, and
  expose the "current + next 4h" cache/window logic.
- A small icon-rendering helper (new file, e.g.
  `Services/IconRenderer.cs`) — generates the solid-color PNG data URI.
- `Models/SahkonhintaSettingsModel.cs` — simplified as above.
- `BaseAction.cs` — background loop changes from a settings-driven interval
  to a fixed ~30s tick; `OnWillAppear` always starts the loop (no "Never"
  option anymore).
- `SahkonhintaPriceAction.cs` — `OnTap`/`OnLongPress` both trigger an
  immediate forced refresh; `UpdateDisplay` implements the cache-or-fetch
  and icon/title rendering logic described above.
- `property_inspector/property_inspector.html` (+ its JS) — simplified
  fields as described above.
- `manifest.json` — unchanged UUID/category; description/tooltip text
  updated to mention spot-hinta.fi.
- `README.md` — updated to describe the new data source, behavior, and
  configuration options.

## Testing

- Manual verification: build the plugin, confirm it loads a price on
  startup, confirm button press forces a refresh, confirm color changes
  when adjusting thresholds/colors in the property inspector.
- Unit-test-friendly seams: the price-window parsing/selection logic (given
  a list of entries and "now", pick the current entry; determine whether a
  refresh is due) should be implemented as pure, testable methods separate
  from the HTTP call and Stream Deck manager calls.

## Note on repository state

This working copy is not currently a git repository, so the design doc
above could not be committed as part of this change; it is saved to disk
only.
