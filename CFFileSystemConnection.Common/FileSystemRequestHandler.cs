using CFConnectionMessaging;
using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Constants;
using CFFileSystemConnection.Enums;
using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.MessageConverters;
using CFFileSystemConnection.Models;

namespace CFFileSystemConnection.Common
{
    /// <summary>
    /// Handles request messages for file system messages.
    /// 
    /// This class should be used by the client that is providing access to it's file system. The remote client
    /// will instantiate FileSystemConnection (IFileSystem) which sends messages to this class.
    /// </summary>
    public class FileSystemRequestHandler : IDisposable
    {
        private ConnectionTcp _connection;

        public delegate void StatusMessage(string message);
        public event StatusMessage? OnStatusMessage;

        /// <summary>
        /// Interface for accesing the local file system.
        /// </summary>
        private readonly IFileSystem _fileSystemLocal;

        private readonly IUserService _userService;

        // Message converters
        private readonly IExternalMessageConverter<DeleteRequest> _deleteRequestConverter = new DeleteRequestMessageConverter();
        private readonly IExternalMessageConverter<DeleteResponse> _deleteResponseConverter = new DeleteResponseMessageConverter();

        private readonly IExternalMessageConverter<GetDrivesRequest> _getDrivesRequestConverter = new GetDrivesRequestMessageConverter();
        private readonly IExternalMessageConverter<GetDrivesResponse> _getDrivesResponseConverter = new GetDrivesResponseMessageConverter();

        private readonly IExternalMessageConverter<GetFolderRequest> _getFolderRequestConverter = new GetFolderRequestMessageConverter();
        private readonly IExternalMessageConverter<GetFolderResponse> _getFolderResponseConverter = new GetFolderResponseMessageConverter();

        private readonly IExternalMessageConverter<GetFileContentRequest> _getFileContentRequestConverter = new GetFileContentRequestMessageConverter();
        private readonly IExternalMessageConverter<GetFileContentResponse> _getFileContentResponseConverter = new GetFileContentResponseMessageConverter();

        private readonly IExternalMessageConverter<GetFileRequest> _getFileRequestConverter = new GetFileRequestMessageConverter();
        private readonly IExternalMessageConverter<GetFileResponse> _getFileResponseConverter = new GetFileResponseMessageConverter();

        private readonly IExternalMessageConverter<MoveRequest> _moveRequestConverter = new MoveRequestMessageConverter();
        private readonly IExternalMessageConverter<MoveResponse> _moveResponseConverter = new MoveResponseMessageConverter();

        private readonly IExternalMessageConverter<WriteFileRequest> _writeFileRequestConverter = new WriteFileRequestMessageConverter();
        private readonly IExternalMessageConverter<WriteFileResponse> _writeFileResponseConverter = new WriteFileResponseMessageConverter();

        /// <summary>
        /// Details of a file write session. Created when we receive first request to write file X. Deleted
        /// when final request with last file section processed or there's a timeout
        /// </summary>
        private class FileWriteSession
        {
            /// <summary>
            /// Session Id for all request messages to have
            /// </summary>
            public string SessionId { get; set; } = String.Empty;

            /// <summary>
            /// Time last section received. Used to detect timeouts if client stops sending while writing a 
            /// file
            /// </summary>
            public DateTime LastSectionReceived { get; set; } = DateTime.MinValue;

            /// <summary>
            /// Temp file accumulating sections until whole file is received
            /// </summary>
            public string TempFile { get; set; } = String.Empty;
        }

        /// <summary>
        /// List of active file write sessions.
        /// </summary>
        private List<FileWriteSession> _fileWriteSessions = new List<FileWriteSession>();

        public FileSystemRequestHandler(IFileSystem fileSystemLocal,
                                        IUserService userService)
        {
            _fileSystemLocal = fileSystemLocal;
            _userService = userService;

            _connection = new ConnectionTcp();
            _connection.OnConnectionMessageReceived += _connection_OnConnectionMessageReceived;            
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.StopListening();
                _connection.OnConnectionMessageReceived -= _connection_OnConnectionMessageReceived;
                _connection = null;
            }

            // Delete all temp files for active file writes
            _fileWriteSessions.ForEach(fws => fws.LastSectionReceived = DateTime.UtcNow.Subtract(TimeSpan.FromDays(10)));
            CleanUpFileWriteSessions(TimeSpan.FromMilliseconds(1));
        }

        private void _connection_OnConnectionMessageReceived(ConnectionMessage connectionMessage, MessageReceivedInfo messageReceivedInfo)
        {
            if (OnStatusMessage != null)
            {
                OnStatusMessage($"Received message {connectionMessage.Id} {connectionMessage.TypeId} from " +
                        $"{messageReceivedInfo.RemoteEndpointInfo.Ip}:{messageReceivedInfo.RemoteEndpointInfo.Port}");
            }            

            // Handle message. Should only be a request
            switch(connectionMessage.TypeId)
            {
                case MessageTypeIds.DeleteRequest:
                    var deleteRequest = _deleteRequestConverter.GetExternalMessage(connectionMessage);
                    HandleDeleteRequest(deleteRequest, messageReceivedInfo);
                    break;
                case MessageTypeIds.GetDrivesRequest:
                    var getDrivesRequest = _getDrivesRequestConverter.GetExternalMessage(connectionMessage);
                    HandleGetDrivesRequest(getDrivesRequest, messageReceivedInfo);
                    break;
                case MessageTypeIds.GetFileContentRequest:                    
                    var getFileContentRequest = _getFileContentRequestConverter.GetExternalMessage(connectionMessage);
                    HandleGetFileContentRequest(getFileContentRequest, messageReceivedInfo);
                    break;
                case MessageTypeIds.GetFileRequest:                    
                    var getFileRequest = _getFileRequestConverter.GetExternalMessage(connectionMessage);
                    HandleGetFileRequest(getFileRequest, messageReceivedInfo);
                    break;
                case MessageTypeIds.GetFolderRequest:                    
                    var getFolderRequest = _getFolderRequestConverter.GetExternalMessage(connectionMessage);                    
                    HandleGetFolderRequest(getFolderRequest, messageReceivedInfo);
                    break;
                case MessageTypeIds.MoveRequest:
                    var moveRequest = _moveRequestConverter.GetExternalMessage(connectionMessage);
                    HandleMoveRequest(moveRequest, messageReceivedInfo);
                    break;
                case MessageTypeIds.WriteFileRequest:                    
                    var writeFileRequest = _writeFileRequestConverter.GetExternalMessage(connectionMessage);                        
                    HandleWriteFileRequest(writeFileRequest, messageReceivedInfo);                    
                    break;
                default:
                    if (OnStatusMessage != null)
                    {
                        OnStatusMessage($"No processing for message {connectionMessage.Id} {connectionMessage.TypeId} from " +
                                $"{messageReceivedInfo.RemoteEndpointInfo.Ip}:{messageReceivedInfo.RemoteEndpointInfo.Port}");
                    }
                    break;
            }

            // Bit of a hack but do it here. Saves us using a timer or new thread
            CleanUpFileWriteSessions(TimeSpan.FromSeconds(300));
        }

        public void StartListening(int port)
        {
            _connection.ReceivePort = port;
            _connection.StartListening();
        }

        public void StopListening()
        {
            _connection.StopListening();
        }

        public bool IsListening => _connection.IsListening;     

        /// <summary>
        /// Whether user can access the path
        /// </summary>
        /// <param name="user"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsUserCanAccessFolder(User user, string path)
        {
            if (user.Permissions.Paths != null &&
                !user.Permissions.Paths.Any(drivePath => path.StartsWith(drivePath)))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Handles GetDrivesRequest message
        /// </summary>
        /// <param name="getFolderRequest"></param>
        private void HandleGetDrivesRequest(GetDrivesRequest getDrivesRequest, MessageReceivedInfo messageReceivedInfo)
        {
            try
            {
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Processing request {getDrivesRequest.Id} to get drives");
                }

                var getDrivesResponse = new GetDrivesResponse()
                {
                    Id = Guid.NewGuid().ToString(),
                    TypeId = MessageTypeIds.GetDrivesResponse,
                    Response = new MessageResponse()
                    {
                        MessageId = getDrivesRequest.Id,
                        IsMore = false
                    }
                };

                // Check permissions
                var user = _userService.GetBySecurityKey(getDrivesRequest.SecurityKey);
                if (user == null ||
                    !user.Roles.Contains(UserRoles.FileSystemRead))   // Invalid credentials
                {
                    getDrivesResponse.Response.ErrorMessage = "Permission denied";
                    getDrivesResponse.Response.ErrorCode = ResponseErrorCodes.PermissionDenied;
                }
                else     // Valid credentials
                {
                    try
                    {
                        getDrivesResponse.Drives = _fileSystemLocal.GetDrives();
                        if (getDrivesResponse.Drives == null)
                        {
                            getDrivesResponse.Response.ErrorCode = ResponseErrorCodes.Unknown;
                            getDrivesResponse.Response.ErrorMessage = "No drives available";
                        }
                        else if (user.Permissions.Paths != null)   // Exclude drives that user can't access
                        {
                            getDrivesResponse.Drives = getDrivesResponse.Drives.Where(d => IsUserCanAccessFolder(user, d.Path)).ToList();
                        }
                    }
                    catch (Exception exception)
                    {
                        getDrivesResponse.Response.ErrorMessage = exception.Message;
                        getDrivesResponse.Response.ErrorCode = ResponseErrorCodes.FileSystemError;
                    }
                }

                // Send response
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Sending response for get drives request {getDrivesRequest.Id} to " +
                                $"{messageReceivedInfo.RemoteEndpointInfo.Ip}:{messageReceivedInfo.RemoteEndpointInfo.Port}");
                }
                _connection.SendMessage(_getDrivesResponseConverter.GetConnectionMessage(getDrivesResponse),
                                    messageReceivedInfo.RemoteEndpointInfo);

                if (OnStatusMessage != null)
                {
                    if (getDrivesResponse.Response.ErrorCode == null)
                    {
                        OnStatusMessage($"Processed request {getDrivesRequest.Id} to get drives");
                    }
                    else
                    {
                        OnStatusMessage($"Processed request {getDrivesRequest.Id} to get drives: {getDrivesResponse.Response.ErrorCode}");
                    }
                }
            }
            catch(Exception exception)
            {
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Error processing request {getDrivesRequest.Id} to get drives: {exception.Message}");
                }
            }
        }

        /// <summary>
        /// Handles GetFolderRequest message
        /// </summary>
        /// <param name="getFolderRequest"></param>
        private void HandleGetFolderRequest(GetFolderRequest getFolderRequest, MessageReceivedInfo messageReceivedInfo)
        {
            try
            {
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Processing request {getFolderRequest.Id} to get folder {getFolderRequest.Path}");
                }

                var getFolderResponse = new GetFolderResponse()
                {
                    Id = Guid.NewGuid().ToString(),
                    TypeId = MessageTypeIds.GetFolderResponse,
                    Response = new MessageResponse()
                    {
                        MessageId = getFolderRequest.Id,
                        IsMore = false
                    }
                };

                // Check permissions
                var user = _userService.GetBySecurityKey(getFolderRequest.SecurityKey);
                if (user == null ||
                    !user.Roles.Contains(UserRoles.FileSystemRead))   // Invalid credentials
                {
                    getFolderResponse.Response.ErrorMessage = "Permission denied";
                    getFolderResponse.Response.ErrorCode = ResponseErrorCodes.PermissionDenied;
                }
                else if (!IsUserCanAccessFolder(user, getFolderRequest.Path))
                {
                    getFolderResponse.Response.ErrorMessage = "Permission denied";
                    getFolderResponse.Response.ErrorCode = ResponseErrorCodes.PermissionDenied;
                }
                else     // Valid credentials
                {
                    try
                    {
                        getFolderResponse.FolderObject = _fileSystemLocal.GetFolder(getFolderRequest.Path,
                                                getFolderRequest.GetFiles, getFolderRequest.RecurseSubFolders);
                        if (getFolderResponse.FolderObject == null)
                        {
                            getFolderResponse.Response.ErrorCode = ResponseErrorCodes.DirectoryDoesNotExist;
                            getFolderResponse.Response.ErrorMessage = "Directory does not exist";
                        }
                    }
                    catch (Exception exception)
                    {
                        getFolderResponse.Response.ErrorMessage = exception.Message;
                        getFolderResponse.Response.ErrorCode = ResponseErrorCodes.FileSystemError;
                    }
                }

                // Send response
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Sending response for get folder request {getFolderRequest.Id} to " +
                                $"{messageReceivedInfo.RemoteEndpointInfo.Ip}:{messageReceivedInfo.RemoteEndpointInfo.Port}");
                }
                _connection.SendMessage(_getFolderResponseConverter.GetConnectionMessage(getFolderResponse),
                                    messageReceivedInfo.RemoteEndpointInfo);

                if (OnStatusMessage != null)
                {
                    if (getFolderResponse.Response.ErrorCode == null)
                    {
                        OnStatusMessage($"Processed request {getFolderRequest.Id} to get folder {getFolderRequest.Path}");
                    }
                    else
                    {
                        OnStatusMessage($"Processed request {getFolderRequest.Id} to get folder {getFolderRequest.Path} ({getFolderResponse.Response.ErrorCode})");
                    }
                }
            }
            catch (Exception exception)
            {
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Error processing request {getFolderRequest.Id} to get folder {getFolderRequest.Path}: {exception.Message}");
                }
            }
        }

        /// <summary>
        /// Handles GetFileRequest message
        /// </summary>
        /// <param name="getFolderRequest"></param>
        private void HandleGetFileRequest(GetFileRequest getFileRequest, MessageReceivedInfo messageReceivedInfo)
        {
            try
            {
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Processing request {getFileRequest.Id} to get file {getFileRequest.Path}");
                }

                var getFileResponse = new GetFileResponse()
                {
                    Id = Guid.NewGuid().ToString(),
                    TypeId = MessageTypeIds.GetFolderResponse,
                    Response = new MessageResponse()
                    {
                        MessageId = getFileRequest.Id,
                        IsMore = false
                    }
                };

                var user = _userService.GetBySecurityKey(getFileRequest.SecurityKey);
                if (user == null ||
                    !user.Roles.Contains(UserRoles.FileSystemRead))   // Invalid credentials
                {
                    getFileResponse.Response.ErrorMessage = "Permission denied";
                    getFileResponse.Response.ErrorCode = ResponseErrorCodes.PermissionDenied;
                }
                else if (!IsUserCanAccessFolder(user, Path.GetDirectoryName(getFileRequest.Path)))
                {
                    getFileResponse.Response.ErrorMessage = "Permission denied";
                    getFileResponse.Response.ErrorCode = ResponseErrorCodes.PermissionDenied;
                }
                else    // Valid credentials
                {
                    try
                    {
                        getFileResponse.FileObject = _fileSystemLocal.GetFile(getFileRequest.Path);
                        if (getFileResponse.FileObject == null)
                        {
                            getFileResponse.Response.ErrorCode = ResponseErrorCodes.FileDoesNotExist;
                            getFileResponse.Response.ErrorMessage = "File does not exist";
                        }
                    }
                    catch (Exception exception)
                    {
                        getFileResponse.Response.ErrorMessage = exception.Message;
                        getFileResponse.Response.ErrorCode = ResponseErrorCodes.FileSystemError;
                    }
                }

                // Send response
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Sending response for get file request {getFileRequest.Id} to " +
                                $"{messageReceivedInfo.RemoteEndpointInfo.Ip}:{messageReceivedInfo.RemoteEndpointInfo.Port}");
                }

                _connection.SendMessage(_getFileResponseConverter.GetConnectionMessage(getFileResponse),
                                    messageReceivedInfo.RemoteEndpointInfo);

                if (OnStatusMessage != null)
                {
                    if (getFileResponse.Response.ErrorCode == null)
                    {
                        OnStatusMessage($"Processed request {getFileRequest.Id} to get file {getFileRequest.Path}");
                    }
                    else
                    {
                        OnStatusMessage($"Processed request {getFileRequest.Id} to get file {getFileRequest.Path} ({getFileResponse.Response.ErrorCode})");
                    }
                }
            }
            catch(Exception exception)
            {
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Error processing request {getFileRequest.Id} to get file {getFileRequest.Path}: {exception.Message}");
                }
            }
        }

        /// <summary>
        /// Handles GetFileContentRequest message.
        /// 
        /// We send back multiple GetFileContentResponse messages with file sections and the last one will be set as
        /// GetFileContentResponse.Response.IsMore=false.
        /// </summary>
        /// <param name="getFileContentRequest"></param>
        private void HandleGetFileContentRequest(GetFileContentRequest getFileContentRequest, MessageReceivedInfo messageReceivedInfo)
        {
            try
            {
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Processing request {getFileContentRequest.Id} to get file content for {getFileContentRequest.Path}");
                }

                var getFileContentResponse = new GetFileContentResponse()
                {
                    Id = Guid.NewGuid().ToString(),
                    TypeId = MessageTypeIds.GetFileContentResponse,
                    Response = new MessageResponse()
                    {
                        MessageId = getFileContentRequest.Id,
                        IsMore = false
                    }
                };

                var user = _userService.GetBySecurityKey(getFileContentRequest.SecurityKey);
                if (user == null ||
                    !user.Roles.Contains(UserRoles.FileSystemRead))   // Invalid credentials
                {
                    getFileContentResponse.Response.ErrorMessage = "Permission denied";
                    getFileContentResponse.Response.ErrorCode = ResponseErrorCodes.PermissionDenied;
                }
                else if (!IsUserCanAccessFolder(user, Path.GetDirectoryName(getFileContentRequest.Path)))
                {
                    getFileContentResponse.Response.ErrorMessage = "Permission denied";
                    getFileContentResponse.Response.ErrorCode = ResponseErrorCodes.PermissionDenied;
                }
                else    // Valid credentials
                {
                    try
                    {
                        // Check that file exists
                        var fileObject = _fileSystemLocal.GetFile(getFileContentRequest.Path);

                        if (fileObject == null)
                        {
                            getFileContentResponse.Response.ErrorMessage = "File does not exist";
                            getFileContentResponse.Response.ErrorCode = ResponseErrorCodes.FileDoesNotExist;
                        }
                        else   // File exists, return it in sections
                        {
                            int fileSectionSequence = -1;

                            _fileSystemLocal.GetFileContentBySection(getFileContentRequest.Path, getFileContentRequest.SectionBytes,
                                (sectionBytes, isMore) =>     // Handle file sections
                                {
                                    fileSectionSequence++;
                                    var getFileContentResponseSection = new GetFileContentResponse()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        TypeId = MessageTypeIds.GetFileContentResponse,
                                        Content = sectionBytes,
                                        Response = new MessageResponse()
                                        {
                                            MessageId = getFileContentRequest.Id,
                                            IsMore = isMore,
                                            Sequence = fileSectionSequence
                                        }
                                    };

                                    // Send section
                                    _connection.SendMessage(_getFileContentResponseConverter.GetConnectionMessage(getFileContentResponseSection),
                                                        messageReceivedInfo.RemoteEndpointInfo);
                                    Thread.Yield();
                                });
                        }
                    }
                    catch (Exception exception)
                    {
                        getFileContentResponse.Response.ErrorMessage = exception.Message;
                        getFileContentResponse.Response.ErrorCode = ResponseErrorCodes.FileSystemError;
                    }
                }

                // Send response if error
                if (getFileContentResponse.Response.ErrorCode != null)
                {
                    // Send response
                    if (OnStatusMessage != null)
                    {
                        OnStatusMessage($"Sending response for get file content request {getFileContentRequest.Id} to " +
                                    $"{messageReceivedInfo.RemoteEndpointInfo.Ip}:{messageReceivedInfo.RemoteEndpointInfo.Port}");
                    }

                    _connection.SendMessage(_getFileContentResponseConverter.GetConnectionMessage(getFileContentResponse),
                                        messageReceivedInfo.RemoteEndpointInfo);
                }

                if (OnStatusMessage != null)
                {                    
                    OnStatusMessage($"Processed request {getFileContentRequest.Id} to get file content for {getFileContentRequest.Path}");
                }
            }
            catch(Exception exception)
            {
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Error processing request {getFileContentRequest.Id} to get file content for {getFileContentRequest.Path}: {exception.Message}");
                }
            }
        }

        private void HandleWriteFileRequest(WriteFileRequest writeFileRequest, MessageReceivedInfo messageReceivedInfo)
        {
            try
            {
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Processing request {writeFileRequest.Id} to write file {writeFileRequest.FileObject.Path}");
                }

                var writeFileResponse = new WriteFileResponse()
                {
                    Id = Guid.NewGuid().ToString(),
                    TypeId = MessageTypeIds.WriteFileResponse,
                    SessionId = writeFileRequest.SessionId,
                    Response = new MessageResponse()
                    {
                        MessageId = writeFileRequest.Id,
                        IsMore = false
                    }
                };

                // Check permissions
                var user = _userService.GetBySecurityKey(writeFileRequest.SecurityKey);
                if (user == null ||
                    !user.Roles.Contains(UserRoles.FileSystemWrite))   // Invalid credentials
                {
                    writeFileResponse.Response.ErrorMessage = "Permission denied";
                    writeFileResponse.Response.ErrorCode = ResponseErrorCodes.PermissionDenied;
                }
                else if (!IsUserCanAccessFolder(user, writeFileRequest.FileObject.Path))   // User does't have access to path
                {
                    writeFileResponse.Response.ErrorMessage = "Permission denied";
                    writeFileResponse.Response.ErrorCode = ResponseErrorCodes.PermissionDenied;
                }
                else     // Valid credentials
                {
                    try
                    {
                        // Check if sesssion active
                        var fileWriteSession = _fileWriteSessions.FirstOrDefault(fws => fws.SessionId == writeFileRequest.SessionId);
                        if (fileWriteSession == null)   // Start new session
                        {
                            // We'll create a temp file in the destination folder
                            fileWriteSession = new FileWriteSession()
                            {
                                SessionId = writeFileRequest.SessionId,                                                                     
                                TempFile = GetUniqueFileName(Path.GetDirectoryName(writeFileRequest.FileObject.Path), ".tmp")
                            };
                            Directory.CreateDirectory(Path.GetDirectoryName(fileWriteSession.TempFile));
                            _fileWriteSessions.Add(fileWriteSession);
                        }
                        fileWriteSession.LastSectionReceived = DateTime.UtcNow;

                        // Write section to temp file
                        using (var writer = new BinaryWriter(File.OpenWrite(fileWriteSession.TempFile)))
                        {
                            writer.Seek(0, SeekOrigin.End);
                            writer.Write(writeFileRequest.Content);
                        }
                        
                        // If all sections received then copy temp file to destination                        
                        if (!writeFileRequest.IsMore)
                        {
                            // Check that the temp file is the correct size before we move it
                            var fileInfo = new FileInfo(fileWriteSession.TempFile);
                            if (fileInfo.Length == writeFileRequest.FileObject.Length)
                            {
                                // Move file
                                File.Move(fileWriteSession.TempFile, writeFileRequest.FileObject.Path);
                            }
                            else
                            {
                                writeFileResponse.Response.ErrorMessage = "Received file size is different to expected";
                                writeFileResponse.Response.ErrorCode = ResponseErrorCodes.FileSystemError;
                            }

                            // Clean up
                            File.Delete(fileWriteSession.TempFile);
                            _fileWriteSessions.Remove(fileWriteSession);
                        }
                    }
                    catch (Exception exception)
                    {
                        writeFileResponse.Response.ErrorMessage = exception.Message;
                        writeFileResponse.Response.ErrorCode = ResponseErrorCodes.FileSystemError;
                    }
                }

                // Send response
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Sending response for get folder request {writeFileRequest.Id} to " +
                                $"{messageReceivedInfo.RemoteEndpointInfo.Ip}:{messageReceivedInfo.RemoteEndpointInfo.Port}");
                }
                _connection.SendMessage(_writeFileResponseConverter.GetConnectionMessage(writeFileResponse),
                                    messageReceivedInfo.RemoteEndpointInfo);

                if (OnStatusMessage != null)
                {
                    if (writeFileResponse.Response.ErrorCode == null)
                    {
                        OnStatusMessage($"Processed request {writeFileRequest.Id} to write file {writeFileRequest.FileObject.Path}");
                    }
                    else
                    {
                        OnStatusMessage($"Processed request {writeFileRequest.Id} to write file {writeFileRequest.FileObject.Path} ({writeFileResponse.Response.ErrorCode})");
                    }
                }
            }
            catch (Exception exception)
            {
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Error processing request {writeFileRequest.Id} to write file {writeFileRequest.FileObject.Path}: {exception.Message}");
                }
            }
        }

        /// <summary>
        /// Creates a unique file name in the folder
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        private static string GetUniqueFileName(string folder, string extension)
        {
            string file = "";
            do
            {
                file = Path.Combine(folder, $"{Guid.NewGuid().ToString()}{extension}");
            } while (File.Exists(file));

            return file;
        }

        /// <summary>
        /// Cleans up file write sessions that have timed out receiving all file sections
        /// </summary>
        private void CleanUpFileWriteSessions(TimeSpan timeout)
        {
            var fileWriteSessions = _fileWriteSessions.Where(fws => 
                        fws.LastSectionReceived.Add(timeout) < DateTime.UtcNow).ToList();

            while (fileWriteSessions.Any())
            {
                var fileWriteSession = fileWriteSessions.First();
                fileWriteSessions.Remove(fileWriteSession);
                _fileWriteSessions.Remove(fileWriteSession);

                // Delete temp file
                if (File.Exists(fileWriteSession.TempFile))
                {
                    File.Delete(fileWriteSession.TempFile);
                }
            }
        }

        /// <summary>
        /// Reads temp file in to section queue variable until all file read. Limits size of queue in order to restrict
        /// amount of memory consumed.
        /// </summary>
        /// <param name="fileWriteSession"></param>
        /// <param name="sectionBytes"></param>
        /// <param name="sectionQueue"></param>
        /// <param name="queueMutex"></param>
        /// <returns></returns>
        private Task LoadTempFileSectionsToQueueAsync(FileWriteSession fileWriteSession,
                                int sectionBytes,
                                Queue<Tuple<byte[], bool>> sectionQueue,
                                Mutex queueMutex)
        {
            var readFileTask = Task.Factory.StartNew(() =>
            {                
                using (var streamReader = new BinaryReader(File.OpenRead(fileWriteSession.TempFile)))
                {
                    var section = new byte[0];
                    do
                    {
                        // Read section
                        section = streamReader.ReadBytes(sectionBytes);
                        var isMore = (streamReader.BaseStream.Position < streamReader.BaseStream.Length);

                        // Add to queue
                        queueMutex.WaitOne();
                        sectionQueue.Enqueue(new Tuple<byte[], bool>(section, isMore));
                        queueMutex.ReleaseMutex();

                        // Limit size of file in memory
                        while (isMore &&
                            sectionQueue.Count > 50)
                        {
                            Thread.Sleep(100);
                        }

                        Thread.Yield();
                    } while (streamReader.BaseStream.Position < streamReader.BaseStream.Length);
                }
            });

            return readFileTask;
        }

        /// <summary>
        /// Handles DeleteRequest message
        /// </summary>
        /// <param name="deleteRequest"></param>
        private void HandleDeleteRequest(DeleteRequest deleteRequest, MessageReceivedInfo messageReceivedInfo)
        {
            try
            {
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Processing request {deleteRequest.Id} to delete {deleteRequest.Path}");
                }

                var deleteResponse = new DeleteResponse()
                {
                    Id = Guid.NewGuid().ToString(),
                    TypeId = MessageTypeIds.DeleteResponse,
                    Response = new MessageResponse()
                    {
                        MessageId = deleteRequest.Id,
                        IsMore = false
                    }
                };

                // Check permissions
                var user = _userService.GetBySecurityKey(deleteRequest.SecurityKey);
                if (user == null ||
                    !user.Roles.Contains(UserRoles.FileSystemWrite))   // Invalid credentials
                {
                    deleteResponse.Response.ErrorMessage = "Permission denied";
                    deleteResponse.Response.ErrorCode = ResponseErrorCodes.PermissionDenied;
                }
                else     // Valid credentials
                {
                    try
                    {
                        // Check if file/folder exists
                        var fileObject = _fileSystemLocal.GetFile(deleteRequest.Path);
                        var folderObject = _fileSystemLocal.GetFolder(deleteRequest.Path, false, false);

                        if (fileObject != null)
                        {
                            //_fileSystemLocal.DeleteFile(deleteRequest.Path);
                        }
                        else if (folderObject != null)
                        {
                            //_fileSystemLocal.DeleteFolder(deleteRequest.Path);
                        }
                        else
                        {
                            deleteResponse.Response.ErrorCode = ResponseErrorCodes.Unknown;
                            deleteResponse.Response.ErrorMessage = "File or folder does not exist";
                        }            
                    }
                    catch (Exception exception)
                    {
                        deleteResponse.Response.ErrorMessage = exception.Message;
                        deleteResponse.Response.ErrorCode = ResponseErrorCodes.FileSystemError;
                    }
                }

                // Send response
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Sending response for deleting {deleteRequest.Path} request {deleteRequest.Id} to " +
                                $"{messageReceivedInfo.RemoteEndpointInfo.Ip}:{messageReceivedInfo.RemoteEndpointInfo.Port}");
                }
                _connection.SendMessage(_deleteResponseConverter.GetConnectionMessage(deleteResponse),
                                    messageReceivedInfo.RemoteEndpointInfo);

                if (OnStatusMessage != null)
                {
                    if (deleteResponse.Response.ErrorCode == null)
                    {
                        OnStatusMessage($"Processed request {deleteRequest.Id} to delete {deleteRequest.Path}");
                    }
                    else
                    {
                        OnStatusMessage($"Processed request {deleteRequest.Id} to delete {deleteRequest.Path}: {deleteResponse.Response.ErrorCode}");
                    }
                }
            }
            catch (Exception exception)
            {
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Error processing request {deleteRequest.Id} to delete {deleteRequest.Path}: {exception.Message}");
                }
            }
        }

        /// <summary>
        /// Handles MoveRequest message
        /// </summary>
        /// <param name="moveRequest"></param>
        private void HandleMoveRequest(MoveRequest moveRequest, MessageReceivedInfo messageReceivedInfo)
        {
            try
            {
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Processing request {moveRequest.Id} to move {moveRequest.OldPath}");
                }

                var moveResponse = new MoveResponse()
                {
                    Id = Guid.NewGuid().ToString(),
                    TypeId = MessageTypeIds.DeleteResponse,
                    Response = new MessageResponse()
                    {
                        MessageId = moveRequest.Id,
                        IsMore = false
                    }
                };

                // Check permissions
                var user = _userService.GetBySecurityKey(moveRequest.SecurityKey);
                if (user == null ||
                    !user.Roles.Contains(UserRoles.FileSystemWrite))   // Invalid credentials
                {
                    moveResponse.Response.ErrorMessage = "Permission denied";
                    moveResponse.Response.ErrorCode = ResponseErrorCodes.PermissionDenied;
                }
                else     // Valid credentials
                {
                    try
                    {
                        // Check if old/new file/folder exists
                        var oldFileObject = _fileSystemLocal.GetFile(moveRequest.OldPath);
                        var newFileObject = _fileSystemLocal.GetFile(moveRequest.NewPath);
                        var oldFolderObject = _fileSystemLocal.GetFolder(moveRequest.OldPath, false, false);
                        var newFolderObject = _fileSystemLocal.GetFolder(moveRequest.NewPath, false, false);

                        if (oldFileObject != null)    // Move file
                        {
                            if (newFileObject == null)
                            {
                                _fileSystemLocal.MoveFile(moveRequest.OldPath, moveRequest.NewPath);
                            }
                            else    // New file exists
                            {
                                moveResponse.Response.ErrorCode = ResponseErrorCodes.Unknown;
                                moveResponse.Response.ErrorMessage = "New file already exists";
                            }
                        }
                        else if (oldFolderObject != null)   // Move folder
                        {
                            if (newFolderObject == null)
                            {
                                _fileSystemLocal.MoveFolder(moveRequest.OldPath, moveRequest.NewPath);
                            }
                            else   // New folder exists
                            {
                                moveResponse.Response.ErrorCode = ResponseErrorCodes.Unknown;
                                moveResponse.Response.ErrorMessage = "New folder already exists";
                            }                        
                        }
                        else
                        {
                            moveResponse.Response.ErrorCode = ResponseErrorCodes.Unknown;
                            moveResponse.Response.ErrorMessage = "File or folder does not exist";
                        }
                    }
                    catch (Exception exception)
                    {
                        moveResponse.Response.ErrorMessage = exception.Message;
                        moveResponse.Response.ErrorCode = ResponseErrorCodes.FileSystemError;
                    }
                }

                // Send response
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Sending response for moving {moveRequest.OldPath} request {moveRequest.Id} to " +
                                $"{messageReceivedInfo.RemoteEndpointInfo.Ip}:{messageReceivedInfo.RemoteEndpointInfo.Port}");
                }
                _connection.SendMessage(_moveResponseConverter.GetConnectionMessage(moveResponse),
                                    messageReceivedInfo.RemoteEndpointInfo);

                if (OnStatusMessage != null)
                {
                    if (moveResponse.Response.ErrorCode == null)
                    {
                        OnStatusMessage($"Processed request {moveRequest.Id} to move {moveRequest.OldPath}");
                    }
                    else
                    {
                        OnStatusMessage($"Processed request {moveRequest.Id} to move {moveRequest.OldPath}: {moveResponse.Response.ErrorCode}");
                    }
                }
            }
            catch (Exception exception)
            {
                if (OnStatusMessage != null)
                {
                    OnStatusMessage($"Error processing request {moveRequest.Id} to move {moveRequest.OldPath}: {exception.Message}");
                }
            }
        }
    }
}

