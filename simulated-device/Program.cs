using System;
using System.Threading.Tasks;

namespace simulated_device
{
    class Program
    {

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello IoT!");
            System.Console.WriteLine("Press Enter to exit.");

            Device device = new Device();

            await device.Init();

            await device.ReportConnectity();

            await device.SetHandlers();

            device.ReceiveCloudToDeviceMessagesAsync();

            await device.SendDeviceToCloudMessageAsync();

          //  await device.SendDeviceToCloudBlobAsync("filename.jpg");

            Console.ReadLine();
        }

        // C# =< 7.1
       // static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();
            
       
    }
}
