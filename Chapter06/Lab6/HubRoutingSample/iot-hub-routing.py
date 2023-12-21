import asyncio
import json
import random
import signal
import sys
import time
from azure.iot.device.aio import IoTHubDeviceClient
from azure.iot.device import Message

# Simulated device parameters
min_temperature = 20
min_humidity = 60
level = ""


async def send_device_to_cloud_messages(device_client):
    async def send_message():

        while True:
            current_temperature = min_temperature + random.random() * 15
            current_humidity = min_humidity + random.random() * 20

            if random.random() > 0.7:
                if random.random() > 0.5:
                    level = "critical"
                    info_string = "This is a critical message."
                else:
                    level = "storage"
                    info_string = "This is a storage message."
            else:
                level = "normal"
                info_string = "This is a normal message."

            telemetry_data_point = {
                "temperature": current_temperature,
                "humidity": current_humidity,
                "pointInfo": info_string,
                "level": level
            }

            telemetry_data_string = json.dumps(telemetry_data_point)
            message = Message(telemetry_data_string, content_encoding="utf-8", content_type="application/json")
            message.custom_properties["level"] = level

            try:
                await device_client.send_message(message)
                print(f"{time.strftime('%Y-%m-%d %H:%M:%S')} > Sent message: {telemetry_data_string}")
            except asyncio.CancelledError:
                break

            await asyncio.sleep(1)

    return await send_message()


async def main(connection_string):
    try:
        device_client = IoTHubDeviceClient.create_from_connection_string(connection_string)


        print("Routing Tutorial: Simulated device\n")
        print("Press Control+C at any time to quit the sample.\n")

        await send_device_to_cloud_messages(device_client)

    except Exception as ex:
        print(f"Exception: {ex}")
        sys.exit(1)


if __name__ == "__main__":
    connection_str = "YOUR_PRIMARY_CONNECTION_STRING_HERE"
    asyncio.run(main(connection_str))

