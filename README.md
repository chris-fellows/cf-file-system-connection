# cf-file-system-connection

Class library for easily accessing remote file system via a TCP connection without having to write
any socket level code. An example would be a PC application that displays a file explorer for
managing files or backing up files on a mobile phone.

There would typically be two applications:
1) The application that manages the remote file system. It uses the IFileSystem interface.
2) The application that provides it's file system. It uses the FileSystemRequestHandler class.

WARNING: If you provide remote access to the file system then you should be careful about protecting
access to files. Requests must provide a security key.

CFFileSystemConnection.Common
-----------------------------
This class library provides the components for managing a local or remote file system.

It makes uses of the CFConnectionMessaging.Common library which implements sending & receiving of
messages via TCP/UDP.

The class library can be used in the following ways:
1) By an application wanting to manage a remote file system. E.g. On a mobile phone.
2) By the application wanting to expose it's file system via TCP. E.g. An .NET Maui Android application
   that provides access to it's file system. In this instance then the application would host an instance
   of the FileSystemRequestHandler class.

Main components:
IFileSystem 						: Interface to file system.
FileSystemLocal (IFileSystem)		: Provides access to local file system.
FileSystemConnection (IFileSystem)	: Provides access to remote file system via TCP connection.
FolderObject						: Folder details.
FileObject							: File details.
FileSystemRequestHandler			: Handles request messages received via TCP.
IUserService						: Service that stores users and associated security keys.

CFFileSystemHandler
-------------------
A sample console application that demonstrates the handler that owns the file system and can process
messages for the file system. E.g. Get folder details.

The application hosts an instance of the FileSystemRequestHandler class.

CFFileSystemManager
-------------------
A WinForms application that uses the remote connection to manage the remote file system.

The application uses the FileSystemConnection (IFileSystem) class to access the remote file system.

The CFFileSystemHandler application can be used for providing the remote file system.

CFFileSystemMobile
------------------
A .NET Maui application that allows the file system to be managed remotely.

The application hosts an instance of the FileSystemRequestHandler class.

Security
--------
Access is controlled by specifying a security key in the request message. The security key relates to a 
user which has a list of roles defined.