using CFFileSystemMobile.Models;

namespace CFFileSystemMobile.Interfaces
{
    public interface ICurrentState
    {
        public CurrentStateEvents Events { get; }
    }
}
