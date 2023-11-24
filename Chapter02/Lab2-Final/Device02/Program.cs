using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace Device02
{
    class Program
    {
        private static DeviceClient? deviceClient;
        private readonly static string connectionString = < 'connection string' >;
        private static void Main(string[] args)
        {
            Console.WriteLine("IoT Hub C# Simulated Device. Ctrl-C to exit.\n");
            deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt);
            deviceClient.SetMethodHandlerAsync("reboot", onReboot, null).Wait();
            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }
        private static Task<MethodResponse> onReboot(MethodRequest methodRequest, object userContext)
        {
            try
            {
                Console.WriteLine("Rebooting!");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample:   {0}", ex.Message);
            }
            string result = @"{""result"":""Reboot started.""}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            // Create an instance of our sensor
            var sensor = new EnvironmentSensor();
            while (true)
            {
                // read data from the sensor
                var currentTemperature = sensor.ReadTemperature();
                var currentHumidity = sensor.ReadHumidity();
                var messageString = CreateMessageString(currentTemperature, currentHumidity);
                // create a byte array from the message string using ASCII encoding
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");
                // Send the telemetry message
                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
                await Task.Delay(1000);
            }
        }
        private static string CreateMessageString(double temperature, double humidity)
        {
            // Create an anonymous object that matches the data structure we wish to send
            var telemetryDataPoint = new
            {
                temperature = temperature,
                humidity = humidity
            };
            // Create a JSON string from the anonymous object
            return JsonConvert.SerializeObject(telemetryDataPoint);
        }
    }

    internal class EnvironmentSensor
    {
        // Initial telemetry values
        double minTemperature = 20;
        double minHumidity = 60;
        Random rand = new Random();
        internal EnvironmentSensor()
        {
            // device initialization could occur here
        }
        internal double ReadTemperature()
        {
            return minTemperature + rand.NextDouble() * 15;
        }
        internal double ReadHumidity()
        {
            return minHumidity + rand.NextDouble() * 20;
        }
    }
}
