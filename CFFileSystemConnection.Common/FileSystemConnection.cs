using CFConnectionMessaging;
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
        private readonly IExternalMessageConverter<GetFolderRequest> _getFolderRequestConverter = new GetFolderRequestMessageConverter();
        private readonly IExternalMessageConverter<GetFolderResponse> _getFolderResponseConverter = new GetFolderResponseMessageConverter();

        private readonly IExternalMessageConverter<GetFileContentRequest> _getFileContentRequestConverter = new GetFileContentRequestMessageConverter();
        private readonly IExternalMessageConverter<GetFileContentResponse> _getFileContentResponseConverter = new GetFileContentResponseMessageConverter();

        private readonly IExternalMessageConverter<GetFileRequest> _getFileRequestConverter = new GetFileRequestMessageConverter();
        private readonly IExternalMessageConverter<GetFileResponse> _getFileResponseConverter = new GetFileResponseMessageConverter();

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

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.StopListening();
                _connection.OnConnectionMessageReceived -= _connection_OnConnectionMessageReceived;
                _connection = null;
            }
        }

        public EndpointInfo? RemoteEndpoint
        {
            get { return _remoteEndpointInfo; }
            set { _remoteEndpointInfo = value; }
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

        /// <summary>
        /// Handles message received from connection. Currently only expect to receive responses for requests
        /// </summary>
        /// <param name="connectionMessage"></param>
        /// <param name="messageReceivedInfo"></param>
        private void _connection_OnConnectionMessageReceived(ConnectionMessage connectionMessage, MessageReceivedInfo messageReceivedInfo)
        {
            switch(connectionMessage.TypeId)
            {
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
            }
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
            _connection.SendMessage(_getFolderRequestConverter.GetConnectionMessage(getFolderRequest), _remoteEndpointInfo);

            // Wait for response. Should only receive one response
            var response = WaitForResponses(getFolderRequest, _responseTimeout, _responseMessages).FirstOrDefault();

            // Process response
            if (response != null)
            {
                var getFolderResponse = (GetFolderResponse)response;
                if (getFolderResponse.Response.ErrorCode != null)
                {
                    throw new FileSystemConnectionException($"Error getting folder: {getFolderResponse.Response.ErrorMessage}")
                                { ResponseErrorCode = getFolderResponse.Response.ErrorCode };
                }                
                return getFolderResponse.FolderObject;
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
            _connection.SendMessage(_getFileRequestConverter.GetConnectionMessage(getFileRequest), _remoteEndpointInfo);

            // Wait for response. Should only receive one response
            var response = WaitForResponses(getFileRequest, _responseTimeout, _responseMessages).FirstOrDefault();

            // Process response
            if (response != null)
            {
                var getFileResponse = (GetFileResponse)response;
                if (getFileResponse.Response.ErrorCode != null)
                {
                    throw new FileSystemConnectionException($"Error getting file: {getFileResponse.Response.ErrorMessage}")
                    { ResponseErrorCode = getFileResponse.Response.ErrorCode };
                }
                return getFileResponse.FileObject;
            }

            return null;
        }

        public byte[]? GetFileContent(string path)
        {
            // Send GetFileContentRequest message
            var getFileContentRequest = new GetFileContentRequest()
            {
                Id = Guid.NewGuid().ToString(),
                TypeId = MessageTypeIds.GetFileContentRequest,
                SecurityKey = _securityKey,
                Path = path
            };
            _connection.SendMessage(_getFileContentRequestConverter.GetConnectionMessage(getFileContentRequest), _remoteEndpointInfo);

            // Wait for response. Should only receive one response
            var response = WaitForResponses(getFileContentRequest, _responseTimeout, _responseMessages).FirstOrDefault();

            // Process response
            if (response != null)
            {
                var getFileContentResponse = (GetFileContentResponse)response;
                if (getFileContentResponse.Response.ErrorCode != null)
                {
                    throw new FileSystemConnectionException($"Error getting file: {getFileContentResponse.Response.ErrorMessage}")
                    { ResponseErrorCode = getFileContentResponse.Response.ErrorCode };
                }
                return getFileContentResponse.Content;
            }

            return null;
        }

        /// <summary>
        /// Waits for all responses for request until completed or until timeout
        /// </summary>
        /// <param name="request"></param>
        /// <param name="timeout"></param>
        /// <returns>List of responses if all responses received; null: If failed to get all responses </returns>
        private List<MessageBase>? WaitForResponses(MessageBase request, TimeSpan timeout,
                                             List<MessageBase> responseMessagesToCheck)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();            

            var isGotResponses = false;
            var responses = new List<MessageBase>();
            while (!isGotResponses &&
                stopwatch.Elapsed < timeout)
            {
                // Check for response message
                var responseMessage = responseMessagesToCheck.FirstOrDefault(m => m.Response != null && m.Response.MessageId == request.Id);
            
                if (responseMessage != null)                    
                {
                    // Discard
                    responseMessagesToCheck.Remove(responseMessage);

                    // Add to list to return
                    responses.Add(responseMessage);
                    isGotResponses = !responseMessage.Response.IsMore;
                }

                Thread.Sleep(50);
            }

            return isGotResponses ? responses : null;
        }
    }
}
