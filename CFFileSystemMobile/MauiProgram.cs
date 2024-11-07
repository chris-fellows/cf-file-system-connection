using CFFileSystemConnection.Common;
using CFFileSystemConnection.Service;
using CFFileSystemMobile.Interfaces;
using CFFileSystemMobile.Models;
using CFFileSystemMobile.Utilities;
using CFFileSystemMobile.ViewModels;
using Microsoft.Extensions.Logging;

namespace CFFileSystemMobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Register pages for DI
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<UserSettingsPage>();
            builder.Services.AddSingleton<UserSettingsPageModel>();

            builder.Services.AddSingleton<ICurrentState, CurrentState>();
            builder.Services.AddSingleton<CFFileSystemConnection.Interfaces.IFileSystem, FileSystemLocal>();
            builder.Services.AddSingleton<CFFileSystemConnection.Interfaces.IUserService>((scope) =>
            {
                var userService = new JsonUserService(Path.Combine(FileSystem.AppDataDirectory, "Users"));

                // Create initial users
                // TODO: Do this properly
                var users = userService.GetAll();
                if (!users.Any())
                {
                    InternalUtilities.CreateUsers(userService);
                }
                return userService;
            });

            return builder.Build();
        }
    }
}
