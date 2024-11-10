using CFConnectionMessaging;
using CFConnectionMessaging.Exceptions;
using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Constants;
using CFFileSystemConnection.Exceptions;
using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.MessageConverters;
using CFFileSystemConnection.Models;
using System.Diagnostics;

namespace CFFileSystemConnection.Common
{
    /// <summary>
    /// File system via remote UDP connection.
    /// </summary>
    public class FileSystemConnection : IFileSystem, IDisposable
    {
        private ConnectionTcp _connection;
        private TimeSpan _responseTimeout = TimeSpan.FromSeconds(30);
        private EndpointInfo? _remoteEndpointInfo;

        private readonly string _securityKey;

        // Message converters
        private readonly IExternalMessageConverter<CreateFolderRequest> _createFolderRequestConverter = new CreateFolderRequestMessageConverter();
        private readonly IExternalMessageConverter<CreateFolderResponse> _createFolderResponseConverter = new CreateFolderResponseMessageConverter();

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

        private readonly IExternalMessageConverter<WriteFileRequest> _writeFileRequestConverter = new WriteFileRequestMessageConverter();
        private readonly IExternalMessageConverter<WriteFileResponse> _writeFileResponseConverter = new WriteFileResponseMessageConverter();

        private readonly IExternalMessageConverter<MoveRequest> _moveRequestConverter = new MoveRequestMessageConverter();
        private readonly IExternalMessageConverter<MoveResponse> _moveResponseConverter = new MoveResponseMessageConverter();

        /// <summary>
        /// Stores responses messages until processed
        /// </summary>
        private List<MessageBase> _responseMessages = new List<MessageBase>();

        public FileSystemConnection(string securityKey)
        {
            _securityKey = securityKey;

            _connection = new ConnectionTcp();            
            _connection.OnConnectionMessageReceived += _connection_OnConnectionMessageReceived;
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                if (_connection.IsListening) _connection.StopListening();
                _connection.OnConnectionMessageReceived -= _connection_OnConnectionMessageReceived;
                _connection.Dispose();
            }
        }

        public EndpointInfo? RemoteEndpoint
        {
            get { return _remoteEndpointInfo; }
            set { _remoteEndpointInfo = value; }
        }

        public void StartListening(int port)
        {
            if (_connection.IsListening)
            {
                _connection.StopListening();
            }            
            _connection.ReceivePort = port;
            _connection.StartListening();
        }

        public void StopListening()
        {
            _connection.StopListening();
        }

        /// <summary>
        /// Handles message received from connection. Currently only expect to receive responses for requests
        /// </summary>
        /// <param name="connectionMessage"></param>
        /// <param name="messageReceivedInfo"></param>
        private void _connection_OnConnectionMessageReceived(ConnectionMessage connectionMessage, MessageReceivedInfo messageReceivedInfo)
        {
            switch(connectionMessage.TypeId)
            {
                case MessageTypeIds.DeleteResponse:
                    var deleteResponse = _deleteResponseConverter.GetExternalMessage(connectionMessage);
                    _responseMessages.Add(deleteResponse);
                    break;
                case MessageTypeIds.GetDrivesResponse:
                    var getDrivesResponse = _getDrivesResponseConverter.GetExternalMessage(connectionMessage);
                    _responseMessages.Add(getDrivesResponse);
                    break;
                case MessageTypeIds.GetFileContentResponse:
                    var getFileContentResponse = _getFileContentResponseConverter.GetExternalMessage(connectionMessage);
                    _responseMessages.Add(getFileContentResponse);
                    break;
                case MessageTypeIds.GetFileResponse:
                    var getFileResponse = _getFileResponseConverter.GetExternalMessage(connectionMessage);
                    _responseMessages.Add(getFileResponse);
                    break;
                case MessageTypeIds.GetFolderResponse:
                    var getFolderResponse = _getFolderResponseConverter.GetExternalMessage(connectionMessage);                    
                    _responseMessages.Add(getFolderResponse);
                    break;
                case MessageTypeIds.WriteFileResponse:
                    var writeFileResponse = _writeFileResponseConverter.GetExternalMessage(connectionMessage);
                    _responseMessages.Add(writeFileResponse);
                    break;
            }
        }

        public List<DriveObject> GetDrives()
        {
            // Send GetDrivesRequest message
            var getDrivesRequest = new GetDrivesRequest()
            {
                Id = Guid.NewGuid().ToString(),
                TypeId = MessageTypeIds.GetDrivesRequest,
                SecurityKey = _securityKey,
            };

            try
            {
                _connection.SendMessage(_getDrivesRequestConverter.GetConnectionMessage(getDrivesRequest), _remoteEndpointInfo);
            }
            catch(ConnectionException connectionException)
            {
                throw new FileSystemConnectionException(connectionException.Message, connectionException);
            }

            // Wait for response. Should only receive one response
            var responseMessages = new List<MessageBase>();
            var isGotAllMessages = WaitForResponses(getDrivesRequest, _responseTimeout, _responseMessages,
                                        (responseMessage) =>
                                        {
                                            responseMessages.Add(responseMessage);
                                        });
            
            // Process response
            if (responseMessages.Any())
            {
                var getDrivesResponse = (GetDrivesResponse)responseMessages.First();    // Only one response expected
                if (getDrivesResponse.Response.ErrorCode != null)
                {
                    throw new FileSystemConnectionException($"Error getting drives: {getDrivesResponse.Response.ErrorMessage}")
                    { ResponseErrorCode = getDrivesResponse.Response.ErrorCode };
                }
                return getDrivesResponse.Drives;
            }
            else
            {
                throw new FileSystemConnectionException($"Error getting drives");
            }

            return null;
        }

        public FolderObject? GetFolder(string path, bool getFiles, bool recurseSubFolders)
        {
            // Send GetFolderRequest message
            var getFolderRequest = new GetFolderRequest()
            {
                Id = Guid.NewGuid().ToString(),
                TypeId = MessageTypeIds.GetFolderRequest,
                SecurityKey = _securityKey,
                Path = path,
                GetFiles = getFiles,
                RecurseSubFolders = recurseSubFolders
            };

            try
            {

                _connection.SendMessage(_getFolderRequestConverter.GetConnectionMessage(getFolderRequest), _remoteEndpointInfo);
            }
            catch (ConnectionException connectionException)
            {
                throw new FileSystemConnectionException(connectionException.Message, connectionException);
            }

            // Wait for response. Should only receive one response
            var responseMessages = new List<MessageBase>();
            var isGotAllMessages = WaitForResponses(getFolderRequest, _responseTimeout, _responseMessages,
                                        (responseMessage) =>
                                        {
                                            responseMessages.Add(responseMessage);
                                        });

            // Process response
            if (responseMessages.Any())
            {
                var getFolderResponse = (GetFolderResponse)responseMessages.First();    // Only one response expected
                if (getFolderResponse.Response.ErrorCode != null)
                {
                    throw new FileSystemConnectionException($"Error getting folder: {getFolderResponse.Response.ErrorMessage}")
                                { ResponseErrorCode = getFolderResponse.Response.ErrorCode };
                }                
                return getFolderResponse.FolderObject;
            }
            else
            {
                throw new FileSystemConnectionException($"Error getting folder");
            }

            return null;
        }

        public FileObject? GetFile(string path)
        {
            // Send GetFileRequest message
            var getFileRequest = new GetFileRequest()
            {
                Id = Guid.NewGuid().ToString(),
                TypeId = MessageTypeIds.GetFileRequest,
                SecurityKey = _securityKey,
                Path = path
            };

            try
            {
                _connection.SendMessage(_getFileRequestConverter.GetConnectionMessage(getFileRequest), _remoteEndpointInfo);
            }
            catch (ConnectionException connectionException)
            {
                throw new FileSystemConnectionException(connectionException.Message, connectionException);
            }

            // Wait for response. Should only receive one response
            var responseMessages = new List<MessageBase>();
            var isGotAllMessages = WaitForResponses(getFileRequest, _responseTimeout, _responseMessages,
                                        (responseMessage) =>
                                        {
                                            responseMessages.Add(responseMessage);
                                        });

            // Process response
            if (responseMessages.Any())
            {
                var getFileResponse = (GetFileResponse)responseMessages.First();
                if (getFileResponse.Response.ErrorCode != null)
                {
                    throw new FileSystemConnectionException($"Error getting file: {getFileResponse.Response.ErrorMessage}")
                    { ResponseErrorCode = getFileResponse.Response.ErrorCode };
                }
                return getFileResponse.FileObject;
            }
            else
            {
                throw new FileSystemConnectionException($"Error getting file");
            }

            return null;
        }

        //public byte[]? GetFileContent(string path)
        //{
        //    // Send GetFileContentRequest message
        //    var getFileContentRequest = new GetFileContentRequest()
        //    {
        //        Id = Guid.NewGuid().ToString(),
        //        TypeId = MessageTypeIds.GetFileContentRequest,
        //        SecurityKey = _securityKey,
        //        Path = path
        //    };
        //    _connection.SendMessage(_getFileContentRequestConverter.GetConnectionMessage(getFileContentRequest), _remoteEndpointInfo);

        //    // Wait for response. Should only receive one response
        //    var response = WaitForResponses(getFileContentRequest, _responseTimeout, _responseMessages).FirstOrDefault();

        //    // Process response
        //    if (response != null)
        //    {
        //        var getFileContentResponse = (GetFileContentResponse)response;
        //        if (getFileContentResponse.Response.ErrorCode != null)
        //        {
        //            throw new FileSystemConnectionException($"Error getting file: {getFileContentResponse.Response.ErrorMessage}")
        //            { ResponseErrorCode = getFileContentResponse.Response.ErrorCode };
        //        }
        //        return getFileContentResponse.Content;
        //    }

        //    return null;
        //}

        public void GetFileContentBySection(string path, int sectionBytes, Action<byte[], bool> actionSection)
        {            
            // Send GetFileContentRequest message
            var getFileContentRequest = new GetFileContentRequest()
            {
                Id = Guid.NewGuid().ToString(),
                TypeId = MessageTypeIds.GetFileContentRequest,
                SecurityKey = _securityKey,
                Path = path,
                SectionBytes = sectionBytes
            };

            try
            {
                _connection.SendMessage(_getFileContentRequestConverter.GetConnectionMessage(getFileContentRequest), _remoteEndpointInfo);
            }
            catch (ConnectionException connectionException)
            {
                throw new FileSystemConnectionException(connectionException.Message, connectionException);
            }

            // Wait for all responses, pass sections to caller
            MessageResponse? firstErrorMessageResponse = null;
            var isGotAllResponses = WaitForResponses(getFileContentRequest, _responseTimeout, _responseMessages,
                                (responseMessage) =>
                                {
                                    var getFileContentResponse = (GetFileContentResponse)responseMessage;
                                    if (getFileContentResponse.Response.ErrorCode == null)
                                    {
                                        actionSection(getFileContentResponse.Content, getFileContentResponse.Response.IsMore);
                                    }
                                    else if (firstErrorMessageResponse == null)    // Store first error
                                    {
                                        firstErrorMessageResponse = getFileContentResponse.Response;
                                    }
                                });

            // Process response
            if (firstErrorMessageResponse != null)
            {                
                throw new FileSystemConnectionException($"Error getting file: {firstErrorMessageResponse.ErrorMessage}")
                                { ResponseErrorCode = firstErrorMessageResponse.ErrorCode };                                
            }            
            else if (!isGotAllResponses)    // Timeout
            {
                throw new FileSystemConnectionException($"Timed out getting file");
            }            
        }

        public void WriteFileContentBySection(FileObject fileObject, Func<Tuple<byte[], bool>> getSectionFunction)
        {
            // Send file sections until all sent
            bool isMore = true;
            var sessionId = Guid.NewGuid().ToString();  // All requests share same SessionId
            while (isMore)
            {                
                var sectionData = getSectionFunction();

                // Send WriteFileRequest message
                var writeFileRequest = new WriteFileRequest()
                {
                    Id = Guid.NewGuid().ToString(),
                    TypeId = MessageTypeIds.WriteFileRequest,
                    SecurityKey = _securityKey,
                    SessionId = sessionId, 
                    FileObject = fileObject,
                    Content = sectionData.Item1,
                    IsMore = sectionData.Item2
                };

                try
                {
                    _connection.SendMessage(_writeFileRequestConverter.GetConnectionMessage(writeFileRequest), _remoteEndpointInfo);
                }
                catch (ConnectionException connectionException)
                {
                    throw new FileSystemConnectionException(connectionException.Message, connectionException);
                }

                // Wait for response. Should only receive one response
                var responseMessages = new List<MessageBase>();
                var isGotAllMessages = WaitForResponses(writeFileRequest, _responseTimeout, _responseMessages,
                                            (responseMessage) =>
                                            {
                                                responseMessages.Add(responseMessage);
                                            });

                // Process response
                if (responseMessages.Any())
                {
                    var writeFileResponse = (WriteFileResponse)responseMessages.First();
                    if (writeFileResponse.Response.ErrorCode != null)
                    {
                        throw new FileSystemConnectionException($"Error writing file: {writeFileResponse.Response.ErrorMessage}")
                        { ResponseErrorCode = writeFileResponse.Response.ErrorCode };
                    }                    
                }
                else
                {
                    throw new FileSystemConnectionException($"Error writing file");
                }

                isMore = sectionData.Item2;
            }
        }

        /// <summary>
        /// Waits for all responses for request until completed or timeout. Where multiple responses are required then
        /// MessageBase.Response.IsMore=true for all except the last one.
        /// </summary>
        /// <param name="request">Request to check</param>
        /// <param name="timeout">Timeout receiving responses</param>
        /// <param name="responseMessagesToCheck">List where responses are added</param>
        /// <param name="responseMessageAction">Action to forward next response</param>
        /// <returns>Whether all responses received</returns>
        private bool WaitForResponses(MessageBase request, TimeSpan timeout,
                                      List<MessageBase> responseMessagesToCheck,
                                      Action<MessageBase> responseMessageAction)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var isGotAllResponses = false;            
            while (!isGotAllResponses &&
                    stopwatch.Elapsed < timeout)
            {
                // Check for next response message
                var responseMessage = responseMessagesToCheck.FirstOrDefault(m => m.Response != null && m.Response.MessageId == request.Id);

                if (responseMessage != null)
                {
                    // Discard
                    responseMessagesToCheck.Remove(responseMessage);

                    // Check if last response
                    isGotAllResponses = !responseMessage.Response.IsMore;

                    // Pass response to caller
                    responseMessageAction(responseMessage);                                       
                }

                Thread.Sleep(20);
            }

            return isGotAllResponses; 
        }
    
        public void DeleteFile(string path)
        {
            // Send DeleteRequest message
            var deleteRequest = new DeleteRequest()
            {
                Id = Guid.NewGuid().ToString(),
                TypeId = MessageTypeIds.GetFileRequest,
                SecurityKey = _securityKey,
                Path = path
            };

            try
            {
                _connection.SendMessage(_deleteRequestConverter.GetConnectionMessage(deleteRequest), _remoteEndpointInfo);
            }
            catch (ConnectionException connectionException)
            {
                throw new FileSystemConnectionException(connectionException.Message, connectionException);
            }

            // Wait for response. Should only receive one response
            var responseMessages = new List<MessageBase>();
            var isGotAllMessages = WaitForResponses(deleteRequest, _responseTimeout, _responseMessages,
                                        (responseMessage) =>
                                        {
                                            responseMessages.Add(responseMessage);
                                        });

            // Process response
            if (responseMessages.Any())
            {
                var deleteResponse = (DeleteResponse)responseMessages.First();
                if (deleteResponse.Response.ErrorCode != null)
                {
                    throw new FileSystemConnectionException($"Error deleting file: {deleteResponse.Response.ErrorMessage}")
                    { ResponseErrorCode = deleteResponse.Response.ErrorCode };
                }                
            }
            else
            {
                throw new FileSystemConnectionException($"Error deleting file");
            }            
        }

        public void DeleteFolder(string path)
        {
            // Send DeleteRequest message
            var deleteRequest = new DeleteRequest()
            {
                Id = Guid.NewGuid().ToString(),
                TypeId = MessageTypeIds.GetFileRequest,
                SecurityKey = _securityKey,
                Path = path
            };

            try
            {
                _connection.SendMessage(_deleteRequestConverter.GetConnectionMessage(deleteRequest), _remoteEndpointInfo);
            }
            catch (ConnectionException connectionException)
            {
                throw new FileSystemConnectionException(connectionException.Message, connectionException);
            }

            // Wait for response. Should only receive one response
            var responseMessages = new List<MessageBase>();
            var isGotAllMessages = WaitForResponses(deleteRequest, _responseTimeout, _responseMessages,
                                        (responseMessage) =>
                                        {
                                            responseMessages.Add(responseMessage);
                                        });

            // Process response
            if (responseMessages.Any())
            {
                var deleteResponse = (DeleteResponse)responseMessages.First();
                if (deleteResponse.Response.ErrorCode != null)
                {
                    throw new FileSystemConnectionException($"Error deleting folder: {deleteResponse.Response.ErrorMessage}")
                    { ResponseErrorCode = deleteResponse.Response.ErrorCode };
                }
            }
            else
            {
                throw new FileSystemConnectionException($"Error deleting folder");
            }
        }

        public void MoveFile(string oldPath, string newPath)
        {
            // Send DeleteRequest message
            var moveRequest = new MoveRequest()
            {
                Id = Guid.NewGuid().ToString(),
                TypeId = MessageTypeIds.MoveRequest,
                SecurityKey = _securityKey,
                OldPath = oldPath,
                NewPath = newPath
            };

            try
            {
                _connection.SendMessage(_moveRequestConverter.GetConnectionMessage(moveRequest), _remoteEndpointInfo);
            }
            catch (ConnectionException connectionException)
            {
                throw new FileSystemConnectionException(connectionException.Message, connectionException);
            }

            // Wait for response. Should only receive one response
            var responseMessages = new List<MessageBase>();
            var isGotAllMessages = WaitForResponses(moveRequest, _responseTimeout, _responseMessages,
                                        (responseMessage) =>
                                        {
                                            responseMessages.Add(responseMessage);
                                        });

            // Process response
            if (responseMessages.Any())
            {
                var moveResponse = (MoveResponse)responseMessages.First();
                if (moveResponse.Response.ErrorCode != null)
                {
                    throw new FileSystemConnectionException($"Error moving file: {moveResponse.Response.ErrorMessage}")
                    { ResponseErrorCode = moveResponse.Response.ErrorCode };
                }
            }
            else
            {
                throw new FileSystemConnectionException($"Error moving file");
            }
        }

        public void MoveFolder(string oldPath, string newPath)
        {
            // Send DeleteRequest message
            var moveRequest = new MoveRequest()
            {
                Id = Guid.NewGuid().ToString(),
                TypeId = MessageTypeIds.MoveRequest,
                SecurityKey = _securityKey,
                OldPath = oldPath,
                NewPath = newPath
            };

            try
            {
                _connection.SendMessage(_moveRequestConverter.GetConnectionMessage(moveRequest), _remoteEndpointInfo);
            }
            catch (ConnectionException connectionException)
            {
                throw new FileSystemConnectionException(connectionException.Message, connectionException);
            }

            // Wait for response. Should only receive one response
            var responseMessages = new List<MessageBase>();
            var isGotAllMessages = WaitForResponses(moveRequest, _responseTimeout, _responseMessages,
                                        (responseMessage) =>
                                        {
                                            responseMessages.Add(responseMessage);
                                        });

            // Process response
            if (responseMessages.Any())
            {
                var moveResponse = (MoveResponse)responseMessages.First();
                if (moveResponse.Response.ErrorCode != null)
                {
                    throw new FileSystemConnectionException($"Error moving folder: {moveResponse.Response.ErrorMessage}")
                    { ResponseErrorCode = moveResponse.Response.ErrorCode };
                }
            }
            else
            {
                throw new FileSystemConnectionException($"Error moving folder");
            }
        }

        public void CreateFolder(string path)
        {
            // Send DeleteRequest message
            var createFolderRequest = new CreateFolderRequest()
            {
                Id = Guid.NewGuid().ToString(),
                TypeId = MessageTypeIds.CreateFolderRequest,
                SecurityKey = _securityKey,
                Path = path
            };

            try
            {
                _connection.SendMessage(_createFolderRequestConverter.GetConnectionMessage(createFolderRequest), _remoteEndpointInfo);
            }
            catch (ConnectionException connectionException)
            {
                throw new FileSystemConnectionException(connectionException.Message, connectionException);
            }

            // Wait for response. Should only receive one response
            var responseMessages = new List<MessageBase>();
            var isGotAllMessages = WaitForResponses(createFolderRequest, _responseTimeout, _responseMessages,
                                        (responseMessage) =>
                                        {
                                            responseMessages.Add(responseMessage);
                                        });

            // Process response
            if (responseMessages.Any())
            {
                var createFolderResponse = (CreateFolderResponse)responseMessages.First();
                if (createFolderResponse.Response.ErrorCode != null)
                {
                    throw new FileSystemConnectionException($"Error creating folder: {createFolderResponse.Response.ErrorMessage}")
                    { ResponseErrorCode = createFolderResponse.Response.ErrorCode };
                }
            }
            else
            {
                throw new FileSystemConnectionException($"Error creating folder");
            }
        }
    }
}
