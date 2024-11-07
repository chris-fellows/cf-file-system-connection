using CFFileSystemConnection.Enums;
using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Models;
using CFFileSystemMobile.Interfaces;
using CFFileSystemMobile.Models;
using CFFileSystemMobile.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CFFileSystemMobile.ViewModels
{
    /// <summary>
    /// Model for UserSettingsPage. Allows user to configure user settings (Security key, permissions etc)
    /// </summary>
    public class UserSettingsPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly ICurrentState _currentState;
        private readonly IUserService _userService;        

        public ICommand DeleteUserCommand { get; set; }

        public ICommand SaveUserCommand { get; set; }

        public ICommand NewSecurityKeyCommand { get; set; }

        public ICommand CancelCommand { get; set; }


        private const int _securityKeyLength = 25;


        public UserSettingsPageModel(ICurrentState currentState,
                                     IUserService userService)
        {
            _currentState = currentState;
            _userService = userService;

            // Set commands            
            DeleteUserCommand = new Command(DeleteUser);
            SaveUserCommand = new Command(SaveUser);
            NewSecurityKeyCommand = new Command(NewSecurityKey);
            CancelCommand = new Command(Cancel);

            LoadUsers();
        }

        /// <summary>
        /// Loads users. Selects specific user (if requested) for first user
        /// </summary>
        /// <param name="selectedUserId">User Id to select (if any)</param>
        private void LoadUsers(string? selectedUserId = null)
        {
            var users = _userService.GetAll();

            // Add dummy user
            users.Add(new User()
            {
                Name = "[New]",
                Permissions = new UserPermissions(),
                Roles = new List<UserRoles>(),
                SecurityKey= "[New]"
            });
            Users = users;

            SelectedUser = String.IsNullOrEmpty(selectedUserId) ?
                        users[0] : users.First(u => u.Id == selectedUserId);
        }

        private void NewSecurityKey(object parameter)
        {
            var securityKey = new StringBuilder("");
            var random = new Random();
            while (securityKey.Length < _securityKeyLength)
            {
                Char character = (Char)random.Next(0, 127);
                if (Char.IsDigit(character) ||
                    Char.IsLetter(character))
                {
                    securityKey.Append(character);
                }
            }

            SecurityKey = securityKey.ToString();
        }

        private void Cancel(object parameter)
        {
            LoadUsers(SelectedUser?.Id);
        }
      
        private void DeleteUser(object parameter)
        {
            _userService.Delete(SelectedUser.Id);

            // Notify user updated
            _currentState.Events.RaiseOnUserUpdated(SelectedUser);

            LoadUsers();
        }

        private void SaveUser(object parameter)
        {
            // Set user roles
            SelectedUser.Roles = this.UserRoles.Where(ur => ur.Enabled)
                                    .Select(ur => ur.Value).ToList();

            // Set paths that user can access            
            SelectedUser.Permissions.Paths = DriveSettings.Where(ds => ds.Enabled).Select(ds => ds.Value).ToList();            

            if (String.IsNullOrEmpty(SelectedUser.Id))   // New user
            {
                SelectedUser.Id = Guid.NewGuid().ToString();
                _userService.Add(SelectedUser);
            }
            else    // Existing user
            {
                _userService.Update(SelectedUser);                
            }

            // Notify user updated
            _currentState.Events.RaiseOnUserUpdated(SelectedUser);

            // Refresh users
            LoadUsers(SelectedUser.Id);
        }

        public string? UserName
        {
            get { return _selectedUser?.Name; }
            set
            {
                if (_selectedUser != null) _selectedUser.Name = value;

                OnPropertyChanged(nameof(UserName));
                OnPropertyChanged(nameof(IsSaveEnabled));
            }
        }

        /// <summary>
        /// Security key for user
        /// </summary>
        public string? SecurityKey
        {
            get { return _selectedUser?.SecurityKey; }
            set
            {
                if (_selectedUser != null) _selectedUser.SecurityKey = value;

                OnPropertyChanged(nameof(SecurityKey));
                OnPropertyChanged(nameof(IsSaveEnabled));
            }
        }

        /// <summary>
        /// Selected user
        /// </summary>
        private User? _selectedUser;
        public User? SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;

                OnPropertyChanged(nameof(SelectedUser));
                OnPropertyChanged(nameof(UserName));
                OnPropertyChanged(nameof(SecurityKey));
                OnPropertyChanged(nameof(IsDeleteEnabled));
                OnPropertyChanged(nameof(IsSaveEnabled));

                if (_selectedUser == null)
                {
                    UserRoles = new List<SelectableItem<UserRoles>>();
                    DriveSettings = new List<SelectableItem<string>>();
                }
                else
                {
                    LoadUserSettings(_selectedUser);
                }
            }
        }

        /// <summary>
        /// Loads user settings
        /// </summary>
        /// <param name="user"></param>
        private void LoadUserSettings(User user)
        {
            UserRoles = new List<SelectableItem<UserRoles>>();
            DriveSettings = new List<SelectableItem<string>>();

            // Load user roles
            var userRoles = new List<SelectableItem<UserRoles>>();
            foreach (UserRoles userRole in Enum.GetValues(typeof(UserRoles)))
            {
                var userRoleItem = new SelectableItem<UserRoles>()
                {
                    Name = EnumUtilities.GetEnumDescription(userRole),
                    Value = userRole,
                    Enabled = user.Roles.Contains(userRole)
                };
                userRoles.Add(userRoleItem);
            }
            
            UserRoles = userRoles.OrderBy(ur => ur.Name).ToList();

            // Load drive settings
            var driveSettings = new List<SelectableItem<string>>();
            foreach (var drive in DriveInfo.GetDrives()
                    .Where(d => d.DriveType != DriveType.Ram))
            {
                var driveSetting = new SelectableItem<string>()
                {
                    Name = drive.Name,
                    Value = drive.RootDirectory.ToString(),
                    Enabled = user.Permissions.Paths != null &&
                            user.Permissions.Paths.Contains(drive.RootDirectory.ToString())
                };
                driveSettings.Add(driveSetting);
            }

            DriveSettings = driveSettings.OrderBy(ds => ds.Name).ToList();
        }

        /// <summary>
        /// Whether user can be deleted
        /// </summary>
        public bool IsDeleteEnabled => SelectedUser != null && 
                            !String.IsNullOrEmpty(SelectedUser.Id);

        /// <summary>
        /// Whether user can be saved. Can save even if no paths set.
        /// </summary>
        public bool IsSaveEnabled => SelectedUser != null &&
                            !String.IsNullOrEmpty(SelectedUser.Name) &&
                            !String.IsNullOrEmpty(SelectedUser.SecurityKey) &&
                            SelectedUser.SecurityKey.Length > 0;    // _securityKeyLength;   // TODO: Set min length

        /// <summary>
        /// User list
        /// </summary>
        private List<User> _users = new List<User>();
        public List<User> Users
        {
            get { return _users; }
            set
            {
                _users = value;

                OnPropertyChanged(nameof(Users));
            }
        }

        /// <summary>
        /// Drive settings for user. Enabled=true if selected.
        /// </summary>
        private List<SelectableItem<string>> _driveSettings;
        public List<SelectableItem<string>> DriveSettings
        {
            get { return _driveSettings; }
            set
            {
                _driveSettings = value;

                OnPropertyChanged(nameof(DriveSettings));
            }
        }

        /// <summary>
        /// User roles. Enabled=true if selected.
        /// </summary>
        private List<SelectableItem<UserRoles>> _userRoles;

        public List<SelectableItem<UserRoles>> UserRoles
        {
            get { return _userRoles; }
            set
            {
                _userRoles = value;

                OnPropertyChanged(nameof(UserRoles));
            }
        }
    }
}
