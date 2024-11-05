using CFFileSystemConnection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.Models
{
    public class WriteFileResponse : MessageBase
    {
        public string SessionId { get; set; } = String.Empty;
    }
}
