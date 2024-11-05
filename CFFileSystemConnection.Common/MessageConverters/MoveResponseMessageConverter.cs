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
    public class MoveResponseMessageConverter : IExternalMessageConverter<MoveResponse>
    {
        public ConnectionMessage GetConnectionMessage(MoveResponse moveResponse)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = moveResponse.Id,
                TypeId = moveResponse.TypeId,
                Parameters = new List<ConnectionMessageParameter>()
                {
                    new ConnectionMessageParameter()
                    {
                        Name = "Response",
                        Value = moveResponse.Response == null ? "" :
                                    JsonUtilities.SerializeToBase64String(moveResponse.Response,
                                            JsonUtilities.DefaultJsonSerializerOptions)
                    }
                }
            };
            return connectionMessage;
        }

        public MoveResponse GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var moveResponse = new MoveResponse()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId
            };

            // Get response
            var responseParameter = connectionMessage.Parameters.First(p => p.Name == "Response");
            if (!String.IsNullOrEmpty(responseParameter.Value))
            {
                moveResponse.Response = JsonUtilities.DeserializeFromBase64String<MessageResponse>(responseParameter.Value, JsonUtilities.DefaultJsonSerializerOptions);
            }

            return moveResponse;
        }
    }
}
