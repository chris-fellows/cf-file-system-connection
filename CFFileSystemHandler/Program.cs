// See https://aka.ms/new-console-template for more information
using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Service;
using CFFileSystemHandler;
using CFFileSystemHandler.Interfaces;
using CFFileSystemHandler.Services;
using CFFileSystemHandler.Utilities;

// Create logging service
ILoggingService loggingService = new CSVLoggingService(Path.Combine(Environment.CurrentDirectory, "Logs"));

try
{
    loggingService.Log("Starting File System Handler");

    // Create users
    IUserService userService = new JsonUserService(Path.Combine(Environment.CurrentDirectory, "Data"));
    InternalUtilities.CreateUsers(userService);

    // Start listening
    const int port = 11000;
    var server = new Server(loggingService, userService);
    server.StartListening(port);

    // Run until cancelled
    var cancellationTokenSource = new CancellationTokenSource();    // Currently no wait to cancel cleanly
    server.Run(cancellationTokenSource.Token);
}
catch (Exception exception)
{
    loggingService.Log($"Exception: {exception.Message}");
    loggingService.Log($"Stack: {exception.StackTrace}");
}
finally
{
    loggingService.Log("Terminating File System Handler");
}

