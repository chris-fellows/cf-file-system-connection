using CFFileSystemConnection.Enums;

namespace CFFileSystemConnection.Models
{
    /// <summary>
    /// User within the system
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public string Id { get; set; } = String.Empty;

        /// <summary>
        /// User name
        /// </summary>
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Security key
        /// </summary>
        public string SecurityKey { get; set; } = String.Empty;

        /// <summary>
        /// User roles
        /// </summary>
        public List<UserRoles> Roles { get; set; } = new List<UserRoles>();

        /// <summary>
        /// User permissions
        /// </summary>
        public UserPermissions Permissions { get; set; } = new UserPermissions();
    }
}
