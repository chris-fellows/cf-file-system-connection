using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Constants;
using CFFileSystemConnection.Models;

namespace CFFileSystemConnection.MessageConverters
{
    public class GetDrivesRequestMessageConverter : IExternalMessageConverter<GetDrivesRequest>
    {
        public ConnectionMessage GetConnectionMessage(GetDrivesRequest getDrivesRequest)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = getDrivesRequest.Id,
                TypeId = MessageTypeIds.GetDrivesRequest,
                Parameters = new List<ConnectionMessageParameter>()
                {
                   new ConnectionMessageParameter()
                   {
                       Name = "SecurityKey",
                       Value = getDrivesRequest.SecurityKey
                   }                  
                }
            };
            return connectionMessage;
        }

        public GetDrivesRequest GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var getDrivesRequest = new GetDrivesRequest()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId,
                SecurityKey = connectionMessage.Parameters.First(p => p.Name == "SecurityKey").Value,                
            };

            return getDrivesRequest;
        }
    }
}
