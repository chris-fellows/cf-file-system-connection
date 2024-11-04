using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.Enums
{
    public enum ResponseErrorCodes
    {
        Unknown,        
        DirectoryDoesNotExist,
        FileDoesNotExist,
        FileSystemError,
        PermissionDenied    // Invalid security key
    }
}
