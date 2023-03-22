using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NightFisionAutomatedPrintAndPickList.Services
{
    internal class PrintNodeHttpClient
    {
        private readonly ConfigManager _configuration;

        private readonly ILogger<Worker> _logger;

        private readonly HttpClient _httpClient;

        private ExceptionHandler _exceptionHandler;

        private string _printNodeApiUrl;

        private string _printNodeApiPrintUrl;

        private string _printNodeApiKey;

        private string _printNodePrinterId;

        public PrintNodeHttpClient(ILogger<Worker> logger, ExceptionHandler exceptionHandler, ConfigManager configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _exceptionHandler = exceptionHandler;
            _printNodeApiUrl = _configuration.GetClientSettings("PrintNode", "BaseUrl");
            _printNodeApiPrintUrl = _configuration.GetClientSettings("PrintNode", "PrintJobPath");
            _printNodeApiKey = _configuration.GetClientSettings("PrintNode", "ApiKey");
            _printNodePrinterId = _configuration.GetClientSettings("PrintNode", "PrinterId");
        }

        public async Task<HttpResponseMessage> SendPrintJobAsync(Assembly assembly, Product product)
        {

            var contentPayload = ImageUrlToBase64(product.ImageUrl);

            // build payload string for this assembly
            var payload = new
            {
                printerId = _printNodePrinterId,
                title = assembly.SalesOrderNumber,
                contentType = Product.RawBase64,
                content = contentPayload,
                source = Assembly.AutomatedPrintService,
                copies = assembly.Quantity
            };

            var requestBodyJson = System.Text.Json.JsonSerializer.Serialize(payload);
            // Create the HTTP request message
            var request = new HttpRequestMessage(HttpMethod.Post, _printNodeApiUrl + _printNodeApiPrintUrl);
            request.Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(_printNodeApiKey)));
            request.Headers.Add("X-Idempotency-Key", assembly.AssemblyNumber);


            try
            {
                var result = await SendAsync(request);

                if (result.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[LABEL_PRINT_PRINT_NODE] successfully send to print node {StatusCode}", result.StatusCode);
                }
                else
                {
                    _logger.LogError("[LABEL_PRINT_PRINT_NODE] API returned status code {StausCode}", result.StatusCode);
                    throw new HttpRequestException($"Print node API returned a {result.StatusCode} status code.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("[LABEL_PRINT_PRINT_NODE] Exception received ::", ex);
                await _exceptionHandler.HandleExceptionAsync(ex, "label");
            }

            return null;
        }

        public static string ImageUrlToBase64(string url)
        {
            using (WebClient webClient = new WebClient())
            {
                byte[] imageBytes = webClient.DownloadData(url);
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {

            return await _httpClient.SendAsync(request);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _httpClient.Dispose();
            return Task.CompletedTask;
        }
    }
}
