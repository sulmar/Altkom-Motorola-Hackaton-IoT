using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.EventHubs;

namespace iot_hub_client
{
    class Program
    {
        static async Task Main(string[] args)
        { 
            IotHubService ioTHubservice = new IotHubService();

            await ioTHubservice.SetVolume("Marcin-Device", 50);

            Console.WriteLine("Monitoring. Press Enter key to exit.\n");
            CancellationTokenSource cts = new CancellationTokenSource();

            EventHubService eventHubService = new EventHubService();
            eventHubService.ReceiveMessagesFromDeviceAsync(cts.Token);

             Console.ReadLine();
             Console.WriteLine("Exiting...");
             cts.Cancel();
        }
    }

    class EventHubService
    { 
        static string connectionString = "Endpoint=sb://iothub-ns-hackatonio-1662501-be90d2b51e.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=75rcslOYtdBdIyDgpuykJe2EHHzo+rCfv/ZddmEdVKI=;EntityPath=hackatoniothub1"; 
        static string monitoringEndpointName = "messages/events";

        // dotnet add package Microsoft.Azure.EventHubs
        private readonly EventHubClient eventHubClient;

        public EventHubService(EventHubClient eventHubClient)
        {
            this.eventHubClient = eventHubClient;
        }

        public EventHubService()
            : this(EventHubClient.CreateFromConnectionString(connectionString))
        {
        }
        

        public async Task ReceiveMessagesFromDeviceAsync(CancellationToken cancellationToken = default)
        {
            var information = await eventHubClient.GetRuntimeInformationAsync();

            var tasks = new List<Task>();
            
            foreach (string partition in information.PartitionIds)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cancellationToken));
            }

        }

        private async Task ReceiveMessagesFromDeviceAsync(string partitionId, CancellationToken cancellationToken = default)  
        {  
            var receiver = eventHubClient.CreateReceiver(PartitionReceiver.DefaultConsumerGroupName, partitionId, EventPosition.FromStart());

            var ehEvents = await receiver.ReceiveAsync(100);

            // ReceiveAsync can return null if there are no messages
            if (ehEvents != null)
            {
                // Since ReceiveAsync can return more than a single event you will need a loop to process
                foreach (var ehEvent in ehEvents)
                {
                    // Decode the byte array segment
                    var message = UnicodeEncoding.UTF8.GetString(ehEvent.Body.Array);
                    // Load the custom property that we set in the send example
                    var customType = ehEvent.Properties["Type"];
                    // Implement processing logic here

                     Console.WriteLine($"Message received. Partition: {partitionId} Data: '{message}'");
                }
            }       

           
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

        // Understand and invoke direct methods from IoT Hub
        // https://docs.microsoft.com/bs-latn-ba/azure/iot-hub/iot-hub-devguide-direct-methods
        public async Task SetVolume(string deviceId, byte volume)
        { 
            CloudToDeviceMethod method = new CloudToDeviceMethod(nameof(SetVolume), TimeSpan.FromSeconds(10));
            method.SetPayloadJson(volume.ToString());

            System.Console.WriteLine($"Request {method.MethodName} {method.GetPayloadAsJson()}");

            var response = await serviceClient.InvokeDeviceMethodAsync(deviceId, method);
         
            var json = response.GetPayloadAsJson();

            System.Console.WriteLine($"Response {json}");
        }

    }
}
