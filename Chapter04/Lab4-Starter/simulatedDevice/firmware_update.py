from azure.iot.device import IoTHubDeviceClient
from azure.iot.device import MethodResponse
from azure.iot.hub.models import Twin, TwinProperties
import asyncio
import json
import datetime

class SimulatedDevice:
    def __init__(self):
        self.device_connection_string = "INPUT_YOUR_DEVICE_STRING_HERE"
        self.device_id = "device01"
        self.device_fw_version = "1.0.0"
        self.fw_properties = ""
        self.device_update = False
  
    def log_to_console(self, text):
        print(f"{self.device_id}: {text}")
    
    def get_firmware_version(self):
        return self.device_fw_version
           
    def trigger_update(self):
        self.device_update = True
    
 
    
    def update_firmware(self, fw_version, fw_package_uri, fw_package_check_value):
        self.log_to_console(f"A firmware update was requested from version {self.get_firmware_version()} to version {fw_version}")

        #Below you can include the code to dowload, verify, and install the new firmware. After that you can reboot the device if needed.     
        self.log_to_console(f"Downloading new firmware package from {fw_package_uri}")
        self.log_to_console("The new firmware package has been successfully downloaded.")
        self.log_to_console(f"Verifying firmware package with checksum {fw_package_check_value}")
        self.log_to_console("The new firmware binary package has been successfully verified")
        self.log_to_console("Applying new firmware")
        self.log_to_console("Rebooting")
       
        self.log_to_console("Updating reported properties")
        reported_properties = {"currentFwVersion":fw_version}
        #Updating currentFwVersion in twin reported properties
        self.device_client.patch_twin_reported_properties(reported_properties)
       


    def on_desired_property_changed(self, patch):
        self.log_to_console("Desired property changed:")
        print("the data in the desired properties patch was: {}".format(patch))
      
        if "firmware" in patch:
            # Start the firmware update process
            print("Starting firmware update...")
            fw_properties = patch["firmware"]
            print(fw_properties["fwVersion"])
            print(fw_properties["fwPackageURI"])
            print(fw_properties["fwPackageCheckValue"])
            self.trigger_update()
            self.update_firmware(fw_properties["fwVersion"], fw_properties["fwPackageURI"],fw_properties["fwPackageCheckValue"])
         

    async def init_device(self):
        self.log_to_console("Device booted")
        self.log_to_console("Current firmware version: " + self.get_firmware_version())
     

    async def main(self):
        try:
           await self.init_device()
           self.device_client = IoTHubDeviceClient.create_from_connection_string(self.device_connection_string)
           print("Device client has been created")
           # connect the client.
           self.device_client.connect()
           print("Client connected")
        except Exception as e:
           print(f"Error creating DeviceClient: {e}")
           # Handle the error or add appropriate logging
        
                
        
        while True:
            # set the twin patch handler on the client
            self.device_client.on_twin_desired_properties_patch_received = self.on_desired_property_changed
            if (self.trigger_update==True): 
                self.device_fw_version = fw_version
                await self.init_device()
                self.trigger_update = False
            selection = input("Press Q to quit\n")
            if selection == "Q" or selection == "q":
                print("Quitting...")
                # Shut down the client
                self.device_client.shutdown()
                break


# Run the main loop
if __name__ == "__main__":
    device = SimulatedDevice()
    asyncio.run(device.main())
