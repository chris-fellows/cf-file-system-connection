//using CFFileSystemConnection.Interfaces;
//using CFFileSystemConnection.Models;
//using CFFileSystemManager.Utilities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CFFileSystemManager
//{
//    internal class FolderSync
//    {
//        /// <summary>
//        /// Syncs source folder to destination folder
//        /// </summary>
//        /// <param name="folderSrc"></param>
//        /// <param name="fileSystemSrc"></param>
//        /// <param name="folderDst"></param>
//        /// <param name="fileSystemDst"></param>
//        /// <param name="fileSectionBytes"></param>
//        /// <param name="statusAction"></param>
//        public void SyncFolder(string folderSrc,
//                                IFileSystem fileSystemSrc,
//                                string folderDst,
//                                IFileSystem fileSystemDst,
//                                int fileSectionBytes,
//                                Action<string> statusAction)
//        {
//            var folderObjectSrc = fileSystemSrc.GetFolder(folderSrc, true, false);

//            var folderObjectDst = fileSystemSrc.GetFolder(folderSrc, true, false);

//            if (folderObjectDst == null)   // Dest folder does not exist
//            {
//                fileSystemDst.CreateFolder(folderDst);
//                folderObjectDst = fileSystemDst.GetFolder(folderDst, true, false);
//            }

//            if (folderObjectSrc.Files != null)
//            {
//                // Copy files from src to dst
//                foreach (var fileObjectSrc in folderObjectSrc.Files)
//                {
//                    var fileObjectDst = folderObjectDst.Files.FirstOrDefault(f => f.Name == fileObjectSrc.Name);

//                    SyncFile(fileObjectSrc, fileSystemSrc, fileObjectDst, 
//                                    fileSystemDst, fileSectionBytes,
//                                    statusAction);

//                }

//                // Delete dst files not in src any more
//                foreach (var fileObjectDst in folderObjectDst.Files.Where(fileDst => !folderObjectSrc.Files.Any(f => f.Name == fileDst.Name)))
//                {
//                    //fileSystemDst.DeleteFile(fileObjectDst.Path);
//                }

//            }

//            if (folderObjectSrc.Files != null)
//            {
//                // Sync sub-folders from src to dst
//                foreach (var subFolderSrc in folderObjectSrc.Folders)
//                {
//                    var subFolderDstPath = Path.Combine(folderDst, subFolderSrc.Name);

//                    SyncFolder(subFolderSrc.Path, fileSystemSrc,
//                        subFolderDstPath, fileSystemDst,
//                        fileSectionBytes, statusAction);
//                }
//            }
//        }

//        /// <summary>
//        /// Syncs source file to destination file
//        /// </summary>
//        /// <param name="fileObjectSrc"></param>
//        /// <param name="fileSystemSrc"></param>
//        /// <param name="fileObjectDst"></param>
//        /// <param name="fileSystemDst"></param>
//        /// <param name="fileSectionBytes"></param>
//        /// <param name="statusAction"></param>
//        private void SyncFile(FileObject fileObjectSrc,
//                              IFileSystem fileSystemSrc,
//                              FileObject? fileObjectDst,
//                              IFileSystem fileSystemDst,
//                              int fileSectionBytes,
//                              Action<string> statusAction)
//        {
//            var isCopyFile = false;

//            if (fileObjectDst == null)  // New file
//            {
//                isCopyFile = true;                
//            }
//            else if (fileObjectSrc.Length == fileObjectDst.Length)   // Same size, check if different
//            {
//                if (fileObjectSrc.UpdatedTimeUtc != null &&
//                    fileObjectDst.UpdatedTimeUtc != null)
//                {
//                    TimeSpan diff = fileObjectSrc.UpdatedTimeUtc.Value - fileObjectDst.UpdatedTimeUtc.Value;
//                    if (Math.Abs(diff.TotalSeconds) >= 60)    // Assume same if timestamp similar
//                    {
//                        isCopyFile = true;
//                    }
//                }               
//            }
//            else   // Different file size
//            {
//                isCopyFile = true;
//            }

//            if (isCopyFile)
//            {                
//                FileSystemUtilities.CopyFileBetween(fileSystemSrc, fileObjectSrc.Path,
//                                    fileSystemDst, fileObjectDst.Path,
//                                    fileSectionBytes,
//                                    statusAction);
//            }
//        }
//    }
//}
