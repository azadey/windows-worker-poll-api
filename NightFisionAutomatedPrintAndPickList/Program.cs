using NightFisionAutomatedPrintAndPickList;
using Serilog.Events;
using Serilog;

public static class Program
{
    public static async Task Main(string[] args)
    {
        
        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        if (!Directory.Exists(logPath))
        {
            Directory.CreateDirectory(logPath);
        }

        string _logFilePath = Path.Combine(logPath, $"main_exceptions_{DateTime.Now:yyyyMMdd}.log");


        // Configure Serilog logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File(_logFilePath)
            .CreateLogger();

        try
        {
            Log.Information("Starting service");
            await CreateHostBuilder(args).Build().RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Service terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .UseSerilog()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>();
            });
}