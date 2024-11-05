using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Models;
using CFFileSystemConnection.Utilities;

namespace CFFileSystemConnection.MessageConverters
{
    public class WriteFileResponseMessageConverter : IExternalMessageConverter<WriteFileResponse>
    {
        public ConnectionMessage GetConnectionMessage(WriteFileResponse writeFileResponse)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = writeFileResponse.Id,
                TypeId = writeFileResponse.TypeId,
                Parameters = new List<ConnectionMessageParameter>()
                {
                    new ConnectionMessageParameter()
                    {
                        Name = "Response",
                        Value = writeFileResponse.Response == null ? "" :
                                    JsonUtilities.SerializeToBase64String(writeFileResponse.Response,
                                            JsonUtilities.DefaultJsonSerializerOptions)
                    },
                    new ConnectionMessageParameter()
                    {
                        Name = "SessionId",
                        Value = writeFileResponse.SessionId
                    }
                }
            };
            return connectionMessage;
        }

        public WriteFileResponse GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var writeFileResponse = new WriteFileResponse()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId,
                SessionId = connectionMessage.Parameters.First(p => p.Name == "SessionId").Value                
            };

            // Get response
            var responseParameter = connectionMessage.Parameters.First(p => p.Name == "Response");
            if (!String.IsNullOrEmpty(responseParameter.Value))
            {
                writeFileResponse.Response = JsonUtilities.DeserializeFromBase64String<MessageResponse>(responseParameter.Value, JsonUtilities.DefaultJsonSerializerOptions);
            }

            return writeFileResponse;
        }
    }
}
