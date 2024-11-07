using CFFileSystemConnection.Common;
using CFFileSystemConnection.Interfaces;
using CFFileSystemHandler.Interfaces;

namespace CFFileSystemHandler
{
    /// <summary>
    /// Server processing. Listens on port and handles file system requests.
    /// </summary>
    internal class Server
    {
        private readonly FileSystemRequestHandler _fileSystemRequestHandler;
        private readonly ILoggingService _loggingService;

        public Server(ILoggingService loggingService, IUserService userService)
        {           
            _fileSystemRequestHandler = new FileSystemRequestHandler(new FileSystemLocal(), userService);
            _fileSystemRequestHandler.OnStatusMessage += _fileSystemRequestHandler_OnStatusMessage;
            _fileSystemRequestHandler.OnClientConnected += _fileSystemRequestHandler_OnClientConnected;
            _fileSystemRequestHandler.OnClientDisconnected += _fileSystemRequestHandler_OnClientDisconnected;

            _loggingService = loggingService;
        }

        private void _fileSystemRequestHandler_OnClientDisconnected(CFConnectionMessaging.Models.EndpointInfo endpointInfo)
        {
            LogMessage($"Client connected: {endpointInfo.Ip}:{endpointInfo.Port}");
        }

        private void _fileSystemRequestHandler_OnClientConnected(CFConnectionMessaging.Models.EndpointInfo endpointInfo)
        {
            LogMessage($"Client disconnected: {endpointInfo.Ip}:{endpointInfo.Port}");
        }

        private void LogMessage(string message)
        {
            Console.WriteLine($"{DateTimeOffset.UtcNow} {message}");
            _loggingService.Log(message);
        }

        private void _fileSystemRequestHandler_OnStatusMessage(string message)
        {
            LogMessage(message);
        }

        public void StartListening(int port)
        {
            _fileSystemRequestHandler.StartListening(port);
            LogMessage($"Listening (Port: {port})");
        }

        public void StopListening()
        {
            _fileSystemRequestHandler.StopListening();
            LogMessage("Stopped listening");
        }

        /// <summary>
        /// Runs until cancelled
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
