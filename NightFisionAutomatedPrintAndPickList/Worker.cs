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

        private readonly IEmailService _emailService;

        private readonly GeneratePdfService _generatePdfService;

        private readonly int _labelInterval;

        private readonly int _pickNoteInterval;

        private readonly int _logEmailInterval;

        private UnleasheHttpClient _unleashedHttpClient;

        private PrintNodeHttpClient _printNodeHttpClient;

        private SendErrorLogEmail _sendErrorLogEmail;

        private IExceptionHandler _exceptionHandler;


        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
            _emailService = new SmtpEmailService(_configuration);
            _generatePdfService = new GeneratePdfService();
            _labelInterval = _configuration.GetValue<int>("UnleashedApiLabelInterval");
            _pickNoteInterval = _configuration.GetValue<int>("UnleashedApiPickNoteInterval");
            _logEmailInterval = _configuration.GetValue<int>("EmailServiceInterval");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _unleashedHttpClient = new UnleasheHttpClient(_logger, _exceptionHandler, _configuration, new HttpClient());
            _printNodeHttpClient = new PrintNodeHttpClient(_logger, _exceptionHandler, _configuration, new HttpClient());
            _sendErrorLogEmail = new SendErrorLogEmail(_logger, _configuration, _emailService);
            _exceptionHandler = new IExceptionHandler();

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

                var labelTime = new Timer(async _ =>
                {
                    try
                    {
                        await runLabelPrintTask(stoppingToken);

                    }
                    catch (Exception ex)
                    {
                        await _exceptionHandler.HandleExceptionAsync(ex, "label");
                    }

                }, null, 0, _labelInterval);
                
                
                var pickNoteTime = new Timer(async _ =>
                {
                    try
                    {
                        await runPickNoteTask(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        await _exceptionHandler.HandleExceptionAsync(ex, "picknote");
                    }


                }, null, 0, _pickNoteInterval);

                var logEmailTime = new Timer(async _ =>
                {
                    try
                    {
                        await runLogEmailTask(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[EMAIL_SERVICE] Exception received ::", ex);
                    }

                }, null, 0, _logEmailInterval);

                await Task.Delay(Timeout.Infinite, stoppingToken);

                labelTime.Dispose();
                pickNoteTime.Dispose();

                _logger.LogInformation("Unleashed workers stopped.");
            }
        }

        private async Task runLabelPrintTask(CancellationToken stoppingToken)
        {
            try
            {
                // Send request to Unleashed API
                var assemblies = await _unleashedHttpClient.GetAssemblies(_labelInterval);

                if (assemblies?.Count > 0)
                {
                    foreach (var assembly in assemblies)
                    {

                        if (assembly.SalesOrderNumber?.Length > 0 && assembly.Product?.Guid?.Length > 0)
                        {
                            var product = await _unleashedHttpClient.GetProduct(assembly.Product.Guid);

                            if (product != null && product.ImageUrl?.Length > 0)
                            {
                                var printNodeResponse = await _printNodeHttpClient.SendPrintJobAsync(assembly, product);

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("[LABEL_PRINT] Exception received ::", ex);
                await _exceptionHandler.HandleExceptionAsync(ex, "label");
            }
        }

        private async Task runPickNoteTask(CancellationToken stoppingToken)
        {
            try
            {
                // Send request to Unleashed API
                var assemblies = await _unleashedHttpClient.GetPickNoteAssemblies(_pickNoteInterval);

                if (assemblies?.Count > 0)
                {
                    foreach (var assembly in assemblies)
                    {
                        if (assembly.SalesOrderNumber?.Length > 0)
                        {
                            _generatePdfService.GeneratePdf("path");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("[PICK_NOTE] Exception received ::", ex);
                await _exceptionHandler.HandleExceptionAsync(ex, "picknote");
            }
        }

        private async Task runLogEmailTask(CancellationToken stoppingToken)
        {
            try
            {
                await _sendErrorLogEmail.SendLogEmailAsync(stoppingToken, _logEmailInterval);
            }
            catch (Exception ex)
            {
                _logger.LogError("[EMAIL_SERVICE] Exception received ::", ex);
            }
        }

    }
}