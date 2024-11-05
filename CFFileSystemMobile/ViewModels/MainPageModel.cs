using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using CFFileSystemConnection;
using CFFileSystemConnection.Common;
using CFFileSystemConnection.Interfaces;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace CFFileSystemMobile.ViewModels
{
    /// <summary>
    /// Model to MainPage
    /// </summary>
    public class MainPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly CFFileSystemConnection.Interfaces.IFileSystem _fileSystem;
        private readonly CFFileSystemConnection.Interfaces.IUserService _userService;

        private readonly FileSystemRequestHandler _fileSystemRequestHandler;        

        public MainPageModel(CFFileSystemConnection.Interfaces.IFileSystem fileSystem,
                            CFFileSystemConnection.Interfaces.IUserService userService)
        {
            _fileSystem = fileSystem;
            _userService = userService;            

            // Bind commands
            StartListeningCommand = new Command(StartListening);
            StopListeningCommand = new Command(StopListening);

            // Create file system request handler
            _fileSystemRequestHandler = new FileSystemRequestHandler(fileSystem, userService);
            _fileSystemRequestHandler.OnStatusMessage += (status) =>
            {
                Status = status;
            };

            StringBuilder debug = new StringBuilder("");          
            DebugMessage = debug.ToString();               
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
        /// Command to start listening
        /// </summary>
        public ICommand StartListeningCommand { get; set; }

        /// <summary>
        /// Command to stop listening
        /// </summary>
        public ICommand StopListeningCommand { get; set; }

        /// <summary>
        /// Whether start listening command is enabled
        /// </summary>
        public bool StartListeningEnabled => !_fileSystemRequestHandler.IsListening;

        /// <summary>
        /// Whether stop listening command is enable
        /// </summary>
        public bool StopListeningEnabled => _fileSystemRequestHandler.IsListening;

        private void StartListening(object parameter)
        {
            _fileSystemRequestHandler.StartListening(Convert.ToInt32(LocalPort));

            OnPropertyChanged(nameof(StartListeningEnabled));
            OnPropertyChanged(nameof(StopListeningEnabled));
        }

        private void StopListening(object parameter)
        {            
            _fileSystemRequestHandler.StopListening();

            OnPropertyChanged(nameof(StartListeningEnabled));
            OnPropertyChanged(nameof(StopListeningEnabled));
        }

        private string _localPort = "11000"; // Default
        public string LocalPort
        {
            get { return _localPort; }
            set
            {
                _localPort = value;

                OnPropertyChanged(nameof(LocalPort));
            }
        }

        private string _status = String.Empty;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;

                OnPropertyChanged(nameof(Status));
            }
        }
    }
}
