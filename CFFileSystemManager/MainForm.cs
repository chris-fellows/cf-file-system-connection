using CFConnectionMessaging.Models;
using CFFileSystemConnection;
using CFFileSystemConnection.Common;
using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Models;
using System.Net;

namespace CFFileSystemManager
{
    public partial class MainForm : Form
    {
        private readonly IFileSystem _fileSystemConnection;

        public MainForm()
        {
            InitializeComponent();

            /*
            var localFileSystem = new FileSystemLocal();
            var test = localFileSystem.GetFolder("/", false, false);
            */

            //FolderObject folderObject = JsonUtilities.DeserializeFromString<FolderObject>(File.ReadAllText("D:\\Test\\Test1.json", System.Text.Encoding.UTF8), JsonUtilities.DefaultJsonSerializerOptions);
           
            string remoteIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(a =>
                            a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
            int remotePort = 11000;   // Port that CFFileSystemHandler is listening on

            var fileSystemConnection = new FileSystemConnection()
            {
                RemoteEndpoint = new EndpointInfo()
                {
                    Ip = remoteIp,      
                    Port  = remotePort
                }
            };
            _fileSystemConnection = fileSystemConnection;

            // Start listening for messages
            fileSystemConnection.StartListening(11005);

            RefreshFolders();
        }

        /// <summary>
        /// Refreshes folder list
        /// </summary>
        private void RefreshFolders()
        {
            // Get root folder and next level folders
            var folder = _fileSystemConnection.GetFolder("/", false, false);

            tvwFolder.Nodes.Clear();

            // Add root node
            var rootNode = tvwFolder.Nodes.Add("/", "/");
            rootNode.Tag = folder;

            // Add sub-folders
            if (folder.Folders != null)
            {
                foreach (var subFolder in folder.Folders)
                {
                    var subFolderNode = rootNode.Nodes.Add($"Folder.{subFolder.Name}", subFolder.Name);
                    subFolderNode.Tag = subFolder;

                    // Add dummy node for sub-folder
                    var dummyNode = subFolderNode.Nodes.Add("Dummy", "Dummy");
                }
            }
        }
    }
}
