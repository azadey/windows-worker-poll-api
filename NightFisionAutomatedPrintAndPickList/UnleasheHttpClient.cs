using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NightFisionAutomatedPrintAndPickList
{
    internal class UnleasheHttpClient
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<Worker> _logger;

        private readonly HttpClient _httpClient;

        private string _unleashedApiUrl;

        private string _unleashedApiId;

        private string _unleashedApiKey;

        private string _unleasheApiArgs;

        public UnleasheHttpClient(ILogger<Worker> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _unleashedApiUrl = _configuration.GetValue<string>("UnleashedApiUrl");
            _unleashedApiId = _configuration.GetValue<string>("UnleashedApiId");
            _unleashedApiKey = _configuration.GetValue<string>("UnleashedApiKey");
            _unleasheApiArgs = _configuration.GetValue<string>("UnleashedApiArgs");

        }

        public async Task<List<Item>?> GetAssemblies()
        {
            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            string signature = GetSignature(_unleasheApiArgs, _unleashedApiKey);
            var request = new HttpRequestMessage(HttpMethod.Get, _unleashedApiUrl + '?' + _unleasheApiArgs) { Content = content };
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("api-auth-id", _unleashedApiId);
            request.Headers.Add("api-auth-signature", signature);
            request.Headers.Add("client-type", "API-Sandbox");

            try
            {
                using (var result = await SendAsync(request))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        var assembliesJson = await result.Content.ReadAsStringAsync();
                        _logger.LogInformation("The website is up. Status code {StatusCode}", result.StatusCode);
                        var assemblies = JsonConvert.DeserializeObject<Assembly>(assembliesJson);
                        return assemblies?.Items;
                    }
                    else
                    {
                        _logger.LogError("The website is down. Status code {StatusCode}", result.StatusCode);
                    }

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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _httpClient.Dispose();
            return Task.CompletedTask;
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
