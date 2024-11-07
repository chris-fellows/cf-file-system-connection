using CFFileSystemConnection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemMobile.Models
{
    public class CurrentStateEvents
    {
        public delegate void UserUpdated(User user);
        public event UserUpdated? OnUserUpdated;

        public void RaiseOnUserUpdated(User user)
        {
            if (OnUserUpdated != null)
            {
                OnUserUpdated(user);
            }
        }
    }
}
