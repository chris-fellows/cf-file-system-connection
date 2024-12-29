using CFFileSystemConnection.Common;
using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemManager.Utilities
{
    /// <summary>
    /// File system (IFileSystem) utilities
    /// </summary>
    internal static class FileSystemUtilities
    {        
        /// <summary>
        /// Copy remote folder to local folder
        /// </summary>
        /// <param name="folderObject"></param>
        /// <param name="localFolder"></param>
        public static void CopyFolderToLocal(IFileSystem fileSystem, FolderObject folderObject, string localFolder,
                                    int fileSectionBytes,
                                    Action<string> statusAction)
        {
            statusAction($"Copying {folderObject.Path}");

            Directory.CreateDirectory(localFolder);

            // Get folder file list
            var folderObjectFull = fileSystem.GetFolder(folderObject.Path, true, false);
            Thread.Yield();

            // Copy files
            if (folderObjectFull.Files != null)
            {
                foreach (var fileObject in folderObjectFull.Files)
                {
                    // Get file content                    
                    statusAction($"Copying {fileObject.Path}");
                    CopyFileToLocal(fileSystem, fileObject, Path.Combine(localFolder, fileObject.Name), fileSectionBytes);
                    Thread.Yield();
                }
            }

            // Copy sub-folders
            if (folderObjectFull.Folders != null)
            {
                foreach (var subFolderObject in folderObjectFull.Folders)
                {
                    CopyFolderToLocal(fileSystem, subFolderObject, Path.Combine(localFolder, subFolderObject.Name),
                                fileSectionBytes, statusAction);
                }
            }

            statusAction($"Copyied {folderObject.Path}");
        }


        /// <summary>
        /// Copies remote file to local
        /// </summary>
        /// <param name="fileObject"></param>
        /// <param name="localFile"></param>
        public static void CopyFileToLocal(IFileSystem fileSystem, FileObject fileObject, string localFile, int fileSectionBytes)
        {
            using (var writer = new BinaryWriter(File.OpenWrite(localFile)))
            {
                fileSystem.GetFileContentBySection(fileObject.Path, fileSectionBytes, (section, isMore) =>
                {
                    writer.Write(section);                    
                });
                writer.Flush();
            }
        }

        /// <summary>
        /// Copies folder between file systems
        /// </summary>
        /// <param name="fileSystemSrc"></param>
        /// <param name="folderSrc"></param>
        /// <param name="fileSystemDst"></param>
        /// <param name="folderDst"></param>
        /// <param name="fileSectionBytes"></param>
        /// <param name="statusAction"></param>
        public static void CopyFolderBetween(IFileSystem fileSystemSrc,
                                            string folderSrc,
                                            IFileSystem fileSystemDst,
                                            string folderDst,
                                            int fileSectionBytes,
                                            Action<string> statusAction)
        {
            statusAction($"Copying {folderSrc}");

            // Get source folder
            var folderObjectSrc = fileSystemSrc.GetFolder(folderSrc, true, false);

            // Get destination folder
            var folderObjectDst = fileSystemDst.GetFolder(folderDst, true, false);

            // Create destination folder
            if (folderObjectDst == null) 
            {
                fileSystemDst.CreateFolder(folderDst);
                folderObjectDst = fileSystemDst.GetFolder(folderDst, true, false);
            }            

            // Copy files
            if (folderObjectSrc.Files != null)
            {
                foreach(var fileSrc in folderObjectSrc.Files)
                {                    
                    var fileDstPath = fileSystemDst.PathCombine(folderDst, fileSrc.Name);

                    CopyFileBetween(fileSystemSrc, fileSrc.Path,
                        fileSystemDst, fileDstPath,
                        fileSectionBytes,
                        statusAction);                                                            
                }
            }
            
            // Copy sub-folders
            if (folderObjectSrc.Folders != null)
            {
                foreach(var subFolderSrc in folderObjectSrc.Folders)
                {
                    var subFolderDstPath = fileSystemDst.PathCombine(folderObjectDst.Path, subFolderSrc.Name);

                    CopyFolderBetween(fileSystemSrc, subFolderSrc.Path,
                        fileSystemDst, subFolderDstPath,
                        fileSectionBytes,
                        statusAction);
                }
            }            

            statusAction($"Copied {folderSrc}");
        }

        /// <summary>
        /// Copies local file to remote. We stream sections of the file in to multiple request messages
        /// </summary>
        /// <param name="localFile"></param>
        /// <param name="remoteFile"></param>
        public static void CopyFileBetween(IFileSystem fileSystemSrc,
                                        string fileSrc,
                                        IFileSystem fileSystemDst,
                                        string fileDst,                                        
                                        int fileSectionBytes,
                                        Action<string> statusAction)
        {
            statusAction($"Copying {fileSrc}");

            // Set remote object
            var remoteFileObject = fileSystemSrc.GetFile(fileSrc);
            remoteFileObject.Path = fileDst;

            // Start task to read file in to queue
            var queueMutex = new Mutex();
            var sectionQueue = new Queue<Tuple<byte[], bool>>();
            var readFileTask = Task.Factory.StartNew(() =>
            {
                using (var streamReader = new BinaryReader(File.OpenRead(fileSrc)))
                {
                    do
                    {
                        // Limit amount of file in memory
                        while (sectionQueue.Count > 50)
                        {
                            Thread.Sleep(100);
                        }

                        // Read section
                        var section = streamReader.ReadBytes(fileSectionBytes);
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
            fileSystemDst.WriteFileContentBySection(remoteFileObject, () =>
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

            statusAction($"Copied {fileSrc}");            
        }

        ///// <summary>
        ///// Implementation of Path.Combine that works for different platforms.
        ///// </summary>
        ///// <param name="delimiter"></param>
        ///// <param name="elements"></param>
        ///// <returns></returns>
        //public static string PathCombine(Char delimiter,
        //                                params string[] elements)
        //{
        //    if (delimiter != Path.DirectorySeparatorChar)
        //    {
        //        var path = new StringBuilder("");
        //        foreach (var element in elements)
        //        {
        //            if (path.Length > 0) path.Append(delimiter);
        //            path.Append(element);
        //        }
        //        return path.ToString();               
        //    }

        //    return Path.Combine(elements);      // Default
        //}
    }
}
