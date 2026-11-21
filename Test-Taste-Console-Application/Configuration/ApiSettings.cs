using System;
using Microsoft.Extensions.Configuration;
using Test_Taste_Console_Application.Constants;

namespace Test_Taste_Console_Application.Configuration
{
    ///<summary>
    /// Settings needed to call the Solar System OpenData API. The API key is never stored in source code;
    /// it is read from the environment, .NET user secrets or a git-ignored appsettings.json.
    ///</summary>
    public class ApiSettings
    {
        public string ApiKey { get; }

        public ApiSettings(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(ExceptionMessage.ApiKeyMissing);
            }

            ApiKey = apiKey.Trim();
        }

        ///<summary>
        /// Resolves the API key. The environment variable wins over the configuration (user secrets,
        /// then appsettings.json), so a key can be overridden per run (e.g. in CI) without touching the stored one.
        ///</summary>
        public static ApiSettings FromConfiguration(IConfiguration configuration)
        {
            var apiKey = Environment.GetEnvironmentVariable(HttpClientSettings.ApiKeySettingName);

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = configuration[HttpClientSettings.ApiKeySettingName];
            }

            return new ApiSettings(apiKey);
        }
    }
}
