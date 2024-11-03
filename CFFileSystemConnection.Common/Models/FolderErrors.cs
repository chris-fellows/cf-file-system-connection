using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.Models
{
    public class FolderErrors
    {  
        /// <summary>
       /// Error reading details of folder
       /// </summary>
        public bool ErrorReading { get; set; }

        /// <summary>
        /// Error reading sub-folder
        /// </summary>
        public bool ErrorReadingSubFolders { get; set; }

        /// <summary>
        /// Error reading files
        /// </summary>
        public bool ErrorReadingFiles { get; set; }
    }
}
