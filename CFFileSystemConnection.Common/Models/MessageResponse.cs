using CFFileSystemConnection.Enums;

namespace CFFileSystemConnection.Models
{
    /// <summary>
    /// Message response
    /// </summary>
    public class MessageResponse
    {   
        /// <summary>
        /// If response then Id of original message
        /// </summary>
        public string MessageId { get; set; } = String.Empty;

        /// <summary>
        /// If response then whether there are more response messages
        /// </summary>
        public bool IsMore { get; set; } = false;

        /// <summary>
        /// Response error code
        /// </summary>
        public ResponseErrorCodes? ErrorCode { get; set; }

        /// <summary>
        /// Response error message
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
