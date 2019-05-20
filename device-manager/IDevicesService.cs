using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace device_manager
{
    public interface IDevicesService
    {
        Task<string> AddAsync(string deviceId);

        Task<string> GetAsync(string deviceId);

        Task<IEnumerable<string>> QueryAsync(string sql);

    


    }
}
