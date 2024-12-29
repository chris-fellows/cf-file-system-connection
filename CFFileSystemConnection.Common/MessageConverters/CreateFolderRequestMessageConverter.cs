using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Models;

namespace CFFileSystemConnection.MessageConverters
{
    public class CreateFolderRequestMessageConverter : IExternalMessageConverter<CreateFolderRequest>
    {
        public ConnectionMessage GetConnectionMessage(CreateFolderRequest createFolderRequest)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = createFolderRequest.Id,
                TypeId = createFolderRequest.TypeId,                
                Parameters = new List<ConnectionMessageParameter>()
                {
                   new ConnectionMessageParameter()
                   {
                       Name = "SecurityKey",
                       Value = createFolderRequest.SecurityKey
                   },                   
                    new ConnectionMessageParameter()
                   {
                       Name = "Path",
                       Value = createFolderRequest.Path
                   }                   
                }
            };
            return connectionMessage;
        }

        public CreateFolderRequest GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var createFolderRequest = new CreateFolderRequest()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId,
                SecurityKey = connectionMessage.Parameters.First(p => p.Name == "SecurityKey").Value,
                Path = connectionMessage.Parameters.First(p => p.Name == "Path").Value                
            };          

            return createFolderRequest;
        }
    }
}
