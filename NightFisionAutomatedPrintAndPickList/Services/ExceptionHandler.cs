using System;
using System.Net;

namespace NightFisionAutomatedPrintAndPickList.Services
{
    internal class ExceptionHandler
    {
        private static object _logLock = new object();

        public async Task HandleExceptionAsync(Exception ex, string taskName)
        {

            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            // Check if the Logs directory exists, create it if it doesn't
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            // Generate log file name based on task name
            string logFileName = Path.Combine(logPath, $"{taskName}_exceptions_{DateTime.Now:yyyyMMdd}.log");

            // Create or append to log file
            lock (_logLock)
            {
                // Open the log file with shared write access
                using (FileStream fs = new FileStream(logFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.WriteLine($"[{DateTime.Now}] Exception occurred:");
                    writer.WriteLine($"Task Name: {taskName}");
                    writer.WriteLine($"Exception message: {ex.Message}");
                    writer.WriteLine($"Stack trace: {ex.StackTrace}");
                    writer.WriteLine(new string('-', 50));
                }
            }
        }
    }
}
