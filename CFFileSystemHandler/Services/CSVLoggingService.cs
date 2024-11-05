using CFFileSystemHandler.Interfaces;
using System.Text;

namespace CFFileSystemHandler.Services
{
    /// <summary>
    /// Logging to CSV
    /// </summary>
    public class CSVLoggingService : ILoggingService
    {
        private readonly string _folder;

        public CSVLoggingService(string folder)
        {
            _folder = folder;
            if (!String.IsNullOrEmpty(_folder)) Directory.CreateDirectory(_folder);
        }

        public void Log(string message)
        {
            Console.WriteLine(message);

            if (!String.IsNullOrEmpty(_folder))
            {
                var now = DateTimeOffset.UtcNow;
                var logFile = Path.Combine(_folder, $"{now.ToString("yyyy-MM-dd")}.txt");
                var delimiter = (Char)9;

                var isWriteHeaders = !File.Exists(logFile);

                using (var streamWriter = new StreamWriter(logFile, true, Encoding.UTF8))
                {
                    if (isWriteHeaders)
                    {
                        streamWriter.WriteLine($"Time{delimiter}Message");
                    }
                    streamWriter.WriteLine($"{now.ToString()}{delimiter}{message}");
                    streamWriter.Flush();
                }
            }
        }
    }
}
