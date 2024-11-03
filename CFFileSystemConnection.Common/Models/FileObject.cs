using System.Text.Json.Serialization;

namespace CFFileSystemConnection.Models
{
    /// <summary>
    /// File object
    /// </summary>
    public class FileObject     //: FileSystemItem
    {
        public string Name { get; set; } = String.Empty;

        public long Length { get; set; }
    }
}
