using System.ComponentModel;

namespace CFFileSystemConnection.Enums
{
    public enum ResponseErrorCodes
    {
        [Description("Unknown")]
        Unknown,
        [Description("Directory does not exist")]
        DirectoryDoesNotExist,        
        [Description("File does not exist")]
        FileDoesNotExist,
        [Description("File system error")]
        FileSystemError,
        [Description("Folder already exists")]
        FolderAlreadyExists,
        [Description("Permission denied")]
        PermissionDenied   
    }
}
