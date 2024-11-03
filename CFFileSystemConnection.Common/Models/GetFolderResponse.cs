using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.Models
{
    public class GetFolderResponse : MessageBase
    {                
        public FolderObject? FolderObject { get; set; }
    }
}
