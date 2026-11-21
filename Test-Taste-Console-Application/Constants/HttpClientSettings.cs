namespace Test_Taste_Console_Application.Constants
{
    public static class HttpClientSettings
    {
        public const string JsonType = "application/json";

        public const string BearerScheme = "Bearer";

        //The name of the setting that holds the API key. It is used both as the environment variable name and as the user secrets key.
        public const string ApiKeySettingName = "SOLAR_SYSTEM_API_KEY";
    }
}
