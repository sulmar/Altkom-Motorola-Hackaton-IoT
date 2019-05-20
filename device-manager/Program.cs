using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace device_manager
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello Iot Hub Devices Manager!");
            
            IDevicesService devicesService = new IotHubDevicesService();

            string device = await devicesService.GetAsync("Marcin-Device");

            System.Console.WriteLine(device);

            // Search by properties desired
            var query1 = await devicesService.QueryAsync("SELECT * FROM devices where properties.desired.interval > 1000");

            Display(query1);

            // Search by properties reported
            var query2 = await devicesService.QueryAsync("SELECT * FROM devices where properties.reported.connectivity.type in ['wifi', 'wired']");            
            Display(query2);

            // Search by tag properties
            var query3 = await devicesService.QueryAsync("SELECT * FROM devices where tags.location.region = 'Poland' and tags.location.city = 'Krakow'");
            Display(query3);

        }

        static void Display(IEnumerable<string> devices)
        {
            foreach(var device in devices)
            {
                System.Console.WriteLine(device);
            }
        }


    }
}
