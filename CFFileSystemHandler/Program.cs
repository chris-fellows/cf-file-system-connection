// See https://aka.ms/new-console-template for more information
using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Service;
using CFFileSystemHandler;

try
{
    Console.WriteLine("Starting Request Handler");

    // Create users
    IUserService userService = new JsonUserService(Path.Combine(Environment.CurrentDirectory, "Data"));
    InternalUtilities.CreateUsers(userService);

    // Start listening
    const int port = 11000;      
    var server = new Server(userService);
    server.StartListening(port);

    Console.WriteLine($"Request Handler listening (Port {port})");

    // Run until cancelled
    var cancellationTokenSource = new CancellationTokenSource();
    server.Run(cancellationTokenSource.Token);

    Console.WriteLine("Terminating Request Handler");
}
catch(Exception exception)
{
    Console.WriteLine($"Exception: {exception.Message}");
    Console.WriteLine($"Stack: {exception.StackTrace}");
}

var result = Console.ReadLine();
