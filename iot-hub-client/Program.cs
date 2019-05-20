using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace iot_hub_client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Iot Hub Client!");
            Console.WriteLine("Press Ctrl+C to exit.");
            
            IotHubService service = new IotHubService();

            await service.SetVolume(50);

        }
    }

    class IotHubService
    {
        static string connectionString = "HostName=HackatonIoTHub1.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=75rcslOYtdBdIyDgpuykJe2EHHzo+rCfv/ZddmEdVKI=";

         private readonly ServiceClient serviceClient;

          public IotHubService()
        : this(ServiceClient.CreateFromConnectionString(connectionString))
        {

        }

        public IotHubService(ServiceClient serviceClient)
        {
            this.serviceClient = serviceClient;
        }

        public async Task SetVolume(byte volume)
        {
            string deviceId = "Marcin-Device";
 
            CloudToDeviceMethod method = new CloudToDeviceMethod(nameof(SetVolume), TimeSpan.FromSeconds(10));
            method.SetPayloadJson(volume.ToString());

            System.Console.WriteLine($"Request {method.MethodName} {method.GetPayloadAsJson()}");

            var response = await serviceClient.InvokeDeviceMethodAsync(deviceId, method);

            var json = response.GetPayloadAsJson();

            System.Console.WriteLine($"Response {json}");
        }

    }
}
