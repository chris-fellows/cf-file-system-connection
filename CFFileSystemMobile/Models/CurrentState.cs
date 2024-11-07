using CFFileSystemMobile.Interfaces;

namespace CFFileSystemMobile.Models
{
    public class CurrentState : ICurrentState
    {
        private CurrentStateEvents _events = new CurrentStateEvents();
        public CurrentStateEvents Events => _events;
    }
}
