

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Microsoft.Azure.Devices.Client.Samples
{
    /// <summary>
    /// This is the code that sends messages to the IoT Hub for testing the routing as defined
    ///
    /// This program encodes the message body so it can be queried against by the Iot hub.
    ///
    /// </summary>
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Parse application parameters
            Parameters parameters = null;
            ParserResult<Parameters> result = Parser.Default.ParseArguments<Parameters>(args)
                .WithParsed(parsedParams =>
                {
                    parameters = parsedParams;
                })
                .WithNotParsed(errors =>
                {
                    Environment.Exit(1);
                });

            
            {
                // Send messages to the simulated device. Each message will contain a randomly generated
                //   Temperature and Humidity.
                // The "level" of each message is set randomly to "storage", "critical", or "normal".
                // The messages are routed to different endpoints depending on the level, temperature, and humidity.
                
                Console.WriteLine("Routing Tutorial: Simulated device\n");
                using var deviceClient = DeviceClient.CreateFromConnectionString(parameters.PrimaryConnectionString);

                using var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    cts.Cancel();
                    Console.WriteLine("Sample execution cancellation requested; will exit.");
                };

                Console.WriteLine($"Press Control+C at any time to quit the sample.");

                try
                {
                    await SendDeviceToCloudMessagesAsync(deviceClient, cts.Token);
                }
                catch (OperationCanceledException) { }
                await deviceClient.CloseAsync(cts.Token);
            }
        }

        /// <summary>
        /// Send message to the Iot hub. This generates the object to be sent to the hub in the message.
        /// </summary>
        private static async Task SendDeviceToCloudMessagesAsync(DeviceClient deviceClient, CancellationToken token)
        {
            double minTemperature = 20;
            double minHumidity = 60;
            var rand = new Random();

            while (!token.IsCancellationRequested)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                string infoString;
                string levelValue;

                if (rand.NextDouble() > 0.7)
                {
                    if (rand.NextDouble() > 0.5)
                    {
                        levelValue = "critical";
                        infoString = "This is a critical message.";
                    }
                    else
                    {
                        levelValue = "storage";
                        infoString = "This is a storage message.";
                    }
                }
                else
                {
                    levelValue = "normal";
                    infoString = "This is a normal message.";
                }

                var telemetryDataPoint = new
                {
                    temperature = currentTemperature,
                    humidity = currentHumidity,
                    pointInfo = infoString
                };
                // serialize the telemetry data and convert it to JSON.
                string telemetryDataString = JsonConvert.SerializeObject(telemetryDataPoint);

                // Encode the serialized object using UTF-8 so it can be parsed by IoT Hub when
                // processing messaging rules.
                using var message = new Message(Encoding.UTF8.GetBytes(telemetryDataString))
                {
                    ContentEncoding = "utf-8",
                    ContentType = "application/json",
                };

                // Add one property to the message.
                message.Properties.Add("level", levelValue);

                try
                {
                    // Submit the message to the hub.
                    await deviceClient.SendEventAsync(message, token);

                    // Print out the message.
                    Console.WriteLine("{0} > Sent message: {1}", DateTime.UtcNow, telemetryDataString);
                }
                catch (OperationCanceledException) { }

                try
                {
                    await Task.Delay(1000, token);
                }
                catch (OperationCanceledException) { }
            }
        }

      
    }
}
