using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Models;
using CFFileSystemConnection.Utilities;

namespace CFFileSystemConnection.MessageConverters
{
    /// <summary>
    /// Converts between ConnectionMessage and GetFolderRequest
    /// </summary>
    public class GetFolderResponseMessageConverter : IExternalMessageConverter<GetFolderResponse>
    {
        public ConnectionMessage GetConnectionMessage(GetFolderResponse getFolderResponse)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = getFolderResponse.Id,
                TypeId = getFolderResponse.TypeId,
                Parameters = new List<ConnectionMessageParameter>()
                {
                    new ConnectionMessageParameter()
                    {
                        Name = "Response",                        
                        Value = getFolderResponse.Response == null ? "" :
                                    JsonUtilities.SerializeToBase64String(getFolderResponse.Response,
                                            JsonUtilities.DefaultJsonSerializerOptions)
                    },
                   new ConnectionMessageParameter()
                   {
                       Name = "Folder",                                             
                       Value = getFolderResponse.FolderObject == null ? "" :
                                    JsonUtilities.SerializeToBase64String(getFolderResponse.FolderObject,
                                            JsonUtilities.DefaultJsonSerializerOptions)
                   }
                }
            };
            return connectionMessage;
        }

        public GetFolderResponse GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var getFolderResponse = new GetFolderResponse()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId,
            };

            // Get response
            var responseParameter = connectionMessage.Parameters.First(p => p.Name == "Response");
            if (!String.IsNullOrEmpty(responseParameter.Value))
            {
                getFolderResponse.Response = JsonUtilities.DeserializeFromBase64String<MessageResponse>(responseParameter.Value, JsonUtilities.DefaultJsonSerializerOptions);                
            }

            // Get folder object
            var folderParameter = connectionMessage.Parameters.First(p => p.Name == "Folder");
            if (!String.IsNullOrEmpty(folderParameter.Value))
            {
                getFolderResponse.FolderObject = JsonUtilities.DeserializeFromBase64String<FolderObject>(folderParameter.Value, JsonUtilities.DefaultJsonSerializerOptions);                
            }

            return getFolderResponse;
        }
    }
}
