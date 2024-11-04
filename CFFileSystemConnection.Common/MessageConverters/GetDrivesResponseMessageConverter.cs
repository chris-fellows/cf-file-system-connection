using CFConnectionMessaging.Interfaces;
using CFConnectionMessaging.Models;
using CFFileSystemConnection.Models;
using CFFileSystemConnection.Utilities;

namespace CFFileSystemConnection.MessageConverters
{
    public class GetDrivesResponseMessageConverter : IExternalMessageConverter<GetDrivesResponse>
    {
        public ConnectionMessage GetConnectionMessage(GetDrivesResponse getDrivesResponse)
        {
            var connectionMessage = new ConnectionMessage()
            {
                Id = getDrivesResponse.Id,
                TypeId = getDrivesResponse.TypeId,
                Parameters = new List<ConnectionMessageParameter>()
                {
                    new ConnectionMessageParameter()
                    {
                        Name = "Response",
                        Value = getDrivesResponse.Response == null ? "" :
                                    JsonUtilities.SerializeToBase64String(getDrivesResponse.Response,
                                            JsonUtilities.DefaultJsonSerializerOptions)
                    },
                   new ConnectionMessageParameter()
                   {
                       Name = "Drives",
                       Value = getDrivesResponse.Drives == null ? "" :
                                    JsonUtilities.SerializeToBase64String(getDrivesResponse.Drives,
                                            JsonUtilities.DefaultJsonSerializerOptions)
                   }
                }
            };
            return connectionMessage;
        }

        public GetDrivesResponse GetExternalMessage(ConnectionMessage connectionMessage)
        {
            var getDrivesResponse = new GetDrivesResponse()
            {
                Id = connectionMessage.Id,
                TypeId = connectionMessage.TypeId                
            };

            // Get response
            var responseParameter = connectionMessage.Parameters.First(p => p.Name == "Response");
            if (!String.IsNullOrEmpty(responseParameter.Value))
            {
                getDrivesResponse.Response = JsonUtilities.DeserializeFromBase64String<MessageResponse>(responseParameter.Value, JsonUtilities.DefaultJsonSerializerOptions);
            }

            // Get folder object
            var drivesParameter = connectionMessage.Parameters.First(p => p.Name == "Drives");
            if (!String.IsNullOrEmpty(drivesParameter.Value))
            {
                getDrivesResponse.Drives = JsonUtilities.DeserializeFromBase64String<List<DriveObject>>(drivesParameter.Value, JsonUtilities.DefaultJsonSerializerOptions);
            }

            return getDrivesResponse;
        }
    }
}
