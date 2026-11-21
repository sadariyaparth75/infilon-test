using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Test_Taste_Console_Application.Configuration;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Services
{
    ///<summary>
    /// A service to create the HttpClient and to execute GET requests against the Solar System OpenData API.
    ///</summary>
    public class HttpClientService
    {
        public HttpClient Client { get; }

        public HttpClientService(HttpClient client, ApiSettings apiSettings)
        {
            //The HTTP client is configured in the constructor.
            Client = client;
            Client.BaseAddress = new Uri(UriPath.BaseUri);
            Client.DefaultRequestHeaders.Accept.Add(new
                MediaTypeWithQualityHeaderValue(HttpClientSettings.JsonType));

            //Since API v2.0.0 every request needs a bearer token. The key comes from configuration, never from source code.
            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(HttpClientSettings.BearerScheme, apiSettings.ApiKey);
        }

        ///<summary>
        /// Executes a GET request and deserializes the JSON response. Failures throw an exception with a clear
        /// message, instead of silently returning empty data that would look like "no planets found".
        ///</summary>
        public T GetFromJson<T>(string relativeUri) where T : class
        {
            HttpResponseMessage response;
            try
            {
                //The console app is synchronous; GetAwaiter().GetResult() surfaces the real exception instead of an AggregateException.
                response = Client.GetAsync(relativeUri).GetAwaiter().GetResult();
            }
            catch (Exception exception) when (exception is HttpRequestException || exception is TaskCanceledException)
            {
                Logger.Instance.Error($"{ExceptionMessage.ApiUnreachable}{exception.Message}", exception);
                throw new InvalidOperationException($"{ExceptionMessage.ApiUnreachable}{exception.Message}", exception);
            }

            using (response)
            {
                //The API answers 401 for a missing key and 403 for an unknown/invalid key.
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    Logger.Instance.Warn($"{LoggerMessage.GetRequestFailed}{response.StatusCode}");
                    throw new InvalidOperationException(ExceptionMessage.ApiKeyRejected);
                }

                if (!response.IsSuccessStatusCode)
                {
                    Logger.Instance.Warn($"{LoggerMessage.GetRequestFailed}{response.StatusCode}");
                    throw new HttpRequestException(
                        $"{ExceptionMessage.ApiRequestFailed}{(int)response.StatusCode} ({response.StatusCode}).");
                }

                var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return JsonConvert.DeserializeObject<T>(content);
            }
        }
    }
}
