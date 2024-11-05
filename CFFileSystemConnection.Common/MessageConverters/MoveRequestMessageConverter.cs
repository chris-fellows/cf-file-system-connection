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
    public class MoveRequestMessageConverter : IExternalMessageConverter<MoveRequest>
    {
        public ConnectionMessage GetConnectionMessage(MoveRequest moveRequest)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = moveRequest.Id,
                TypeId = moveRequest.TypeId,
                Parameters = new List<ConnectionMessageParameter>()
                {
                   new ConnectionMessageParameter()
                   {
                       Name = "SecurityKey",
                       Value = moveRequest.SecurityKey
                   },
                   new ConnectionMessageParameter()
                   {
                       Name = "OldPath",
                       Value = moveRequest.OldPath
                   },
                   new ConnectionMessageParameter()
                   {
                       Name = "NewPath",
                       Value = moveRequest.NewPath
                   }
                }
            };
            return connectionMessage;
        }

        public MoveRequest GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var moveRequest = new MoveRequest()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId,
                SecurityKey = connectionMessage.Parameters.First(p => p.Name == "SecurityKey").Value,
                OldPath = connectionMessage.Parameters.First(p => p.Name == "OldPath").Value,
                NewPath = connectionMessage.Parameters.First(p => p.Name == "NewPath").Value
            };

            return moveRequest;
        }
    }
}
