using CFFileSystemConnection.Common;
using CFFileSystemConnection.Interfaces;
using CFFileSystemMobile.Interfaces;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace CFFileSystemMobile.ViewModels
{
    /// <summary>
    /// Model for MainPage. User can start or stop listening for messages from remote connection
    /// </summary>
    public class MainPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        //private readonly ICurrentState _currentState;
        private readonly CFFileSystemConnection.Interfaces.IFileSystem _fileSystem;
        private readonly CFFileSystemConnection.Interfaces.IUserService _userService;

        private readonly FileSystemRequestHandler _fileSystemRequestHandler;

        public MainPageModel(ICurrentState currentState,
                            CFFileSystemConnection.Interfaces.IFileSystem fileSystem,
                            CFFileSystemConnection.Interfaces.IUserService userService)
        {            
            _fileSystem = fileSystem;
            _userService = userService;            

            // Bind commands
            StartStopListeningCommand = new Command(StartStopListening);          

            // Create file system request handler
            _fileSystemRequestHandler = new FileSystemRequestHandler(fileSystem, userService);
            _fileSystemRequestHandler.OnStatusMessage += (status) =>
            {
                //Status = status;
            };

            // Set handler for client connected
            _fileSystemRequestHandler.OnClientConnected += (endpoint) =>
            {
                ConnectionStatusText = $"Connected ({endpoint.Ip}:{endpoint.Port})";
            };

            // Set handler for client disconnected
            _fileSystemRequestHandler.OnClientDisconnected += (endpoint) =>
            {
                ConnectionStatusText = "Disconnected";
            };

            // Handle user updated. E.g. Security key, permissions etc
            currentState.Events.OnUserUpdated += (user) =>
            {
                _fileSystemRequestHandler.HandleUserUpdated(user);
            };
         
            // Set default connection status
            ConnectionStatusText = "Disconnected";
        }

        private string _debugMessage;
        public string DebugMessage
        {
            get { return _debugMessage; }
            set
            {
                _debugMessage = value;

                OnPropertyChanged(nameof(DebugMessage));
            }
        }

        /// <summary>
        /// Connection status text for remote endpoint (E.g. PC)
        /// </summary>
        private string _connectionStatusText;
        public string ConnectionStatusText
        {
            get { return _connectionStatusText; }
            set
            {
                _connectionStatusText = value;

                OnPropertyChanged(nameof(ConnectionStatusText));
            }
        }

        public string LocalIPList
        {
            get
            {
                StringBuilder list = new StringBuilder("");               

//#if ANDROID
//                WifiManager wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Service.WifiService);
//                int ipaddress = wifiManager.ConnectionInfo.IpAddress;

//                IPAddress ipAddr = new IPAddress(ipaddress);


//                //  System.out.println(host);  
//                return ipAddr.ToString();
//#endif

                var result = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                foreach(var item in result.Where(r => r.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
                {
                    if (list.Length > 0) list.Append("; ");
                    list.Append(item.ToString());
                }
                return list.ToString();                

                //var localIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(a =>
                //            a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
                //return localIP;
            }
        }

        /// <summary>
        /// Command to start/stop listening
        /// </summary>
        public ICommand StartStopListeningCommand { get; set; }

        public string ListeningButtonText
        {
            get { return _fileSystemRequestHandler.IsListening ? 
                            "Stop Listening" : "Start Listening"; }
        }

        public bool IsNotListening => !_fileSystemRequestHandler.IsListening;

        public bool IsStartStopListeningEnabled => !String.IsNullOrEmpty(LocalPort);    

        private void StartStopListening(object parameter)
        {
            if (_fileSystemRequestHandler.IsListening)   // Stop listening
            {
                _fileSystemRequestHandler.StopListening();
            }
            else     // Start listening
            {
                _fileSystemRequestHandler.StartListening(Convert.ToInt32(LocalPort));
            }

            OnPropertyChanged(nameof(IsNotListening));
            OnPropertyChanged(nameof(IsStartStopListeningEnabled));
            OnPropertyChanged(nameof(ListeningButtonText));            
        }

        private string _localPort = "11000"; // Default
        public string LocalPort
        {
            get { return _localPort; }
            set
            {
                _localPort = value;

                OnPropertyChanged(nameof(LocalPort));
                OnPropertyChanged(nameof(IsStartStopListeningEnabled));
            }
        }

        //private string _status = String.Empty;
        //public string Status
        //{
        //    get { return _status; }
        //    set
        //    {
        //        _status = value;

        //        OnPropertyChanged(nameof(Status));
        //    }
        //}
    }
}
