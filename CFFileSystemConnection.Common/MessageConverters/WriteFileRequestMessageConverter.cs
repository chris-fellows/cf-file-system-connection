using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Constants;
using CFFileSystemConnection.Models;
using CFFileSystemConnection.Utilities;

namespace CFFileSystemConnection.MessageConverters
{
    public class WriteFileRequestMessageConverter : IExternalMessageConverter<WriteFileRequest>
    {
        public ConnectionMessage GetConnectionMessage(WriteFileRequest writeFileRequest)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = writeFileRequest.Id,
                TypeId = writeFileRequest.TypeId,                
                Parameters = new List<ConnectionMessageParameter>()
                {
                   new ConnectionMessageParameter()
                   {
                       Name = "SecurityKey",
                       Value = writeFileRequest.SecurityKey
                   },
                    new ConnectionMessageParameter()
                   {
                       Name = "SessionId",
                       Value = writeFileRequest.SessionId
                   },
                    new ConnectionMessageParameter()
                   {
                       Name = "File",
                       Value = writeFileRequest.FileObject == null ? "" :
                                    JsonUtilities.SerializeToBase64String(writeFileRequest.FileObject,
                                            JsonUtilities.DefaultJsonSerializerOptions)
                   },
                   new ConnectionMessageParameter()
                   {
                       Name = "Content",
                       Value = writeFileRequest.Content == null ? "" :
                                        Convert.ToBase64String(writeFileRequest.Content)
                   },
                   new ConnectionMessageParameter()
                   {
                       Name= "IsMore",
                       Value = writeFileRequest.IsMore.ToString()
                   }
                }
            };
            return connectionMessage;
        }

        public WriteFileRequest GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var writeFileRequest = new WriteFileRequest()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId,
                SecurityKey = connectionMessage.Parameters.First(p => p.Name == "SecurityKey").Value,
                SessionId = connectionMessage.Parameters.First(p => p.Name == "SessionId").Value,
                IsMore = Convert.ToBoolean(connectionMessage.Parameters.First(p => p.Name == "IsMore").Value)
            };

            // Get folder object
            var fileParameter = connectionMessage.Parameters.First(p => p.Name == "File");
            if (!String.IsNullOrEmpty(fileParameter.Value))
            {
                writeFileRequest.FileObject = JsonUtilities.DeserializeFromBase64String<FileObject>(fileParameter.Value, JsonUtilities.DefaultJsonSerializerOptions);
            }

            // Get content object
            var contentParameter = connectionMessage.Parameters.First(p => p.Name == "Content");
            if (!String.IsNullOrEmpty(contentParameter.Value))
            {
                writeFileRequest.Content = Convert.FromBase64String(contentParameter.Value);
            }

            return writeFileRequest;
        }
    }
}
