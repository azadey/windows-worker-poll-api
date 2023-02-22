using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        private string _printNodeApiUrl;

        private string _printNodeApiKey;

        private string _printNodePrinterId;

        public PrintNodeHttpClient(ILogger<Worker> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _printNodeApiUrl = _configuration.GetValue<string>("PrintNodeApiUrl");
            _printNodeApiKey = _configuration.GetValue<string>("PrintNodeApiKey");
            _printNodePrinterId = _configuration.GetValue<string>("PrintNodePrinterId");

        }

        public async Task<HttpResponseMessage> SendPrintJobAsync()
        {
            var requestBody = new
            {
                printerId = 34,
                title = "My Test PrintJob",
                contentType = "pdf_uri",
                content = "http://sometest.com/pdfhere",
                source = "api documentation!",
                expireAfter = 600
            };

            var requestBodyJson = JsonSerializer.Serialize(requestBody);

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
                    //var assembliesJson = await result.Content.ReadAsStringAsync();
                    _logger.LogInformation("The website is up. Status code {StatusCode}", result.StatusCode);
                    //_logger.LogInformation("The unleashed content {Content}", assembliesJson);
                    //return assembliesJson;
                }
                else
                {
                    _logger.LogError("The website is down. Status code {StatusCode}", result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception received to handle ::", ex);
            }

            return null;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {

            return await _httpClient.SendAsync(request);
        }
    }
}
