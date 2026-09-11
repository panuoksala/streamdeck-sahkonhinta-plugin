[![.NET](https://github.com/panuoksala/streamdeck-sahkonhinta-plugin/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/panuoksala/streamdeck-sahkonhinta-plugin/actions/workflows/dotnet.yml)

# Pörssisähkön hinta (Finnish electricity price) plugin

Source code of a Finnish electricity spot price plugin by Panu Oksala for [Elgato Stream Deck][Stream Deck]. This project works currently only on Windows devices.
Project is created by using the [Stream Deck C# Toolkit][Stream Deck C# Toolkit Homepage].
Use GitHub issues to submit any bugs / feature requests.

## How to use this plugin

Add the "Pörssisähkön hinta" button to your Stream Deck. The button shows the current Finnish
electricity spot price (c/kWh, 15-minute resolution) and changes color based on the configured
low/high price thresholds.

Prices are fetched from the free [spot-hinta.fi API](https://api.spot-hinta.fi/swagger.json)
(`TodayAndDayForward`, 15-minute resolution):

* On startup, the plugin loads the current price and the following 4 hours.
* That 4-hour window is automatically refreshed every 4 hours.
* Pressing the button immediately refreshes the current price and next 4 hours.

Only one price value is displayed at a time (the current one); the forecast window is kept in
memory so the display stays accurate between refreshes without extra API calls (spot-hinta.fi
limits most endpoints to about 1 request/minute per IP address).

### Configurations

| Setting                   | Description|
|---------------------------|------------|
| Low price threshold       |Price (c/kWh) at or below which the icon uses the "low" color.|
| High price threshold      |Price (c/kWh) at or above which the icon uses the "high" color.|
| Low price color           |Icon color used when the price is at or below the low threshold.|
| Medium price color        |Icon color used when the price is between the low and high thresholds.|
| High price color          |Icon color used when the price is at or above the high threshold.|
| Errors                    |Shows the possible error message that is received when the button action is invoked.|

## Contribution guide

1. Install Stream Deck application
2. Clone the repository
3. Build with Visual Studio
4. Visual Studio should automatically add the Sahkonhinta button into your Stream Deck app / device.
5. To debug the app just run it and attach debugger into StreamDeck application (you might have two so try both).
6. If you experience problems try to run the Visual Studio in Administrator mode.
7. Build/Rebuild will stop StreamDeck app, so start it manually after every build. It will reload changes automatically.

If you have problems to build plugin after debugging, quick fix is to restart Visual Studio.

## References

* [Stream Deck C# Toolkit Homepage](https://github.com/FritzAndFriends/StreamDeckToolkit)
* [Stream Deck Page][Stream Deck]
* [Stream Deck SDK Documentation][Stream Deck SDK]
* [spot-hinta.fi API](https://api.spot-hinta.fi/swagger.json)

<!-- References -->
[Stream Deck]: https://www.elgato.com/en/gaming/stream-deck "Elgato's Stream Deck landing page for the hardware, software, and SDK"
[Stream Deck C# Toolkit Homepage]: https://github.com/FritzAndFriends/StreamDeckToolkit "C# Stream Deck library"
[Stream Deck software]: https://www.elgato.com/gaming/downloads "Download the Stream Deck software"
[Stream Deck SDK]: https://developer.elgato.com/documentation/stream-deck "Elgato's online SDK documentation"
[Style Guide]: https://developer.elgato.com/documentation/stream-deck/sdk/style-guide/ "The Stream Deck SDK Style Guide"
[Manifest file]: https://developer.elgato.com/documentation/stream-deck/sdk/manifest "Definition of elements in the manifest.json file"
