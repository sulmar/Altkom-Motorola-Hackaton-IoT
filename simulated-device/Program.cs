using System;
using System.Threading;
using System.Threading.Tasks;

namespace simulated_device
{
    class Program
    {

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello IoT!");
            System.Console.WriteLine("Press Ctrl+C to exit.");

            CancellationTokenSource cts = new CancellationTokenSource();  

            Device device = new Device();

            await device.Init();

            await device.ReportConnectity();

            await device.SetHandlers();

            device.ReceiveCloudToDeviceMessagesAsync(cts.Token);

            await device.SendDeviceToCloudMessageAsync(cts.Token);

            System.Console.CancelKeyPress += (s, e) =>  
            {  
                e.Cancel = true;  
                cts.Cancel();  
                Console.WriteLine("Exiting...");  
            };  
  
           // await device.SendDeviceToCloudBlobAsync("filename.jpg", cts.Token);

            
        }

        // C# =< 7.1
       // static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();
            
       
    }
}
