using CFConnectionMessaging;
using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Constants;
using CFFileSystemConnection.Enums;
using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.MessageConverters;
using CFFileSystemConnection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.Common
{
    /// <summary>
    /// Handles request messages for file system messages.
    /// 
    /// This class should be used by the client that is providing access to it's file system. The remote client
    /// will instantiate FileSystemConnection (IFileSystem) which sends messages to this class.
    /// </summary>
    public class FileSystemRequestHandler
    {
        private ConnectionUdp _connection;

        public delegate void StatusMessage(string message);
        public event StatusMessage? OnStatusMessage;

        /// <summary>
        /// Interface for accesing the local file system.
        /// </summary>
        private readonly IFileSystem _fileSystemLocal;

        // Message converters
        private readonly IExternalMessageConverter<GetFolderRequest> _getFolderRequestConverter = new GetFolderRequestMessageConverter();
        private readonly IExternalMessageConverter<GetFolderResponse> _getFolderResponseConverter = new GetFolderResponseMessageConverter();
        private readonly IExternalMessageConverter<GetFileRequest> _getFileRequestConverter = new GetFileRequestMessageConverter();
        private readonly IExternalMessageConverter<GetFileResponse> _getFileResponseConverter = new GetFileResponseMessageConverter();

        public FileSystemRequestHandler(IFileSystem fileSystemLocal)
        {
            _fileSystemLocal = fileSystemLocal;

            _connection = new ConnectionUdp();
            _connection.OnConnectionMessageReceived += _connection_OnConnectionMessageReceived;            
        }

        private void _connection_OnConnectionMessageReceived(ConnectionMessage connectionMessage, MessageReceivedInfo messageReceivedInfo)
        {
            if (OnStatusMessage != null)
            {
                OnStatusMessage($"Received message {connectionMessage.Id} {connectionMessage.TypeId} from " +
                        $"{messageReceivedInfo.RemoteEndpointInfo.Ip}:{messageReceivedInfo.RemoteEndpointInfo.Port}");
            }

            switch(connectionMessage.TypeId)
            {
                case MessageTypeIds.GetFileResponse:
                    // Get GetFileRequest
                    var getFileRequest = _getFileRequestConverter.GetExternalMessage(connectionMessage);
                    HandleGetFileRequest(getFileRequest, messageReceivedInfo);
                    break;
                case MessageTypeIds.GetFolderRequest:
                    // Get GetFolderRequest
                    var getFolderRequest = _getFolderRequestConverter.GetExternalMessage(connectionMessage);                    
                    HandleGetFolderRequest(getFolderRequest, messageReceivedInfo);
                    break;                
            }
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
        /// Handles GetFolderRequest message
        /// </summary>
        /// <param name="getFolderRequest"></param>
        private void HandleGetFolderRequest(GetFolderRequest getFolderRequest, MessageReceivedInfo messageReceivedInfo)
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
            catch(Exception exception)
            {
                getFolderResponse.Response.ErrorMessage = exception.Message;
                getFolderResponse.Response.ErrorCode = ResponseErrorCodes.FileSystemError;
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
                OnStatusMessage($"Processed request {getFolderRequest.Id} to get folder {getFolderRequest.Path}");
            }
        }

        /// <summary>
        /// Handles GetFileRequest message
        /// </summary>
        /// <param name="getFolderRequest"></param>
        private void HandleGetFileRequest(GetFileRequest getFileRequest, MessageReceivedInfo messageReceivedInfo)
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
                OnStatusMessage($"Processed request {getFileRequest.Id} to get file {getFileRequest.Path}");
            }
        }
    }
}

