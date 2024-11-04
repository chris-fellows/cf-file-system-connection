using CFFileSystemConnection.Models;

namespace CFFileSystemConnection.Interfaces
{
    /// <summary>
    /// Interface for managing User instances
    /// </summary>
    public interface IUserService : IEntityWithIdService<User>
    {
        User? GetBySecurityKey(string securityKey);
    }
}
