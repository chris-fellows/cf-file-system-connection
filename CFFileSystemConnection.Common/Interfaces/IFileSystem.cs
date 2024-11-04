using CFFileSystemConnection.Models;

namespace CFFileSystemConnection.Interfaces
{
    /// <summary>
    /// File system interface
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        /// Gets folder details
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <param name="getFiles">Whether to list files</param>
        /// <param name="recursive">Whether to list sub-folders</param>
        /// <returns></returns>
        FolderObject? GetFolder(string path, bool getFiles, bool recursive);

        /// <summary>
        /// Gets file details
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns></returns>
        FileObject? GetFile(string path);

        /// <summary>
        /// Gets file content
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns></returns>
        byte[]? GetFileContent(string path);
    }
}
