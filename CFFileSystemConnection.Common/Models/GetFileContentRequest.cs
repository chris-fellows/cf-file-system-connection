﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.Models
{
    public class GetFileContentRequest : MessageBase
    {
        public string Path { get; set; } = String.Empty;

        public int SectionBytes { get; set; } = 1024 * 1000;    // Multiple response messages
    }
}
