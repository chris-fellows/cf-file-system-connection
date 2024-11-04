using CFFileSystemConnection;
using CFFileSystemConnection.Common;
using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Service;

namespace CFFileSystemHandler
{
    /// <summary>
    /// Server processing. Listens on port and handles file system requests.
    /// </summary>
    internal class Server
    {
        private readonly FileSystemRequestHandler _fileSystemRequestHandler;

        public Server(IUserService userService)
        {           
            _fileSystemRequestHandler = new FileSystemRequestHandler(new FileSystemLocal(), userService);
            _fileSystemRequestHandler.OnStatusMessage += _fileSystemRequestHandler_OnStatusMessage;
        }

        private void _fileSystemRequestHandler_OnStatusMessage(string message)
        {
            Console.WriteLine($"{DateTimeOffset.UtcNow} {message}");
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
