using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Models;
using CFFileSystemConnection.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.MessageConverters
{
    public class CreateFolderResponseMessageConverter : IExternalMessageConverter<CreateFolderResponse>
    {
        public ConnectionMessage GetConnectionMessage(CreateFolderResponse createFolderResponse)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = createFolderResponse.Id,
                TypeId = createFolderResponse.TypeId,
                Parameters = new List<ConnectionMessageParameter>()
                {
                    new ConnectionMessageParameter()
                    {
                        Name = "Response",
                        Value = createFolderResponse.Response == null ? "" :
                                    JsonUtilities.SerializeToBase64String(createFolderResponse.Response,
                                            JsonUtilities.DefaultJsonSerializerOptions)
                    },                    
                }
            };
            return connectionMessage;
        }

        public CreateFolderResponse GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var createFolderResponse = new CreateFolderResponse()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId                
            };

            // Get response
            var responseParameter = connectionMessage.Parameters.First(p => p.Name == "Response");
            if (!String.IsNullOrEmpty(responseParameter.Value))
            {
                createFolderResponse.Response = JsonUtilities.DeserializeFromBase64String<MessageResponse>(responseParameter.Value, JsonUtilities.DefaultJsonSerializerOptions);
            }

            return createFolderResponse;
        }
    }
}
