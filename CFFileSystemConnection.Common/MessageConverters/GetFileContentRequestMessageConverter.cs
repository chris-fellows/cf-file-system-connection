using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Constants;
using CFFileSystemConnection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.MessageConverters
{
    public class GetFileContentRequestMessageConverter : IExternalMessageConverter<GetFileContentRequest>
    {
        public ConnectionMessage GetConnectionMessage(GetFileContentRequest getFileContentRequest)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = getFileContentRequest.Id,
                TypeId = MessageTypeIds.GetFileContentRequest,
                Parameters = new List<ConnectionMessageParameter>()
                {
                   new ConnectionMessageParameter()
                   {
                       Name = "SecurityKey",
                       Value = getFileContentRequest.SecurityKey
                   },
                   new ConnectionMessageParameter()
                   {
                       Name = "Path",
                       Value = getFileContentRequest.Path
                   },
                }
            };
            return connectionMessage;
        }

        public GetFileContentRequest GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var getFileContentRequest = new GetFileContentRequest()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId,
                SecurityKey = connectionMessage.Parameters.First(p => p.Name == "SecurityKey").Value,
                Path = connectionMessage.Parameters.First(p => p.Name == "Path").Value
            };

            return getFileContentRequest;
        }
    }
}
