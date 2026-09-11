// global websocket, used to communicate from/to Stream Deck software
// as well as some info about our plugin, as sent by Stream Deck software 
var websocket = null,
    uuid = null,
    inInfo = null,
    actionInfo = {},
    settingsModel = {};

function connectElgatoStreamDeckSocket(inPort, inUUID, inRegisterEvent, inInfo, inActionInfo) {
    uuid = inUUID;
    actionInfo = JSON.parse(inActionInfo);
    inInfo = JSON.parse(inInfo);
    websocket = new WebSocket('ws://localhost:' + inPort);

    //initialize values
    if (actionInfo.payload.settings.settingsModel) {
        settingsModel.LowPriceThreshold = actionInfo.payload.settings.settingsModel.LowPriceThreshold;
        settingsModel.HighPriceThreshold = actionInfo.payload.settings.settingsModel.HighPriceThreshold;
        settingsModel.LowPriceColor = actionInfo.payload.settings.settingsModel.LowPriceColor;
        settingsModel.MediumPriceColor = actionInfo.payload.settings.settingsModel.MediumPriceColor;
        settingsModel.HighPriceColor = actionInfo.payload.settings.settingsModel.HighPriceColor;
        settingsModel.ErrorMessage = actionInfo.payload.settings.settingsModel.ErrorMessage;
    } else {
        settingsModel.LowPriceThreshold = 5;
        settingsModel.HighPriceThreshold = 15;
        settingsModel.LowPriceColor = "#2ecc71";
        settingsModel.MediumPriceColor = "#f1c40f";
        settingsModel.HighPriceColor = "#e74c3c";
        settingsModel.ErrorMessage = "";
    }

    document.getElementById('txtLowPriceThreshold').value = settingsModel.LowPriceThreshold;
    document.getElementById('txtHighPriceThreshold').value = settingsModel.HighPriceThreshold;
    document.getElementById('txtLowPriceColor').value = settingsModel.LowPriceColor;
    document.getElementById('txtMediumPriceColor').value = settingsModel.MediumPriceColor;
    document.getElementById('txtHighPriceColor').value = settingsModel.HighPriceColor;
    document.getElementById('error_message').innerHTML = settingsModel.ErrorMessage;

    websocket.onopen = function () {
        var json = { event: inRegisterEvent, uuid: inUUID };
        // register property inspector to Stream Deck
        websocket.send(JSON.stringify(json));
    };

    websocket.onmessage = function (evt) {
        // Received message from Stream Deck
        var jsonObj = JSON.parse(evt.data);
        var sdEvent = jsonObj['event'];
        switch (sdEvent) {
            case "didReceiveSettings":
                if (jsonObj.payload.settings.settingsModel.LowPriceThreshold !== undefined) {
                    settingsModel.LowPriceThreshold = jsonObj.payload.settings.settingsModel.LowPriceThreshold;
                    document.getElementById('txtLowPriceThreshold').value = settingsModel.LowPriceThreshold;
                }
                if (jsonObj.payload.settings.settingsModel.HighPriceThreshold !== undefined) {
                    settingsModel.HighPriceThreshold = jsonObj.payload.settings.settingsModel.HighPriceThreshold;
                    document.getElementById('txtHighPriceThreshold').value = settingsModel.HighPriceThreshold;
                }
                if (jsonObj.payload.settings.settingsModel.LowPriceColor !== undefined) {
                    settingsModel.LowPriceColor = jsonObj.payload.settings.settingsModel.LowPriceColor;
                    document.getElementById('txtLowPriceColor').value = settingsModel.LowPriceColor;
                }
                if (jsonObj.payload.settings.settingsModel.MediumPriceColor !== undefined) {
                    settingsModel.MediumPriceColor = jsonObj.payload.settings.settingsModel.MediumPriceColor;
                    document.getElementById('txtMediumPriceColor').value = settingsModel.MediumPriceColor;
                }
                if (jsonObj.payload.settings.settingsModel.HighPriceColor !== undefined) {
                    settingsModel.HighPriceColor = jsonObj.payload.settings.settingsModel.HighPriceColor;
                    document.getElementById('txtHighPriceColor').value = settingsModel.HighPriceColor;
                }
                if (jsonObj.payload.settings.settingsModel.ErrorMessage) {
                    settingsModel.ErrorMessage = jsonObj.payload.settings.settingsModel.ErrorMessage;
                    document.getElementById('error_message').innerHTML = settingsModel.ErrorMessage;
                }
                break;
            default:
                break;
        }
    };
}

const setSettings = (value, param) => {
    if (websocket) {
        settingsModel[param] = value;
        var json = {
            "event": "setSettings",
            "context": uuid,
            "payload": {
                "settingsModel": settingsModel
            }
        };
        websocket.send(JSON.stringify(json));
    }
};
