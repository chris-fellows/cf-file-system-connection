using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.Common
{
    /// <summary>
    /// File system for local file system
    /// </summary>
    public class FileSystemLocal : IFileSystem
    {
        public List<DriveObject> GetDrives()
        {
            var drives = new List<DriveObject>();

            foreach (var drive in DriveInfo.GetDrives())
            {
                drives.Add(new DriveObject()
                {
                    Name = drive.Name,
                    Path = drive.RootDirectory.FullName
                });
            }

            return drives;
        }

        public FolderObject? GetFolder(string path, bool getFiles, bool recurseSubFolders)
        {
            if (Directory.Exists(path))
            {
                FolderObject? folderObject = null;                
                try
                {
                    folderObject = GetFolderInfo(new DirectoryInfo(path));                    
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
                if (getFiles && (folderObject.Errors == null || !folderObject.Errors.ErrorReading))
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
                            //folderObject.Items.Add(subFolderObject!);
                            folderObject.Folders.Add(subFolderObject);
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

                    if (subFolders.Any())
                    {
                        foreach (var subFolder in subFolders)
                        {
                            var subFolderObject = GetFolder(subFolder, false, false);
                            folderObject.Folders.Add(subFolderObject!);
                        }
                    }
                }

                return folderObject;
            }

            return null;           
        }
        
        public FileObject? GetFile(string path)
        {
            return GetFileObject(path);
        }

        private static List<FileObject> GetFiles(string path)
        {
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

        private static FolderObject GetFolderInfo(DirectoryInfo directoryInfo)
        {            
            return new FolderObject()
            {
                Name = directoryInfo.Name,
                Path = directoryInfo.FullName,
                UnixFileMode = directoryInfo.UnixFileMode.ToString()
            };
        }

        public byte[]? GetFileContent(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }

            return null;
        }
    }
}
