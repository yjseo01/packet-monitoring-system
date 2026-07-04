# packet-monitoring-system

`PCapture` captures network packets and publishes them over MQTT, while `SubscriberWebApp` receives the messages, stores them in SQLite, and displays them in a Blazor UI.

## Run Order

1. Start an MQTT broker on `localhost:1883`.
2. Run `SubscriberWebApp`.
3. Run `PCapture` and select the NIC you want to capture from.
4. Open the `/modbustcp` page in the browser.

## Web App URLs

- `http://localhost:5034`
- `https://localhost:7198`

The default HTTP port in `launchSettings.json` is `5034`.

## Database

- The web app uses `modbus.db`.
- If the table does not exist on first run, it is created automatically with `EnsureCreated()`.

## MQTT Behavior

- `PCapture` publishes to `ModbusTCP/{functionCode}`.
- `SubscriberWebApp` subscribes to `ModbusTCP/+`.
- The current payload is a JSON string containing captured packet metadata.

## Project Layout

- `PCapture`: packet capture and MQTT publishing
- `SubscriberWebApp`: MQTT subscription, SQLite storage, and Blazor query UI

## Notes

- `PCapture` prints the available NIC list at startup.
- `SubscriberWebApp` starts the MQTT subscriber in the background when the app runs.
