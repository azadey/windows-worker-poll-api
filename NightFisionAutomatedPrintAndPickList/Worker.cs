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
        
        private HttpClient _httpClient;

        private string _unleashedApiUrl;

        private string _unleashedApiId;

        private string _unleashedApiKey;

        private string _unleasheApiArgs;


        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _httpClient = new HttpClient();
            _unleashedApiUrl = _configuration.GetValue<string>("UnleashedApiUrl");
            _unleashedApiId = _configuration.GetValue<string>("UnleashedApiId");
            _unleashedApiKey = _configuration.GetValue<string>("UnleashedApiKey");
            _unleasheApiArgs = _configuration.GetValue<string>("UnleashedApiArgs");
            
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _httpClient.Dispose();
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                var content = new StringContent("{}", Encoding.UTF8, "application/json");
                string signature = GetSignature(_unleasheApiArgs, _unleashedApiKey);
                var _request = new HttpRequestMessage(HttpMethod.Get, _unleashedApiUrl) { Content = content };
                _request.Headers.Add("Accept", "application/json");
                _request.Headers.Add("api-auth-id", _unleashedApiId);
                _request.Headers.Add("api-auth-signature", signature);
                _request.Headers.Add("client-type", "API-Sandbox");

                var result = await _httpClient.SendAsync(_request);
                
                if (result.IsSuccessStatusCode)
                {
                    var jsonContent = await result.Content.ReadAsStringAsync();
                    _logger.LogInformation("The website is up. Status code {StatusCode}", result.StatusCode);
                    _logger.LogInformation("The unleashed content {Content}", jsonContent);
                } 
                else
                {
                    _logger.LogError("The website is down. Status code {StatusCode}", result.StatusCode);
                }


                await Task.Delay(5000, stoppingToken);
            }
        }

        private static string GetSignature(string args, string privatekey)
        {
            var encoding = new System.Text.UTF8Encoding();
            byte[] key = encoding.GetBytes(privatekey);
            var myhmacsha256 = new HMACSHA256(key);
            byte[] hashValue = myhmacsha256.ComputeHash(encoding.GetBytes(args));
            string hmac64 = Convert.ToBase64String(hashValue);
            myhmacsha256.Clear();
            return hmac64;
        }
    }
}