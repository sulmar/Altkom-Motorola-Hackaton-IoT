using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

namespace simulated_device
{
    public class Device
    {
        // {your device connection string}
        private static string connectionString = "HostName=HackatonIoTHub1.azure-devices.net;DeviceId=Marcin-Device;SharedAccessKey=pBeE2yc+Mmg7WA7Wtfu1dzik1rBJzrkfUZ/VnvuKy4w=";

        private readonly DeviceClient deviceClient;

        private TimeSpan telemetryInterval = TimeSpan.FromSeconds(1);

        private byte volume;

        public Device()
        : this(DeviceClient.CreateFromConnectionString(connectionString))
        {

        }

        public Device(DeviceClient deviceClient)
        {
            this.deviceClient = deviceClient;
        }


        public async Task SetHandlers()
        {
             await deviceClient.SetMethodHandlerAsync(nameof(SetVolume), SetVolume, null);
        }

        public Task<MethodResponse> SetVolume(MethodRequest methhodRequest, object userContext)
        {
            string data = Encoding.UTF8.GetString(methhodRequest.Data);

            volume = byte.Parse(data);

            System.Console.BackgroundColor = ConsoleColor.Green;
            System.Console.ForegroundColor = ConsoleColor.White; 
            System.Console.WriteLine($"Set Volue {volume}");      

            var response = new
            {
                result = $"Executed direct method {methhodRequest.Name}"
            };

            System.Console.ResetColor();

            var result = JsonConvert.SerializeObject(response);

            MethodResponse methodResponse = new MethodResponse(Encoding.UTF8.GetBytes(result), 200);

            return Task.FromResult(methodResponse);

        }

        public async Task Init()
        {
            Twin twin = await deviceClient.GetTwinAsync();

            await OnDesiredPropertiesUpdate(twin.Properties.Desired, deviceClient);

            await deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, deviceClient);
            
        }

        public async Task ReportConnectity()
        {
            System.Console.WriteLine("Sending connectivity data as reported properties...");

            TwinCollection reportedProperties = new TwinCollection();
            TwinCollection connectivity = new TwinCollection();
            connectivity["type"] = "wifi";
            reportedProperties["connectivity"] = connectivity;

            await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);

            System.Console.WriteLine("Sent.");




        }

        public Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            if (desiredProperties["interval"] != null)
            {
                int interval = desiredProperties["interval"];

                System.Console.BackgroundColor = ConsoleColor.Yellow;
                System.Console.ForegroundColor = ConsoleColor.Black;
                System.Console.WriteLine($"Set interval {interval}");
                System.Console.ResetColor();

                telemetryInterval = TimeSpan.FromMilliseconds(interval);
            }

            return Task.CompletedTask;
        }


        public async Task SendDeviceToCloudMessageAsync()
        {
            double minTemperature = 20;
            double minHumidity = 60;

            Random random = new Random();

            while(true)
            {
                double currentTemperature = minTemperature + random.NextDouble() * 15;
                double currentHumidity = minHumidity + random.NextDouble() * 20;

                var telemetryDataPoint = new 
                {
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };

                string messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                await deviceClient.SendEventAsync(message);
              
                System.Console.WriteLine($"{DateTime.Now} > Sending message: {messageString}");

                await Task.Delay(telemetryInterval);



                
            }
            

        }
    }
}
