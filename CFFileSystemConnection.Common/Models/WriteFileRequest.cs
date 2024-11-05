using CFFileSystemConnection.Models;

namespace CFFileSystemConnection.Models
{
    public class WriteFileRequest : MessageBase
    {
        public string SessionId { get; set; } = String.Empty;

        public FileObject FileObject { get; set; } 

        public byte[] Content { get; set; } = new byte[0];
        
        public bool IsMore { get; set; }
    }
}
