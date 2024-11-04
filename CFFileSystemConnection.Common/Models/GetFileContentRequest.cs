using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.Models
{
    public class GetFileContentRequest : MessageBase
    {
        public string Path { get; set; } = String.Empty;
    }
}
