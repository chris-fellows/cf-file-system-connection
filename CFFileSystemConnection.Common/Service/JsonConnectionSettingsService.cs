using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.Service
{
    public class JsonConnectionSettingsService : JsonEntityWithIdService<ConnectionSettings>, IConnectionSettingsService
    {
        public JsonConnectionSettingsService(string folder) : base(folder, (connectionSettings) => connectionSettings.Id)
        {

        }
    }
}
