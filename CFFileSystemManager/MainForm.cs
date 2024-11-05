using CFConnectionMessaging.Models;
using CFFileSystemConnection;
using CFFileSystemConnection.Common;
using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Models;
using CFFileSystemConnection.Service;
using CFFileSystemManager.Controls;
using System.CodeDom;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace CFFileSystemManager
{
    public partial class MainForm : Form
    {
        private readonly IConnectionSettingsService _connectionSettingsService;
        private IFileSystem _fileSystem;
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

            //var file = "D:\\Test\\Creed\\Human Clay\\09-Higher.mp3";
            //TestFileReadSections(file);

            //IFileSystem fileSystem = new FileSystemLocal();
            //var folderObject = fileSystem.GetFolder(@"C:\", false, false);

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

        //private void TestFileReadSections(string file)
        //{            
        //    int sectionCount = 0;
        //    var sections = new List<byte[]>();
        //    using (var streamReader = new BinaryReader(File.OpenRead(file)))
        //    {
        //        var section = new byte[0];
        //        do
        //        {
        //            section = streamReader.ReadBytes(_fileSectionBytes);
        //            sectionCount++;
        //            sections.Add(section);
        //        } while (streamReader.BaseStream.Position < streamReader.BaseStream.Length);
        //    }

        //    var newFile = Path.Combine("D:\\Test\\", Path.GetFileName(file));
        //    using (var writer = new BinaryWriter(File.OpenWrite(newFile)))
        //    {
        //        while (sections.Any())
        //        {
        //            writer.Write(sections[0]);
        //            sections.RemoveAt(0);
        //        }

        //        writer.Flush();
        //    }

        //    var isSameFileContents = IsSameFileContents(file, newFile);

        //    int xxx = 1000;
        //}

        //private static bool IsSameFileContents(string file1, string file2)
        //{
        //    if (!File.Exists(file1) || !File.Exists(file2)) return false;

        //    var content1 = File.ReadAllBytes(file1);
        //    var content2 = File.ReadAllBytes(file2);

        //    if (content1.Length != content2.Length) return false;

        //    for (int index = 0; index < content1.Length; index++)
        //    {
        //        if (content1[index] != content2[index]) return false;
        //    }

        //    return true;
        //}

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

            // Display drives            
            RefreshDrives();
   
            DisplayStatus("Ready");
        }

        private void RefreshDrives()
        {
            DisplayStatus("Getting drives");

            var drives = _fileSystem.GetDrives();

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

            var folder = _fileSystem.GetFolder(driveObject.Path, false, false);

            tvwFolder.Nodes.Clear();

            // Add root node
            var rootNode = tvwFolder.Nodes.Add("/", "/", _folderIconIndex);            
            rootNode.Tag = folder;

            // Add sub-folders
            if (folder.Folders != null)
            {
                folder.Folders.ForEach(subFolder => AddFolderToTree(rootNode, subFolder));
            }

            // Expand top level sub-folders of root
            rootNode.Expand();

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
            var connectionSettingsList = _connectionSettingsService.GetAll().OrderBy(cs => cs.Name.Contains("Console") ? 0 : 1).ToList();

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
                foreach (var fileObject in folderObjectFull.Files)
                {
                    // Get file content                    
                    DisplayStatus($"Copying {fileObject.Path}");
                    CopyFileToLocal(fileObject, Path.Combine(localFolder, fileObject.Name));
                    Thread.Yield();
                }
            }

            // Copy sub-folders
            if (folderObjectFull.Folders != null)
            {
                foreach (var subFolderObject in folderObjectFull.Folders)
                {
                    CopyFolderToLocal(subFolderObject, Path.Combine(localFolder, subFolderObject.Name));
                }
            }

            DisplayStatus($"Copyied {folderObject.Path}");
        }

        /// <summary>
        /// Copies remote file to local
        /// </summary>
        /// <param name="fileObject"></param>
        /// <param name="localFile"></param>
        private void CopyFileToLocal(FileObject fileObject, string localFile)
        {
            using (var writer = new BinaryWriter(File.OpenWrite(localFile)))
            {
                _fileSystem.GetFileContentBySection(fileObject.Path, 1024 * 500, (section, isMore) =>
                {
                    writer.Write(section);
                    //lastIsMore = isMore;
                });
                writer.Flush();
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
                }
                else
                {
                    RefreshRootFolders(driveObject);
                }
            }
        }

        private void testCopyFileBySectionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //var remoteFile = "D:\\Test\\Creed\\Human Clay\\09-Higher.mp3";
            //var localFile = "D:\\Test\\09-Higher-Copy.mp3";

            //var lastIsMore = false;
            //try
            //{
            //    using (var writer = new BinaryWriter(File.OpenWrite(localFile)))
            //    {
            //        _fileSystem.GetFileContentBySection(remoteFile, 1024 * 500, (section, isMore) =>
            //        {
            //            writer.Write(section);
            //            lastIsMore = isMore;
            //        });
            //        writer.Flush();
            //    }
            //}
            //catch (Exception exception)
            //{   // Clean up partial file
            //    if (File.Exists(localFile)) File.Delete(localFile);
            //    throw;
            //}

            //// Check file same
            //var isSameFile = IsSameFileContents(remoteFile, localFile);

            //int xxx = 1000;
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
                foreach(var localFile in dialog.FileNames)
                {
                    string remoteFile = Path.Combine(folderObject.Path, Path.GetFileName(localFile));
                    CopyLocalFileTo(localFile, remoteFile, _fileSectionBytes);
                }

                MessageBox.Show("File(s) copied", "Copy File(s)");
            }

        }

        /// <summary>
        /// Copies local file to remote. We stream sections of the file in to multiple request message
        /// </summary>
        /// <param name="localFile"></param>
        /// <param name="remoteFile"></param>
        private void CopyLocalFileTo(string localFile, string remoteFile, int sectionBytes)
        {            
            DisplayStatus($"Copying {localFile}");

            // Set remote object
            var remoteFileObject = _fileSystemLocal.GetFile(localFile);
            remoteFileObject.Path = remoteFile;

            // Start task to read file in to queue
            var queueMutex = new Mutex();
            var sectionQueue = new Queue<Tuple<byte[], bool>>();
            var readFileTask = Task.Factory.StartNew(() =>
            {
                using (var streamReader = new BinaryReader(File.OpenRead(localFile)))
                {                    
                    do
                    {
                        // Limit amount of file in memory
                        while (sectionQueue.Count > 50)
                        {
                            Thread.Sleep(100);
                        }

                        // Read section
                        var section = streamReader.ReadBytes(sectionBytes);
                        var isMore = streamReader.BaseStream.Position < streamReader.BaseStream.Length;

                        // Add to queue
                        queueMutex.WaitOne();
                        sectionQueue.Enqueue(new Tuple<byte[], bool>(section, isMore));
                        queueMutex.ReleaseMutex();

                        Thread.Yield();
                    } while (streamReader.BaseStream.Position < streamReader.BaseStream.Length);
                }
            });
            
            // Write file contents by section. Completes when final section writtn
            _fileSystem.WriteFileContentBySection(remoteFileObject, () =>
            {
                // Wait for section
                while (!sectionQueue.Any())
                {
                    Thread.Sleep(100);
                }

                // Process next item
                queueMutex.WaitOne();
                var section = sectionQueue.Dequeue();
                queueMutex.ReleaseMutex();
                return section;
            });

            // Not really necessary but wait
            readFileTask.Wait();

            DisplayStatus($"Copied {localFile}");

            int xxx = 1000;
        }
    }
}
