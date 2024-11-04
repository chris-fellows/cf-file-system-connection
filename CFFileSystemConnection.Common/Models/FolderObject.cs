using System.Text.Json.Serialization;

namespace CFFileSystemConnection.Models
{
    /// <summary>
    /// Folder object
    /// </summary>
    public class FolderObject   //: FileSystemItem
    {
        public string Name { get; set; } = String.Empty;

        public string Path { get; set; } = String.Empty;

        public long Length { get; set; }

        //public List<FileSystemItem> Items { get; set; } = new List<FileSystemItem>();       
        public List<FolderObject> Folders { get; set; } = new List<FolderObject>();

        public List<FileObject> Files { get; set; } = new List<FileObject>();

        public string UnixFileMode { get; set; } = String.Empty;

        /// <summary>
        /// Details of any errors reading folder
        /// </summary>
        public FolderErrors? Errors { get; set; } 
    }
}
