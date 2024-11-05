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
