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
    /// <summary>
    /// Converts between ConnectionMessage and GetFolderRequest
    /// </summary>
    public class GetFolderRequestMessageConverter : IExternalMessageConverter<GetFolderRequest>
    {
        public ConnectionMessage GetConnectionMessage(GetFolderRequest getFolderRequest)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = getFolderRequest.Id,
                TypeId = getFolderRequest.TypeId,
                Parameters = new List<ConnectionMessageParameter>()
                {
                   new ConnectionMessageParameter()
                   {
                       Name = "Path",
                       Value = getFolderRequest.Path
                   },
                     new ConnectionMessageParameter()
                   {
                       Name = "GetFiles",
                       Value = getFolderRequest.GetFiles.ToString()
                   },
                   new ConnectionMessageParameter()
                   {
                       Name = "RecurseSubFolders",
                       Value = getFolderRequest.RecurseSubFolders.ToString()
                   }
                }
            };
            return connectionMessage;
        }

        public GetFolderRequest GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var getFolderRequest = new GetFolderRequest()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId,
                Path = connectionMessage.Parameters.First(p => p.Name == "Path").Value,
                GetFiles = Convert.ToBoolean(connectionMessage.Parameters.First(p => p.Name == "GetFiles").Value),
                RecurseSubFolders = Convert.ToBoolean(connectionMessage.Parameters.First(p => p.Name == "RecurseSubFolders").Value)
            };

            return getFolderRequest;
        }
    }
}
