
namespace CFFileSystemConnection.Models
{
    /// <summary>
    /// User permissions
    /// </summary>
    public class UserPermissions
    {
        /// <summary>
        /// Paths that user can access
        /// </summary>
        public List<string> Paths { get; set; } = new List<string>();
    }
}
