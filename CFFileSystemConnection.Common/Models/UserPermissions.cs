
namespace CFFileSystemConnection.Models
{
    /// <summary>
    /// User permissions
    /// </summary>
    public class UserPermissions
    {
        /// <summary>
        /// Paths that user can access. null=Any. Empty=None.
        /// </summary>
        public List<string>? Paths { get; set; }
    }
}
