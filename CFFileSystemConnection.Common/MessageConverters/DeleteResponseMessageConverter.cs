using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Models;
using CFFileSystemConnection.Utilities;

namespace CFFileSystemConnection.MessageConverters
{
    public class DeleteResponseMessageConverter : IExternalMessageConverter<DeleteResponse>
    {
        public ConnectionMessage GetConnectionMessage(DeleteResponse deleteResponse)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = deleteResponse.Id,
                TypeId = deleteResponse.TypeId,
                Parameters = new List<ConnectionMessageParameter>()
                {
                    new ConnectionMessageParameter()
                    {
                        Name = "Response",
                        Value = deleteResponse.Response == null ? "" :
                                    JsonUtilities.SerializeToBase64String(deleteResponse.Response,
                                            JsonUtilities.DefaultJsonSerializerOptions)
                    }                   
                }
            };
            return connectionMessage;
        }

        public DeleteResponse GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var deleteResponse = new DeleteResponse()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId                
            };

            // Get response
            var responseParameter = connectionMessage.Parameters.First(p => p.Name == "Response");
            if (!String.IsNullOrEmpty(responseParameter.Value))
            {
                deleteResponse.Response = JsonUtilities.DeserializeFromBase64String<MessageResponse>(responseParameter.Value, JsonUtilities.DefaultJsonSerializerOptions);
            }

            return deleteResponse;
        }
    }
}
