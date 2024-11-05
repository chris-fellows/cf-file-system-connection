using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.MessageConverters
{
    public class DeleteRequestMessageConverter : IExternalMessageConverter<DeleteRequest>
    {
        public ConnectionMessage GetConnectionMessage(DeleteRequest deleteRequest)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = deleteRequest.Id,
                TypeId = deleteRequest.TypeId,
                Parameters = new List<ConnectionMessageParameter>()
                {
                   new ConnectionMessageParameter()
                   {
                       Name = "SecurityKey",
                       Value = deleteRequest.SecurityKey
                   },
                   new ConnectionMessageParameter()
                   {
                       Name = "Path",
                       Value = deleteRequest.Path
                   }
                }
            };
            return connectionMessage;
        }

        public DeleteRequest GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var deleteRequest = new DeleteRequest()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId,
                SecurityKey = connectionMessage.Parameters.First(p => p.Name == "SecurityKey").Value,
                Path = connectionMessage.Parameters.First(p => p.Name == "Path").Value
            };

            return deleteRequest;
        }
    }
}
