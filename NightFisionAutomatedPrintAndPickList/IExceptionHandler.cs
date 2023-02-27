using System;
using System.Net;

namespace NightFisionAutomatedPrintAndPickList
{
    internal class IExceptionHandler
    {
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
            using (StreamWriter writer = File.AppendText(logFileName))
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
