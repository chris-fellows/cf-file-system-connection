using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Common.Utilities;
using CFFileSystemConnection.Models;
using CFFileSystemConnection.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFFileSystemConnection.MessageConverters
{ 
    /// <summary>
    /// Converts between ConnectionMessage and GetFileResponse
    /// </summary>
    public class GetFileResponseMessageConverter : IExternalMessageConverter<GetFileResponse>
    {
        public ConnectionMessage GetConnectionMessage(GetFileResponse getFileResponse)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = getFileResponse.Id,
                TypeId = getFileResponse.TypeId,
                Parameters = new List<ConnectionMessageParameter>()
                {
                    new ConnectionMessageParameter()
                    {
                        Name = "Response",                        
                        Value = getFileResponse.Response == null ? "" :
                                    JsonUtilities.SerializeToBase64String(getFileResponse.Response,
                                            JsonUtilities.DefaultJsonSerializerOptions)
                    },
                    new ConnectionMessageParameter()
                   {
                       Name = "File",
                       //Value = getFileResponse.FileObject == null ? "" :
                       //               Convert.ToBase64String(Encoding.UTF8.GetBytes(XmlUtilities.SerializeToString(getFileResponse.FileObject)))
                       Value = getFileResponse.FileObject == null ? "" :
                                        JsonUtilities.SerializeToBase64String(getFileResponse.FileObject,
                                        JsonUtilities.DefaultJsonSerializerOptions)
                   }
                }
            };
            return connectionMessage;
        }

        public GetFileResponse GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var getFileResponse = new GetFileResponse()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId
            };

            // Get response
            var responseParameter = connectionMessage.Parameters.First(p => p.Name == "Response");
            if (!String.IsNullOrEmpty(responseParameter.Value))
            {
                getFileResponse.Response = JsonUtilities.DeserializeFromBase64String<MessageResponse>(responseParameter.Value, JsonUtilities.DefaultJsonSerializerOptions);                
            }

            // Get file object
            var fileParameter = connectionMessage.Parameters.First(p => p.Name == "File");
            if (!String.IsNullOrEmpty(fileParameter.Value))
            {
                getFileResponse.FileObject = JsonUtilities.DeserializeFromBase64String<FileObject>(fileParameter.Value, JsonUtilities.DefaultJsonSerializerOptions);
                //getFileResponse.FileObject = XmlUtilities.DeserializeFromString<FileObject>(Encoding.UTF8.GetString(Convert.FromBase64String(fileParameter.Value)));
            }
             
            return getFileResponse;
        }
    }
}
