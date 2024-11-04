using CFFileSystemConnection.Enums;
using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemMobile.Utilities
{
    internal class InternalUtilities
    { 
        /// <summary>
        /// Creates users
        /// </summary>
        /// <param name="userService"></param>
        public static void CreateUsers(IUserService userService)
        {
            var user1 = new User()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Read",
                Roles = new List<UserRoles>() { UserRoles.FileSystemRead },
                SecurityKey = "d8ahs9b2ik3h49shIaAB2a9ds0338dhdh"
            };
            userService.Add(user1);

            var user2 = new User()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Read & Write",
                Roles = new List<UserRoles>() { UserRoles.FileSystemRead, UserRoles.FileSystemWrite },
                SecurityKey = "sa82j302akspaoihejb7s*aAZ1s29"
            };
            userService.Add(user2);
        }
    }
}
