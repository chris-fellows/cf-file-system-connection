using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Models;

namespace CFFileSystemConnection.MessageConverters
{
    /// <summary>
    /// Converts between ConnectionMessage and GetFileRequest
    /// </summary>
    public class GetFileRequestMessageConverter : IExternalMessageConverter<GetFileRequest>
    {
        public ConnectionMessage GetConnectionMessage(GetFileRequest getFileRequest)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = getFileRequest.Id,
                TypeId = getFileRequest.TypeId,
                Parameters = new List<ConnectionMessageParameter>()
                {
                   new ConnectionMessageParameter()
                   {
                       Name = "Path",
                       Value = getFileRequest.Path
                   },
                }
            };
            return connectionMessage;
        }

        public GetFileRequest GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var getFileRequest = new GetFileRequest()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId,                
                Path = connectionMessage.Parameters.First(p => p.Name == "Path").Value
            };

            return getFileRequest;
        }
    }
}
