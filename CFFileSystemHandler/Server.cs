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

            _loggingService = loggingService;
        }

        private void _fileSystemRequestHandler_OnStatusMessage(string message)
        {
            Console.WriteLine($"{DateTimeOffset.UtcNow} {message}");
            _loggingService.Log(message);
        }

        public void StartListening(int port)
        {
            _fileSystemRequestHandler.StartListening(port);
        }

        public void StopListening()
        {
            _fileSystemRequestHandler.StopListening();
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
