using CFConnectionMessaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.Models
{
    public class ConnectionSettings
    {
        public string Id { get; set; } = String.Empty;

        public string Name { get; set; } = String.Empty;

        public string SecurityKey { get; set; } = String.Empty;

        public EndpointInfo RemoteEndpoint { get; set; } = new EndpointInfo();

        public string PathDelimiter { get; set; } = String.Empty;
    }
}
