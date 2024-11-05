using CFFileSystemConnection.Models;

namespace CFFileSystemConnection.Interfaces
{
    /// <summary>
    /// File system interface
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        /// Gets drives
        /// </summary>
        /// <returns></returns>
        List<DriveObject> GetDrives();

        /// <summary>
        /// Gets folder details. Always gets list of immediate sub-folders.
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

        ///// <summary>
        ///// Gets file content
        ///// </summary>
        ///// <param name="path">Path to check</param>
        ///// <returns></returns>
        //byte[]? GetFileContent(string path);

        /// <summary>
        /// Gets file content by section. This is to enable handling of large files.
        /// 
        /// If exception is thrown then complete file was not received and caller should discard all file sections
        /// received so far.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <param name="sectionBytes">Size of each section (Bytes)</param>
        /// <param name="actionSection">Action for each section (Values: Section bytes, Is more sections)</param>
        /// <returns>Whether success</returns>
        void GetFileContentBySection(string path, int sectionBytes, Action<byte[], bool> actionSection);

        /// <summary>
        /// Writes file content by section. This is to enable handling of large files.
        /// </summary>
        /// <param name="fileObject"></param>
        /// <param name="getSectionFunction"></param>
        void WriteFileContentBySection(FileObject fileObject, Func<Tuple<byte[], bool>> getSectionFunction);

        /// <summary>
        /// Deletes file
        /// </summary>
        /// <param name="path"></param>
        void DeleteFile(string path);

        /// <summary>
        /// Deletes folder
        /// </summary>
        /// <param name="path"></param>
        void DeleteFolder(string path);

        /// <summary>
        /// Move file
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        void MoveFile(string oldPath, string newPath);

        /// <summary>
        /// Move folder
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        void MoveFolder(string oldPath, string newPath);
    }
}
