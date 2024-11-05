using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemMobile.Models
{
    public class SelectableItem<TValueType>
    {
        public string Name { get; set; } = String.Empty;

        public TValueType? Value { get; set; }

        public bool Enabled { get; set; }
    }
}
