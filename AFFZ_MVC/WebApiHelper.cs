using AFFZ_Customer.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace AFFZ_Customer
{
    public class WebApiHelper
    {
        private static string _baseUrl;

        public WebApiHelper(IOptions<AppSettings> appSettings)
        {
            // Initialize the base URL from the app settings
            _baseUrl = appSettings.Value.ApiHttpsPort;
        }
        public static async Task<string> GetData(string RequestUrl)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                string responseData = string.Empty;

                using (var client = new HttpClient(handler)) // Pass handler here
                {
                    client.BaseAddress = new Uri(_baseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // GET Method
                    HttpResponseMessage response = await client.GetAsync(RequestUrl);
                    responseData = await response.Content.ReadAsStringAsync();
                }

                return responseData;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<string> PostData<T>(string RequestUrl, T postData)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                string responseData = string.Empty;
                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri(_baseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Convert the post data to JSON
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json");

                    // POST Method
                    HttpResponseMessage response = await client.PostAsync(RequestUrl, jsonContent);
                    if (response.IsSuccessStatusCode)
                    {
                        responseData = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        responseData = await response.Content.ReadAsStringAsync();
                    }
                }
                return responseData.Trim('"'); // Remove surrounding quotes from the response string;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
