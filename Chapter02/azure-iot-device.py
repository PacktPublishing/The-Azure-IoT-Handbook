import sys
import asyncio
import datetime
import json
import random
import time

from azure.iot.device.aio import IoTHubDeviceClient

async def on_reboot(method_request):
    try:
        print("Rebooting!")
    except Exception as ex:
        print(f"Error in sample: {ex}")
    
    result = {"result": "Reboot started."}
    return result, 200

async def send_device_to_cloud_messages_async(device_client):
    min_temperature = 20
    min_humidity = 60
    rand = random.Random()

    while True:
        current_temperature = min_temperature + rand.random() * 15
        current_humidity = min_humidity + rand.random() * 20

        telemetry_data = {
            "temperature": current_temperature,
            "humidity": current_humidity
        }

        data = json.dumps(telemetry_data)
       
        await device_client.send_message(data)
        print(f"{datetime.datetime.now()} > Sending message: {data}")

        await asyncio.sleep(1)


def main(connection_string):
    print("IoT Hub Python Simulated Device. Ctrl-C to exit.\n")

    device_client = IoTHubDeviceClient.create_from_connection_string(connection_string)

    device_client.on_method_request_received = on_reboot

    asyncio.run(send_device_to_cloud_messages_async(device_client))

if __name__ == "__main__":
    # Replace '<connection string>' with your actual connection string
    connection_str = "<connection string>"
    main(connection_str)
