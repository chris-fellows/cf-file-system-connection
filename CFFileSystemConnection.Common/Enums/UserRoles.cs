using System.ComponentModel;

namespace CFFileSystemConnection.Enums
{
    public enum UserRoles
    {
        [Description("Read file system")]
        FileSystemRead,

        [Description("Write file system")]
        FileSystemWrite        
    }
}
