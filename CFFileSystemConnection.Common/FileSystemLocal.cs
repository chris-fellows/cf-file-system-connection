using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace CFFileSystemConnection.Common
{
    /// <summary>
    /// File system for local file system
    /// </summary>
    public class FileSystemLocal : IFileSystem
    {
        public void Close()
        {

        }

        public List<DriveObject> GetDrives()
        {           
            var drives = new List<DriveObject>();

            //foreach (var drive in DriveInfo.GetDrives())
            //{                                                   
            //    drives.Add(new DriveObject()
            //    {
            //        Name = drive.Name,
            //        Path = drive.RootDirectory.FullName
            //    });
            //}
            
            foreach(var drive in Environment.GetLogicalDrives())
            {
                drives.Add(new DriveObject()
                {
                    Name = drive,
                    Path = drive
                });
            }

            return drives;
        }

        public FolderObject? GetFolder(string path, bool getFiles, bool recurseSubFolders)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            System.Diagnostics.Debug.WriteLine($"Getting folder {path} (GetFiles={getFiles}, RecurseSubFolders={recurseSubFolders}");

            if (Directory.Exists(path))
            {                
                FolderObject? folderObject = null;                
                try
                {
                    folderObject = GetFolderObject(new DirectoryInfo(path));                    
                }
                catch(Exception exception)
                {
                    folderObject = new FolderObject()
                    {
                        Name = Path.GetDirectoryName(path),
                        Errors = new FolderErrors() { ErrorReading = true }
                    };
                }

                // Get files
                if (getFiles && 
                    (folderObject.Errors == null || !folderObject.Errors.ErrorReading))
                {
                    try
                    {
                        folderObject.Files = GetFiles(path);
                    }
                    catch(Exception exception)
                    {
                        if (folderObject.Errors == null) folderObject.Errors = new();
                        folderObject.Errors.ErrorReadingFiles = true;
                    }
                }

                if (recurseSubFolders)
                {
                    // Get sub-folders and all below
                    foreach (var subFolder in Directory.GetDirectories(path))
                    {
                        try
                        {                            
                            var subFolderObject = GetFolder(subFolder, getFiles, recurseSubFolders);
                            if (subFolderObject != null)
                            {
                                if (folderObject.Folders == null) folderObject.Folders = new();
                                folderObject.Folders.Add(subFolderObject);
                            }
                        }
                        catch(Exception exception)
                        {
                            folderObject.Folders.Add(new FolderObject()
                            {
                                Name = Path.GetDirectoryName(subFolder),
                                Errors =new FolderErrors() { ErrorReading = true }
                            });
                        }
                    }
                }
                else    
                {
                    // Just list sub-folders, don't include their sub-folders
                    string[] subFolders = new string[0];
                    try
                    {
                        subFolders = Directory.GetDirectories(path);
                    }
                    catch(UnauthorizedAccessException exception)
                    {
                        if (folderObject.Errors == null) folderObject.Errors = new();
                        folderObject.Errors.ErrorReadingSubFolders = true;
                    }
                    
                    foreach (var subFolder in subFolders)
                    {                        
                        var subFolderObject = GetFolderObject(new DirectoryInfo(subFolder));
                        if (subFolderObject != null)
                        {
                            if (folderObject.Folders == null) folderObject.Folders = new();
                            folderObject.Folders.Add(subFolderObject);
                        }
                    }                                        
                }

                return folderObject;
            }            

            return null;           
        }

        public void CreateFolder(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            Directory.CreateDirectory(path);
        }
        
        public FileObject? GetFile(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            return GetFileObject(path);
        }

        private static List<FileObject> GetFiles(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }


            var fileObjects = new List<FileObject>();

            if (Directory.Exists(path))
            {                
                foreach (var file in Directory.GetFiles(path))
                {
                    fileObjects.Add(GetFileObject(file)!);
                }
            }

            return fileObjects;
        }

        private static FileObject? GetFileObject(string path)
        {
            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);                

                var fileObject = new FileObject()
                {
                    Name = fileInfo.Name,
                    Path = fileInfo.FullName,
                    Length = fileInfo.Length,
                    CreatedTimeUtc = fileInfo.CreationTimeUtc,
                    UpdatedTimeUtc = fileInfo.LastWriteTimeUtc,
                    ReadOnly = fileInfo.IsReadOnly,
                    Attributes = fileInfo.Attributes.ToString(),
                    UnixFileMode = fileInfo.UnixFileMode.ToString()                    
                };                              

                return fileObject;
            }
           
            return null;
        }

        private static FolderObject GetFolderObject(DirectoryInfo directoryInfo)
        {            
            return new FolderObject()
            {
                Name = directoryInfo.Name,
                Path = directoryInfo.FullName,
                UnixFileMode = directoryInfo.UnixFileMode.ToString()
            };
        }
    
        public void GetFileContentBySection(string path, int sectionBytes, Action<byte[], bool> actionSection)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (sectionBytes <= 500)     // Only allow reasonable range
            {
                throw new ArgumentOutOfRangeException(nameof(sectionBytes));
            }

            if (File.Exists(path))
            {                
                using (var streamReader = new BinaryReader(File.OpenRead(path)))
                {                    
                    var section = new byte[0];
                    do
                    {
                        // Read section
                        section = streamReader.ReadBytes(sectionBytes);
                        
                        // Return action
                        actionSection(section,
                                     (streamReader.BaseStream.Position < streamReader.BaseStream.Length));  // IsMore

                        Thread.Yield();
                    } while (streamReader.BaseStream.Position < streamReader.BaseStream.Length);
                }
            }            
        }

        public void WriteFileContentBySection(FileObject fileObject, Func<Tuple<byte[], bool>> getSectionFunction)
        {            
            try
            {
                using (var writer = new BinaryWriter(File.OpenWrite(fileObject.Path)))
                {
                    var isMore = true;
                    while (isMore)
                    {
                        // Get next section
                        var sectionData = getSectionFunction();

                        // Write section
                        writer.Write(sectionData.Item1);

                        // Check if more data
                        isMore = sectionData.Item2;

                        Thread.Yield();
                    }
                    
                    writer.Flush();
                }

                // Apply file properties
                var fileInfo = new FileInfo(fileObject.Path);
                fileInfo.CreationTimeUtc = fileObject.CreatedTimeUtc;
                fileInfo.LastWriteTimeUtc = fileObject.UpdatedTimeUtc.Value;
                fileInfo.IsReadOnly = fileObject.ReadOnly;
                //fileInfo.UnixFileMode = ""
            }
            catch (Exception exception)
            {
                if (File.Exists(fileObject.Path)) File.Delete(fileObject.Path);
            }
        }

        public void DeleteFile(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (File.Exists(path)) File.Delete(path);
        }

        public void DeleteFolder(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (Directory.Exists(path)) Directory.Delete(path, true);
        }

        public void MoveFile(string oldPath, string newPath)
        {
            if (String.IsNullOrEmpty(oldPath))
            {
                throw new ArgumentNullException(nameof(oldPath));
            }
            if (String.IsNullOrEmpty(newPath))
            {
                throw new ArgumentNullException(nameof(newPath));
            }
            if (oldPath == newPath)
            {
                throw new ArgumentException("Old path and new path must be different");
            }
            if (!File.Exists(oldPath))
            {
                throw new FileNotFoundException(oldPath);
            }
            if (File.Exists(newPath))
            {
                throw new IOException("Cannot move file because new file already exists");
            }

            File.Move(oldPath, newPath);
        }

        public void MoveFolder(string oldPath, string newPath)
        {
            if (String.IsNullOrEmpty(oldPath))
            {
                throw new ArgumentNullException(nameof(oldPath));
            }
            if (String.IsNullOrEmpty(newPath))
            {
                throw new ArgumentNullException(nameof(newPath));
            }
            if (oldPath == newPath)
            {
                throw new ArgumentException("Old path and new path must be different");
            }
            if (!Directory.Exists(oldPath))
            {
                throw new IOException("Folder does not exist");
            }
            if (File.Exists(newPath))
            {
                throw new IOException("Cannot move folder because new folder already exists");
            }

            Directory.Move(oldPath, newPath);
        }

        public string PathCombine(params string[] elements)
        {
            return Path.Combine(elements);
        }
    }
}
