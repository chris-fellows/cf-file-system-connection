// See https://aka.ms/new-console-template for more information
using CFFileSystemConnection.Models;
using CFFileSystemHandler;

try
{
    Console.WriteLine("Starting Request Handler");

    // Start listening
    const int port = 11000;
    var server = new Server();
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
