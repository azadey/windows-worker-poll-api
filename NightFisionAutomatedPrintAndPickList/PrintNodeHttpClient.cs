using Newtonsoft.Json;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NightFisionAutomatedPrintAndPickList
{
    internal class PrintNodeHttpClient
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<Worker> _logger;

        private readonly HttpClient _httpClient;

        private IExceptionHandler _exceptionHandler;

        private string _printNodeApiUrl;

        private string _printNodeApiKey;

        private string _printNodePrinterId;

        public PrintNodeHttpClient(ILogger<Worker> logger, IExceptionHandler exceptionHandler, IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _exceptionHandler = exceptionHandler;
            _printNodeApiUrl = _configuration.GetValue<string>("PrintNodeApiUrl");
            _printNodeApiKey = _configuration.GetValue<string>("PrintNodeApiKey");
            _printNodePrinterId = _configuration.GetValue<string>("PrintNodePrinterId");

        }

        public async Task<HttpResponseMessage> SendPrintJobAsync(Assembly assembly, Product prodcut)
        {

            // build payload string for this assembly
            string payload = "{" +
                "\"printerId\": " + _printNodePrinterId +
                ", \"title\": \"" + assembly.SalesOrderNumber +
                "\", \"contentType\": \"" + Product.PdfUri +
                "\", \"content\": \"" + prodcut.ImageUrl +
                "\", \"source\": \"" + Assembly.AutomatedPrintService +
                "\", \"copies\": " + assembly.Quantity + "}";

            
            var requestBodyJson = System.Text.Json.JsonSerializer.Serialize(payload);

            _logger.LogInformation("The payload sent to Print node. {Payload}", requestBodyJson);

            // Create the HTTP request message
            var request = new HttpRequestMessage(HttpMethod.Post, _printNodeApiUrl);
            request.Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(_printNodeApiKey)));
            // TODO get the key
            request.Headers.Add("X-Idempotency-Key", "abcde12345");


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
                    throw new HttpException($"Print node API returned a {result.StatusCode} status code.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("[LABEL_PRINT_PRINT_NODE] Exception received ::", ex);
                await _exceptionHandler.HandleExceptionAsync(ex, "label");
            }

            return null;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {

            return await _httpClient.SendAsync(request);
        }
    }
}
