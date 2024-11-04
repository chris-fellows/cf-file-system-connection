using CFConnectionMessaging.Models;
using CFFileSystemConnection;
using CFFileSystemConnection.Common;
using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Models;
using CFFileSystemConnection.Service;
using CFFileSystemManager.Controls;
using System.Net;
using System.Windows.Forms;

namespace CFFileSystemManager
{
    public partial class MainForm : Form
    {
        private readonly IConnectionSettingsService _connectionSettingsService;
        private IFileSystem _fileSystem;

        public MainForm()
        {
            InitializeComponent();

            _connectionSettingsService = new JsonConnectionSettingsService(Path.Combine(Environment.CurrentDirectory, "Data"));

            /*
            // Get remote IP and port
            var remoteIp = System.Configuration.ConfigurationManager.AppSettings.Get("RemoteIP").ToString();
            if (remoteIp.Equals("local"))
            {
                remoteIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(a =>
                            a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
            }

            int remotePort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings.Get("RemotePort").ToString());

            var securityKey = System.Configuration.ConfigurationManager.AppSettings.Get("SecurityKey").ToString();
            var fileSystemConnection = new FileSystemConnection(securityKey)
            {
                RemoteEndpoint = new EndpointInfo()
                {
                    Ip = remoteIp,      
                    Port  = remotePort
                }
            };
            _fileSystemConnection = fileSystemConnection;

            // Start listening for messages
            var localPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings.Get("LocalPort").ToString());
            fileSystemConnection.StartListening(localPort);

            RefreshFolders();
            */
        }

        /// <summary>
        /// Initialises the connection to the file system. Displays initialise list of top level folders.
        /// </summary>
        /// <param name="connectionSettings"></param>
        private void InitialiseConnection(ConnectionSettings connectionSettings)
        {
            DisplayStatus($"Initialising connection for {connectionSettings.Name}");

            if (_fileSystem != null)
            {
                _fileSystem = null;
            }

            var fileSystemConnection = new FileSystemConnection(connectionSettings.SecurityKey)
            {
                RemoteEndpoint = new EndpointInfo()
                {
                    Ip = connectionSettings.RemoteEndpoint.Ip,
                    Port = connectionSettings.RemoteEndpoint.Port
                }
            };
            _fileSystem = fileSystemConnection;

            // Start listening for messages
            var localPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings.Get("LocalPort").ToString());
            fileSystemConnection.StartListening(localPort);

            DisplayStatus("Refreshing folders");
            RefreshFolders();
            DisplayStatus("Ready");
        }

        private void DisplayStatus(string status)
        {
            toolStripStatusLabel1.Text = $" {status}";
            statusStrip1.Update();
        }

        private void CreateConnectionSettingsList(bool resetAll)
        {
            var allConnectionSettings = _connectionSettingsService.GetAll();

            if (resetAll)
            {
                foreach (var cs in allConnectionSettings)
                {
                    _connectionSettingsService.Delete(cs.Id);
                }
                allConnectionSettings.Clear();
            }

            if (!allConnectionSettings.Any())
            {
                var localIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(a =>
                                a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();

                var connectionSettings1 = new ConnectionSettings()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test 1",
                    RemoteEndpoint = new EndpointInfo()
                    {
                        Ip = localIp,
                        Port = 11000
                    },
                    SecurityKey = "d8ahs9b2ik3h49shIaAB2a9ds0338dhdh"
                };
                _connectionSettingsService.Add(connectionSettings1);
            }
        }

        /// <summary>
        /// Refreshes folder list
        /// </summary>
        private void RefreshFolders()
        {
            // Get root folder and next level folders
            var folder = _fileSystem.GetFolder("/", false, false);

            tvwFolder.Nodes.Clear();

            // Add root node
            var rootNode = tvwFolder.Nodes.Add("/", "/");
            rootNode.Tag = folder;

            // Add sub-folders
            if (folder.Folders != null)
            {
                folder.Folders.ForEach(subFolder => AddFolderToTree(rootNode, subFolder));
            }

            // Expand top level sub-folders of root
            rootNode.Expand();

            int xxx = 1000;
        }

        /// <summary>
        /// Adds folder node to tree. We add a dummy node so that before node is expanded then we can load
        /// any sub-folders.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="folder"></param>
        private void AddFolderToTree(TreeNode parentNode, FolderObject folder)
        {
            var subFolderNode = parentNode.Nodes.Add($"Folder.{folder.Name}", folder.Name);
            subFolderNode.Tag = folder;
            subFolderNode.ContextMenuStrip = cmsFolder;

            // Add dummy node for sub-folder
            var dummyNode = subFolderNode.Nodes.Add("Dummy", "Dummy");
        }

        private void tscbConnection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tscbConnection.SelectedIndex != -1)
            {
                ConnectionSettings connectionSettings = (ConnectionSettings)tscbConnection.SelectedItem;
                InitialiseConnection(connectionSettings);
            }
        }

        private void tvwFolder_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null && e.Node.Tag is FolderObject)
            {
                FolderObject folderObject = (FolderObject)e.Node.Tag;

                DisplayStatus($"Getting files for {folderObject.Path}");

                // Get files for folder
                var folderObjectWithFiles = _fileSystem.GetFolder(folderObject.Path, true, false);

                // Display folder control and display file list
                splitContainer1.Panel2.Controls.Clear();
                var folderControl = new FolderControl(_fileSystem, folderObjectWithFiles);
                folderControl.Dock = DockStyle.Fill;
                splitContainer1.Panel2.Controls.Add(folderControl);

                DisplayStatus("Ready");
            }
        }

        private void tvwFolder_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            // If node contains dummy node then read replace with nodes for sub-folders
            if (e.Node != null &&
                e.Node.Nodes.Count > 0 &&
                e.Node.Nodes[0].Text.Equals("Dummy"))
            {
                // Clear dummy node                
                e.Node.Nodes.Clear();

                FolderObject parentFolderObject = (FolderObject)e.Node.Tag;

                DisplayStatus($"Reading folder {parentFolderObject.Path}");

                // Get sub-folders
                var folderObject = _fileSystem.GetFolder(parentFolderObject.Path, false, false);

                // Add sub-folders to tree
                if (folderObject.Folders != null)
                {
                    folderObject.Folders.ForEach(subFolder => AddFolderToTree(e.Node, subFolder));
                }

                DisplayStatus("Ready");
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            DisplayStatus("Initialising");

            // Create default connection settings list
            CreateConnectionSettingsList(false);

            // Display connection list
            var connectionSettingsList = _connectionSettingsService.GetAll();
            tscbConnection.ComboBox.DisplayMember = nameof(ConnectionSettings.Name);
            tscbConnection.ComboBox.ValueMember = nameof(ConnectionSettings.Id);
            tscbConnection.ComboBox.DataSource = connectionSettingsList;
            tscbConnection.SelectedIndex = 0;

            DisplayStatus("Ready");
        }

        private void copyToLocalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderObject folderObject = (FolderObject)tvwFolder.SelectedNode.Tag;

            if (MessageBox.Show($"Copy folder {folderObject.Name} to local?", "Copy Folder", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DisplayStatus("Copying folders");

                // Set local folder to copy to
                var localFolder = Path.Combine(Path.GetTempPath(), "Download", Guid.NewGuid().ToString(), folderObject.Name);
                CopyFolderToLocal(folderObject, localFolder);

                DisplayStatus("Ready");

                MessageBox.Show("Folder copied", "Copy Folder");
            }
        }

        /// <summary>
        /// Copy remote folder to local folder
        /// </summary>
        /// <param name="folderObject"></param>
        /// <param name="localFolder"></param>
        private void CopyFolderToLocal(FolderObject folderObject, string localFolder)
        {
            DisplayStatus($"Copying {folderObject.Path}");

            Directory.CreateDirectory(localFolder);

            // Get folder file list
            var folderObjectFull = _fileSystem.GetFolder(folderObject.Path, true, false);
            Thread.Yield();

            // Copy files
            if (folderObjectFull.Files != null)
            {
                foreach(var fileObject in folderObjectFull.Files)
                {
                    // Get file content
                    // TODO: Split file
                    DisplayStatus($"Copying {fileObject.Path}");
                    var fileContent = _fileSystem.GetFileContent(fileObject.Path);
                    if (fileContent != null)
                    {
                        var localFilePath = Path.Combine(localFolder, fileObject.Name);
                        File.WriteAllBytes(localFilePath, fileContent);
                        Thread.Yield();
                    }                    
                }
            }

            // Copy sub-folders
            if (folderObjectFull.Folders != null)
            {
                foreach(var subFolderObject in folderObjectFull.Folders)
                {
                    CopyFolderToLocal(subFolderObject, Path.Combine(localFolder, subFolderObject.Name));
                }
            }

            DisplayStatus($"Copyied {folderObject.Path}");
        }
    }
}