using System;
using System.Collections.Generic;
namespace CFFileSystemConnection.Models
{
    public class GetDrivesResponse : MessageBase
    {
        public List<DriveObject>? Drives { get; set; }
    }
}
