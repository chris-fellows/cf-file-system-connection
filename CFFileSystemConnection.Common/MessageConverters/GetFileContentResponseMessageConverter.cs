using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Models;
using CFFileSystemConnection.Utilities;

namespace CFFileSystemConnection.MessageConverters
{
    public class GetFileContentResponseMessageConverter : IExternalMessageConverter<GetFileContentResponse>
    {
        public ConnectionMessage GetConnectionMessage(GetFileContentResponse getFileContentResponse)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = getFileContentResponse.Id,
                TypeId = getFileContentResponse.TypeId,
                Parameters = new List<ConnectionMessageParameter>()
                {
                    new ConnectionMessageParameter()
                    {
                        Name = "Response",
                        Value = getFileContentResponse.Response == null ? "" :
                                    JsonUtilities.SerializeToBase64String(getFileContentResponse.Response,
                                            JsonUtilities.DefaultJsonSerializerOptions)
                    },
                    new ConnectionMessageParameter()
                   {
                       Name = "Content",                      
                       Value = getFileContentResponse.Content == null ? "" :
                                        Convert.ToBase64String(getFileContentResponse.Content)                       
                   }
                }
            };
            return connectionMessage;
        }

        public GetFileContentResponse GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var getFileContentResponse = new GetFileContentResponse()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId
            };

            // Get response
            var responseParameter = connectionMessage.Parameters.First(p => p.Name == "Response");
            if (!String.IsNullOrEmpty(responseParameter.Value))
            {
                getFileContentResponse.Response = JsonUtilities.DeserializeFromBase64String<MessageResponse>(responseParameter.Value, JsonUtilities.DefaultJsonSerializerOptions);
            }

            // Get content object
            var contentParameter = connectionMessage.Parameters.First(p => p.Name == "Content");
            if (!String.IsNullOrEmpty(contentParameter.Value))
            {
                getFileContentResponse.Content = Convert.FromBase64String(contentParameter.Value);
            }

            return getFileContentResponse;
        }
    }
}
