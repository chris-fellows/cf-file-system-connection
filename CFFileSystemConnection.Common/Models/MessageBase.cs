namespace CFFileSystemConnection.Models
{
    /// <summary>
    /// Base class for message
    /// </summary>
    public abstract class MessageBase
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public string Id { get; set; } = String.Empty;

        /// <summary>
        /// Message type Id
        /// </summary>
        public string TypeId { get; set; } = String.Empty;

        /// <summary>
        /// Response (if any)
        /// </summary>
        public MessageResponse? Response { get; set; }

        /// <summary>
        /// Security key
        /// </summary>
        public string SecurityKey { get; set; } = String.Empty;
    }
}
