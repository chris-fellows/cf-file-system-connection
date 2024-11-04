using System.Text.Json.Serialization;

namespace CFFileSystemConnection.Models
{
    /// <summary>
    /// File object
    /// </summary>
    public class FileObject     //: FileSystemItem
    {
        public string Name { get; set; } = String.Empty;

        public string Path { get; set; } = String.Empty;

        public long Length { get; set; }

        public DateTime CreatedTimeUtc { get; set; }

        public DateTimeOffset? UpdatedTimeUtc { get; set; }

        public bool ReadOnly { get; set; }

        public string Attributes { get; set; } = String.Empty;

        public string UnixFileMode { get; set; } = String.Empty;
    }
}
