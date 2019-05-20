using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace device_manager
{

    // dotnet add package Microsoft.Azure.Devices
    public class IotHubDevicesService : IDevicesService
    {
        private readonly RegistryManager registryManager;
        static string connectionString = "HostName=HackatonIoTHub1.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=75rcslOYtdBdIyDgpuykJe2EHHzo+rCfv/ZddmEdVKI=";

        public IotHubDevicesService()
            : this(RegistryManager.CreateFromConnectionString(connectionString))
        {
                    
        }
        public IotHubDevicesService(RegistryManager registryManager)
        {
            this.registryManager = registryManager;
        }

        public async Task<string> AddAsync(string deviceId)
        {
             Device device = await registryManager.AddDeviceAsync(new Device(deviceId));

            return device.Authentication.SymmetricKey.PrimaryKey;
        }

        public async Task<string> GetAsync(string deviceId)
        {
            Device device = await registryManager.GetDeviceAsync(deviceId);

            return device.Authentication.SymmetricKey.PrimaryKey;
        }


        // IoT Hub query language
        // https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-query-language
        public async Task<IEnumerable<string>> QueryAsync(string sql)
        {
            ICollection<string> devices = new List<string>();
            
            var query = registryManager.CreateQuery(sql, pageSize: 100);

            while (query.HasMoreResults)
            {
                var page = await query.GetNextAsTwinAsync();
                foreach (var twin in page)
                {
                    devices.Add(twin.DeviceId);
                }
            }

            return devices;
        }

       
       


    }
}
