using iTextSharp.text.pdf.qrcode;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Hosting;
using NightFisionAutomatedPrintAndPickList.Services;
using NightFisionAutomatedPrintAndPickList.Services.Interfaces;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.File;
using System.IO.Compression;
using System.IO;
using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NightFisionAutomatedPrintAndPickList
{
    public class Worker : BackgroundService
    {
        private readonly ConfigManager _configuration;

        private readonly ILogger<Worker> _logger;

        private readonly GeneratePdfService _generatePdfService;

        private UnleasheHttpClient _unleashedHttpClient;

        private PrintNodeHttpClient _printNodeHttpClient;

        private SendErrorLogEmail _sendErrorLogEmail;

        private ExceptionHandler _exceptionHandler;


        public Worker(ILogger<Worker> logger)
        {
            _configuration = new ConfigManager(_logger, "Config.xml");
            _logger = logger;
            _generatePdfService = new GeneratePdfService();
            _exceptionHandler = new ExceptionHandler();
            _unleashedHttpClient = new UnleasheHttpClient(_logger, _exceptionHandler, _configuration, new HttpClient());
            _printNodeHttpClient = new PrintNodeHttpClient(_logger, _exceptionHandler, _configuration, new HttpClient());
            var _emailService = new SmtpEmailService(_configuration);
            _sendErrorLogEmail = new SendErrorLogEmail(_logger, _emailService, _configuration);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<Timer> runningTasks = new List<Timer>();

            while (!stoppingToken.IsCancellationRequested)
            {
                XmlNodeList taskNodes = _configuration.GetTasks();

                foreach (XmlNode taskNode in taskNodes)
                {
                    
                    string enabled = taskNode.SelectSingleNode("Enabled").InnerText;

                    if (bool.Parse(enabled))
                    {
                        string interval = taskNode.SelectSingleNode("FrequencyDuration").InnerText;
                        string taskAction = taskNode.SelectSingleNode("TaskAction").InnerText;
                        string taskName = taskNode.SelectSingleNode("TaskName").InnerText;
                        string logError = taskNode.SelectSingleNode("LogError").InnerText;

                        MethodInfo taskMethod = GetType().GetMethod(taskAction);
                        

                        var runningTask = new Timer(async _ =>
                        {
                            try
                            {
                                await (Task)taskMethod.Invoke(this, new object[] { stoppingToken });

                            }
                            catch (Exception ex)
                            {
                                if (bool.Parse(logError))
                                {
                                    await _exceptionHandler.HandleExceptionAsync(ex, taskName);
                                } 
                                else
                                {
                                    _logger.LogError("["+ taskName + "] Exception received ::", ex);
                                }
                            }

                        }, null, 0, int.Parse(interval));

                        runningTasks.Add(runningTask);
                    }
                }

                await Task.Delay(Timeout.Infinite, stoppingToken);

                foreach (var task in runningTasks)
                {
                    task.Dispose();
                }
            }
        }
        
        public async Task runLabelPrintTask(CancellationToken stoppingToken)
        {
            try
            {
                DateTime now = DateTime.Now;
                // Send request to Unleashed API
                var assemblies = await _unleashedHttpClient.GetAssemblies();

                // Update the lastTimeRetrieved
                string dateNow = now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff");
                _configuration.SetTimeRetrieved("UnleashedPrintLabel", dateNow);

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
                await _exceptionHandler.HandleExceptionAsync(ex, "label");
            }
        }

        public async Task runPickNoteTask(CancellationToken stoppingToken)
        {
            try
            {
                DateTime now = DateTime.Now;
                // Send request to Unleashed API
                var assemblies = await _unleashedHttpClient.GetPickNoteAssemblies();

                // Update the lastTimeRetrieved
                string dateNow = now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff");
                _configuration.SetTimeRetrieved("UnleashedPickNote", dateNow);

                if (assemblies?.Count > 0)
                {
                    _logger.LogInformation("[PICK_NOTE] response assemblies", assemblies);
                    foreach (var assembly in assemblies)
                    {
                        if (assembly.SalesOrderNumber?.Length > 0 && assembly.Product?.Guid?.Length > 0)
                        {
                            var stockOnHand = await _unleashedHttpClient.GetStockOnHand(assembly.Product.Guid);

                            if (stockOnHand != null && stockOnHand.ProductGuid?.Length > 0)
                            {
                                _generatePdfService.GeneratePdf(assembly, stockOnHand);
                            }
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

        public async Task runLogEmailTask(CancellationToken stoppingToken)
        {
            try
            {
                
                await _sendErrorLogEmail.SendLogEmailAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("[EMAIL_SERVICE] Exception received ::", ex);
            }
        }

        public async Task runRotateLogs(CancellationToken stoppingToken)
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            string _logFilePath = Path.Combine(logPath, $"main_exceptions_{DateTime.Now.AddDays(-1):yyyyMMdd}.log");

           
            if (File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }
        }
    }
}