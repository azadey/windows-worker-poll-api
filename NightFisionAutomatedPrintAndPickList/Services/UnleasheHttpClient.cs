using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NightFisionAutomatedPrintAndPickList.Services
{
    internal class UnleasheHttpClient
    {
        private readonly ConfigManager _configuration;

        private readonly ILogger<Worker> _logger;

        private readonly HttpClient _httpClient;

        private readonly ExceptionHandler _exceptionHandler;

        private string _unleashedApiBaseUrl;

        private string _unleashedApiAssemblyUrl;

        private string _unleashedApiProductUrl;

        private string _unleashedApiStockOnHandUrl;

        private string _unleashedApiId;

        private string _unleashedApiKey;

        private string _unleasheApiArgs;

        public UnleasheHttpClient(ILogger<Worker> logger, ExceptionHandler exceptionHandler, ConfigManager configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _exceptionHandler = exceptionHandler;

            _unleashedApiBaseUrl = _configuration.GetClientSettings("Unleashed", "BaseUrl");
            _unleashedApiId = _configuration.GetClientSettings("Unleashed", "ApiID");
            _unleashedApiKey = _configuration.GetClientSettings("Unleashed", "ApiKey");
            _unleasheApiArgs = _configuration.GetClientSettings("Unleashed", "Args");
            _unleashedApiAssemblyUrl = _configuration.GetClientSettings("Unleashed", "AssemblyPath");
            _unleashedApiProductUrl = _configuration.GetClientSettings("Unleashed", "ProductPath");
            _unleashedApiStockOnHandUrl = _configuration.GetClientSettings("Unleashed", "StockOnHandPath");
        }

        public async Task<List<Assembly>?> GetAssemblies()
        {
            string lastTimeRetrieved = _configuration.GetTaskSettings("UnleashedPrintLabel", "LastOfficialTimeRetrieved");
            _unleasheApiArgs += "&modifiedSince=" + lastTimeRetrieved;
            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            string signature = GetSignature(_unleasheApiArgs, _unleashedApiKey);

            var request = new HttpRequestMessage(HttpMethod.Get, _unleashedApiBaseUrl + _unleashedApiAssemblyUrl + '?' + _unleasheApiArgs) { Content = content };
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
                        var assemblies = JsonConvert.DeserializeObject<Response>(assembliesJson);

                        _logger.LogInformation("[UNLEASHED_GET_ASSEMBLY] successfully retreived assemblies {StatusCode}", result.StatusCode);

                        return assemblies?.Items;
                    }
                    else
                    {
                        _logger.LogError("[UNLEASHED_GET_ASSEMBLY] API returned status code {StausCode}", result.StatusCode);
                        throw new HttpRequestException($"Unleashed API returned a {result.StatusCode} status code.");
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError("[UNLEASHED_GET_ASSEMBLY] Exception received ::", ex);
                await _exceptionHandler.HandleExceptionAsync(ex, "label");
            }

            return null;
        }

        public async Task<List<Assembly>?> GetPickNoteAssemblies()
        {
            string lastTimeRetrieved = _configuration.GetTaskSettings("UnleashedPickNote", "LastOfficialTimeRetrieved");
            string unleasheApiArgs = "&startDate=" + lastTimeRetrieved;
            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            string signature = GetSignature(_unleasheApiArgs, _unleashedApiKey);

            var request = new HttpRequestMessage(HttpMethod.Get, _unleashedApiBaseUrl + _unleashedApiAssemblyUrl + '?' + _unleasheApiArgs) { Content = content };
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
                        var assemblies = JsonConvert.DeserializeObject<Response>(assembliesJson);

                        _logger.LogInformation("[UNLEASHED_GET_ASSEMBLY_PICKNOTE] successfully retreived assemblies {StatusCode}", result.StatusCode);

                        return assemblies?.Items;
                    }
                    else
                    {
                        _logger.LogError("[UNLEASHED_GET_ASSEMBLY_PICKNOTE] API returned status code {StausCode}", result.StatusCode);
                        throw new HttpRequestException($"Unleashed API returned a {result.StatusCode} status code.");
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError("[UNLEASHED_GET_ASSEMBLY_PICKNOTE] Exception received ::", ex);
                await _exceptionHandler.HandleExceptionAsync(ex, "label");
            }

            return null;
        }

        public async Task<Product?> GetProduct(string productId)
        {
            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            string signature = GetSignature("", _unleashedApiKey);

            var request = new HttpRequestMessage(HttpMethod.Get, _unleashedApiBaseUrl + _unleashedApiProductUrl + '/' + productId) { Content = content };
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
                        var productJson = await result.Content.ReadAsStringAsync();
                        var product = JsonConvert.DeserializeObject<Product>(productJson);

                        _logger.LogInformation("[UNLEASHED_GET_PRODUCT] successfully retreived product {StatusCode}", result.StatusCode);

                        return product;
                    }
                    else
                    {
                        _logger.LogError("[UNLEASHED_GET_PRODUCT] API returned status code {StausCode}", result.StatusCode);
                        throw new HttpRequestException($"Unleashed API returned a {result.StatusCode} status code.");
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError("[UNLEASHED_GET_PRODUCT] Exception received ::", ex);
                await _exceptionHandler.HandleExceptionAsync(ex, "label");
            }

            return null;
        }

        public async Task<StockOnHand?> GetStockOnHand(string productId)
        {
            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            string signature = GetSignature("", _unleashedApiKey);

            var request = new HttpRequestMessage(HttpMethod.Get, _unleashedApiBaseUrl + _unleashedApiStockOnHandUrl + '/' + productId) { Content = content };
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
                        var productJson = await result.Content.ReadAsStringAsync();
                        var stockOnHand = JsonConvert.DeserializeObject<StockOnHand>(productJson);

                        _logger.LogInformation("[UNLEASHED_GET_STOCK_ON_HAND] successfully retrieved stock {StatusCode}", result.StatusCode);

                        return stockOnHand;
                    }
                    else
                    {
                        _logger.LogError("[UNLEASHED_GET_STOCK_ON_HAND] API returned status code {StausCode}", result.StatusCode);
                        throw new HttpRequestException($"Unleashed API returned a {result.StatusCode} status code.");
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError("[UNLEASHED_GET_STOCK_ON_HAND] Exception received ::", ex);
                await _exceptionHandler.HandleExceptionAsync(ex, "picknote");
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
            var encoding = new UTF8Encoding();
            byte[] key = encoding.GetBytes(privatekey);
            var myhmacsha256 = new HMACSHA256(key);
            byte[] hashValue = myhmacsha256.ComputeHash(encoding.GetBytes(args));
            string hmac64 = Convert.ToBase64String(hashValue);
            myhmacsha256.Clear();
            return hmac64;
        }
    }

}
