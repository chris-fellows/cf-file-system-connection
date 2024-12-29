using CFConnectionMessaging.Models;
using CFFileSystemConnection;
using CFFileSystemConnection.Common;
using CFFileSystemConnection.Exceptions;
using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Models;
using CFFileSystemConnection.Service;
using CFFileSystemManager.Controls;
using CFFileSystemManager.Utilities;
using System.IO.Packaging;
using System.Net;

namespace CFFileSystemManager
{
    public partial class MainForm : Form
    {
        private readonly IConnectionSettingsService _connectionSettingsService;
        private IFileSystem _fileSystemRemote;

        /// <summary>
        /// Local file system. We go via this interface so that we can have code than can use either a local or remote file
        /// system.
        /// </summary>
        private IFileSystem _fileSystemLocal = new FileSystemLocal();

        private const int _folderIconIndex = 0;

        /// <summary>
        /// Size of file sections to transfer when reading or writing files
        /// </summary>
        private const int _fileSectionBytes = 1024 * 1000;

        private const string _noneText = "<None>";

        public MainForm()
        {
            InitializeComponent();

            _connectionSettingsService = new JsonConnectionSettingsService(Path.Combine(Environment.CurrentDirectory, "Data"));
        }

        /// <summary>
        /// Initialises the connection to the file system. Displays initialise list of top level folders.
        /// </summary>
        /// <param name="connectionSettings"></param>
        private void InitialiseConnection(ConnectionSettings connectionSettings)
        {
            DisplayStatus($"Initialising connection for {connectionSettings.Name}");

            // Clean up connection
            if (_fileSystemRemote != null)
            {
                _fileSystemRemote.Close();
            }

            var fileSystemConnection = new FileSystemConnection(String.IsNullOrEmpty(connectionSettings.PathDelimiter) ? null : connectionSettings.PathDelimiter[0],
                                                            connectionSettings.SecurityKey)
            {
                RemoteEndpoint = new EndpointInfo()
                {
                    Ip = connectionSettings.RemoteEndpoint.Ip,
                    Port = connectionSettings.RemoteEndpoint.Port
                }
            };
            _fileSystemRemote = fileSystemConnection;

            // Start listening for messages
            var localPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings.Get("LocalPort").ToString());
            fileSystemConnection.StartListening(localPort);

            // Display drives            
            RefreshDrives();

            DisplayStatus("Ready");
        }

        private void RefreshDrives()
        {
            DisplayStatus("Getting drives");

            List<DriveObject> drives = new List<DriveObject>();

            try
            {
                drives = _fileSystemRemote.GetDrives();
            }
            catch (FileSystemConnectionException fscException)
            {
                MessageBox.Show($"Connection error: {fscException.Message}", "Error");
            }

            if (!drives.Any())
            {
                drives.Add(new DriveObject() { Name = _noneText });
            }

            tscbDrive.ComboBox.DisplayMember = nameof(DriveObject.Name);
            tscbDrive.ComboBox.ValueMember = nameof(DriveObject.Name);
            tscbDrive.ComboBox.DataSource = drives;
            tscbDrive.SelectedIndex = 0;

            DisplayStatus("Got drives");
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
        /// Refreshes folder list for drive
        /// </summary>
        private void RefreshRootFolders(DriveObject driveObject)
        {
            DisplayStatus($"Refreshing folders for {driveObject.Name}");

            // Get root folder and next level folders
            //var folder = _fileSystem.GetFolder("/", false, false);

            var folder = _fileSystemRemote.GetFolder(driveObject.Path, false, false);

            splitContainer1.Panel2.Controls.Clear();
            tvwFolder.Nodes.Clear();

            // Add root node
            var rootNode = tvwFolder.Nodes.Add("/", "/", _folderIconIndex);
            rootNode.Tag = folder;

            if (folder == null)
            {
                MessageBox.Show("No folders were returned", "Warning");
            }
            else
            {
                // Add sub-folders
                if (folder.Folders != null)
                {
                    folder.Folders.OrderBy(f => f.Name).ToList().ForEach(subFolder => AddFolderToTree(rootNode, subFolder));
                }

                // Expand top level sub-folders of root
                rootNode.Expand();
            }

            DisplayStatus("Ready");
        }

        /// <summary>
        /// Adds folder node to tree. We add a dummy node so that before node is expanded then we can load
        /// any sub-folders.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="folder"></param>
        private void AddFolderToTree(TreeNode parentNode, FolderObject folder)
        {
            var subFolderNode = parentNode.Nodes.Add($"Folder.{folder.Name}", folder.Name, _folderIconIndex);
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
                var folderObjectWithFiles = _fileSystemRemote.GetFolder(folderObject.Path, true, false);

                // Display folder control and display file list
                splitContainer1.Panel2.Controls.Clear();
                var folderControl = new FolderControl(_fileSystemRemote, folderObjectWithFiles);
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
                var folderObject = _fileSystemRemote.GetFolder(parentFolderObject.Path, false, false);

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
            //var connectionSettingsList = _connectionSettingsService.GetAll().OrderBy(cs => cs.Name.Contains("Console") ? 0 : 1).ToList();
            var connectionSettingsList = _connectionSettingsService.GetAll().OrderBy(cs => cs.Name.Contains("G82") ? 0 : 1).ToList();

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
                FileSystemUtilities.CopyFolderToLocal(_fileSystemRemote, folderObject, localFolder, _fileSectionBytes,
                                    (status) => DisplayStatus(status));

                DisplayStatus("Ready");

                MessageBox.Show("Folder copied", "Copy Folder");
            }
        }

        private void tscbDrive_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tscbDrive.SelectedIndex != -1)
            {
                DriveObject driveObject = (DriveObject)tscbDrive.SelectedItem;
                if (driveObject.Name == _noneText)
                {
                    tvwFolder.Nodes.Clear();
                    splitContainer1.Panel2.Controls.Clear();
                }
                else
                {
                    RefreshRootFolders(driveObject);
                }
            }
        }

        private void testCopyFileBySectionsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void copyLocalFileToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Select file(s)",
                CheckFileExists = true
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FolderObject folderObject = (FolderObject)tvwFolder.SelectedNode.Tag;

                // Copy file(s)
                foreach (var localFile in dialog.FileNames)
                {
                    string remoteFile = Path.Combine(folderObject.Path, Path.GetFileName(localFile));
                    FileSystemUtilities.CopyFileBetween(_fileSystemLocal, localFile,
                                    _fileSystemRemote, remoteFile,
                                    _fileSectionBytes,
                                    (status) => DisplayStatus(status));
                }

                MessageBox.Show("File(s) copied", "Copy File(s)");
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clean up
            if (_fileSystemRemote != null)
            {
                _fileSystemRemote.Close();
            }
            if (_fileSystemLocal != null)
            {
                _fileSystemLocal.Close();
            }
        }

        //// Determine whether one node is a parent 
        //// or ancestor of a second node.
        //private bool ContainsNode(TreeNode node1, TreeNode node2)
        //{
        //    // Check the parent node of the second node.
        //    if (node2.Parent == null) return false;
        //    if (node2.Parent.Equals(node1)) return true;

        //    // If the parent node is not null or equal to the first node, 
        //    // call the ContainsNode method recursively using the parent of 
        //    // the second node.
        //    return ContainsNode(node1, node2.Parent);
        //}

        private void tvwFolder_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void tvwFolder_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Move the dragged node when the left mouse button is used.
            if (e.Button == MouseButtons.Left)
            {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }

            // Copy the dragged node when the right mouse button is used.
            else if (e.Button == MouseButtons.Right)
            {
                DoDragDrop(e.Item, DragDropEffects.Copy);
            }
        }

        private void tvwFolder_DragOver(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the mouse position.
            Point targetPoint = tvwFolder.PointToClient(new Point(e.X, e.Y));

            // Select the node at the mouse position.
            tvwFolder.SelectedNode = tvwFolder.GetNodeAt(targetPoint);
        }

        private void tvwFolder_DragDrop(object sender, DragEventArgs e)
        {
            // Get drop location point
            Point targetPoint = tvwFolder.PointToClient(new Point(e.X, e.Y));

            // Get drop node
            TreeNode targetNode = tvwFolder.GetNodeAt(targetPoint);

            // Check that it's a folder node
            if (!(targetNode.Tag is FolderObject))
            {
                return;
            }

            // Get folder object to copy to
            var folderObject = (FolderObject)targetNode.Tag;

            // Get files dropped
            var filesDropped = (string[])e.Data.GetData(DataFormats.FileDrop);

            // Check that only files dropped
            // TODO: Support folder copy            
            if (filesDropped.Any(file => !File.Exists(file)))
            {
                MessageBox.Show("Only files can be dropped", "Error");
                return;
            }

            if (MessageBox.Show($"Do you want to copy the selected file(s) to {folderObject.Path}?", "Copy Files", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (var file in filesDropped)
                {
                    if (File.Exists(file))      // Check that it's not a folder
                    {
                        FileSystemUtilities.CopyFileBetween(_fileSystemLocal, file,
                                    _fileSystemRemote, Path.Combine(folderObject.Path, Path.GetFileName(file)),
                                    _fileSectionBytes,
                                    (status) => DisplayStatus(status));
                    }
                }

                MessageBox.Show("File(s) copied", "Copy Files");
            }
        }

        private void cmsFolder_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Set state of paste folders/files
            pasteFilesToolStripMenuItem.Visible = false;
            pasteFoldersToolStripMenuItem.Visible = false;

            if (Clipboard.ContainsFileDropList())
            {
                var items = Clipboard.GetFileDropList();
                foreach (var item in items)
                {
                    if (Directory.Exists(item))
                    {
                        pasteFoldersToolStripMenuItem.Visible = true;
                    }
                    else if (File.Exists(item))
                    {
                        pasteFilesToolStripMenuItem.Visible = true;
                    }
                }
            }

            int ccc = 1000;
        }

        private void pasteFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get folder to copy to
            FolderObject folderObjectDst = (FolderObject)tvwFolder.SelectedNode.Tag;

            if (Clipboard.ContainsFileDropList())
            {
                // Get folders to copy
                var items = Clipboard.GetFileDropList();
                var folders = new List<string>();
                foreach (var item in items)
                {
                    if (Directory.Exists(item))
                    {
                        folders.Add(item);
                    }
                }
                folders.Sort();

                // Copy folders
                if (folders.Any())
                {
                    if (MessageBox.Show($"Copy folders to {folderObjectDst.Path}?", "Copy Folder(s)", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        foreach (var folderSrc in folders)
                        {
                            var folderName = new DirectoryInfo(folderSrc).Name;

                            var folderDst = _fileSystemRemote.PathCombine(folderObjectDst.Path, folderName);

                            FileSystemUtilities.CopyFolderBetween(_fileSystemLocal, folderSrc,
                                            _fileSystemRemote, folderDst,
                                            _fileSectionBytes,
                                              (status) => DisplayStatus(status));
                        }
                    }
                }
            }

        }

        private void getMusicFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var drives = _fileSystemRemote.GetDrives();

            var drivesFound = new List<DriveObject>();

            foreach (var drive in drives)
            {
                try
                {
                    var folder = _fileSystemRemote.GetFolder(drive.Path, false, true);
                    if (folder != null)
                    {
                        if (IsFolderToFind(folder))
                        {
                            drivesFound.Add(drive);
                        }
                    }
                }
                catch (Exception exception)
                {

                }
            }

            int xxx = 1000;
        }

        private static bool IsFolderToFind(FolderObject folderObject)
        {
            if (Array.IndexOf(new[] { "music", "dcim", "podcasts" }, folderObject.Name.ToLower()) != -1) return true;

            if (folderObject.Folders != null)
            {
                foreach (var subFolderObject in folderObject.Folders.Where(sf => sf != null))
                {
                    if (IsFolderToFind(subFolderObject)) return true;
                }
            }

            return false;
        }

        private void pasteFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get folder to copy to
            FolderObject folderObjectDst = (FolderObject)tvwFolder.SelectedNode.Tag;

            if (Clipboard.ContainsFileDropList())
            {
                // Get files to copy
                var items = Clipboard.GetFileDropList();
                var files = new List<string>();
                foreach (var item in items)
                {
                    if (File.Exists(item))
                    {
                        files.Add(item);
                    }
                }
                files.Sort();

                // Copy files
                if (files.Any())
                {
                    if (MessageBox.Show($"Copy files to {folderObjectDst.Path}?", "Copy Files(s)", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        foreach (var fileSrc in files)
                        {
                            var fileObjectSrc = _fileSystemLocal.GetFile(fileSrc);

                            var fileDst = _fileSystemRemote.PathCombine(folderObjectDst.Path, fileObjectSrc.Name);

                            FileSystemUtilities.CopyFileBetween(_fileSystemLocal, fileSrc,
                                            _fileSystemRemote, fileDst,
                                            _fileSectionBytes,
                                              (status) => DisplayStatus(status));
                        }
                    }
                }
            }
        }
    }
}
