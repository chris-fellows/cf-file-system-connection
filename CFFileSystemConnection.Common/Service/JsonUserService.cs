using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Models;
using CFFileSystemConnection.Utilities;
using System.Text;

namespace CFFileSystemConnection.Service
{
    /// <summary>
    /// User service with data stored in local JSON files
    /// </summary>
    public class JsonUserService : JsonEntityWithIdService<User>, IUserService
    {        
        public JsonUserService(string folder) : base(folder, (user) => user.Id)
        {
         
        }

        public User? GetBySecurityKey(string securityKey)
        {
            return GetAll().FirstOrDefault(u => u.SecurityKey == securityKey);            
        }      
    }
}
