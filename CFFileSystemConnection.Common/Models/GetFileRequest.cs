namespace CFFileSystemConnection.Models
{
    public class GetFileRequest : MessageBase
    {
        public string Path { get; set; } = String.Empty;
    }
}
