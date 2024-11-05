using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.Models
{
    public class MoveRequest : MessageBase
    {
        public string OldPath { get; set; } = String.Empty;

        public string NewPath { get; set; } = String.Empty;
    }
}
