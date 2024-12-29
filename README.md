# cf-file-system-connection

Class library for easily accessing remote file system via a TCP connection without having to write
any socket level code. An example would be a PC application that displays a file explorer for managing
files or backing up files on a mobile phone.

There would typically be two applications:
1) The application that manages the remote file system. It uses the IFileSystem interface.
2) The application that provides it's file system. It uses the FileSystemRequestHandler class.

WARNING: If you provide remote access to the file system then you should be careful about protecting
access to files. Requests must provide a security key. The mechanism is really only intended for
management of particular folders (E.g. Music, photos etc)

CFFileSystemConnection.Common (Class library)
---------------------------------------------
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

CFFileSystemHandler (Console)
-----------------------------
A sample console application that demonstrates the handler that owns the file system and can process
messages for the file system. E.g. Get folder details.

The application hosts an instance of the FileSystemRequestHandler class.

CFFileSystemManager (WinForms)
------------------------------
A WinForms application that uses the remote connection to manage the remote file system.

The application uses the FileSystemConnection (IFileSystem) class to access the remote file system.

The CFFileSystemHandler application can be used for providing the remote file system.

CFFileSystemMobile (.NET Maui)
------------------------------
A .NET Maui application that allows the file system to be managed remotely without having to connect the 
phone to a computer via USB cable. It is also possible to configure the list of users, security key and
associated permissions.

The application hosts an instance of the FileSystemRequestHandler class.

Users & Security
----------------
Access is controlled by specifying a security key in the request message. The security key relates to a 
user. Users are managed via the IUserService interface.

For each user then it's possible to set the following settings:
1) Which roles that the user has.
2) Which folder paths that the user has access to.

It is strongly recommended that security is as strict as possible to limit which paths that the user can
access and whether they can read/write to the file system.

How To Remotely Manage Files On a Phone
---------------------------------------
1) Install CFFileSystemMobile on the phone. Currently only Android.
2) Run CFFileSystemMobile and configure a user in User Settings.
3) Note down the IP address for the phone and the security key created in User Settings.
4) Click on the Start Listening button.
4) Run CFFileSystemManager and configure the IP, port & security key for the phone.