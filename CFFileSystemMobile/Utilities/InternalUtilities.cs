using CFFileSystemConnection.Enums;
using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Models;

namespace CFFileSystemMobile.Utilities
{
    internal class InternalUtilities
    {
        /// <summary>
        /// Creates users
        /// 
        /// NOTE: UserPermissions can specify a list of paths that are allowed. By default then users have access to
        /// all paths.
        /// </summary>
        /// <param name="userService"></param>
        public static void CreateUsers(IUserService userService)
        {
            var user1 = new User()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Read",
                Roles = new List<UserRoles>() { UserRoles.FileSystemRead },
                SecurityKey = "d8ahs9b2ik3h49shIaAB2a9ds0338dhdh",
                Permissions = new UserPermissions()
                {
                    Paths = null   // All paths
                }
            };
            userService.Add(user1);

            var user2 = new User()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Read & Write",
                Roles = new List<UserRoles>() { UserRoles.FileSystemRead, UserRoles.FileSystemWrite },
                SecurityKey = "sa82j302akspaoihejb7s*aAZ1s29",
                Permissions = new UserPermissions()
                {
                    Paths = null    // All paths
                }
            };
            userService.Add(user2);
        }
    }
}
