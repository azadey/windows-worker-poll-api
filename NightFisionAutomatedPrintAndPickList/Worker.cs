using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.File;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;

namespace NightFisionAutomatedPrintAndPickList
{
    public class Worker : BackgroundService
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<Worker> _logger;
        
        private UnleasheHttpClient _unleashedHttpClient;

        private UnleashedExceptionService _unleashedExceptionService;


        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _unleashedHttpClient = new UnleasheHttpClient(_logger, _configuration, new HttpClient());
            _unleashedExceptionService = new UnleashedExceptionService();
            
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                // Send request to Unleashed API
                var assemblies = await _unleashedHttpClient.GetAssemblies();

                if (assemblies?.Count > 0)
                {
                    // Send to printer node
                    _logger.LogError("Send data to Print Node {@Assemblies}", assemblies);
                }

                await Task.Delay(_configuration.GetValue<int>("UnleashedApiTiming"), stoppingToken);
            }
        }

    }
}