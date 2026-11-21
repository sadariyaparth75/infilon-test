namespace Test_Taste_Console_Application.Constants
{
    public static class ExceptionMessage
    {
        public const string LoadingDataFailed = "Loading the data failed with the following error: ";

        public const string FileOutputOperationFailed = "The file output operation failed with the following error: ";

        public const string ScreenOutputOperationFailed =
            "The screen output operation failed with the following error: ";

        public const string ApiKeyRejected =
            "The API rejected the API key (HTTP 401/403). Check the value of " +
            HttpClientSettings.ApiKeySettingName + ".";

        public const string ApiRequestFailed = "The API request failed with status code ";

        public const string ApiUnreachable = "The API could not be reached: ";

        public const string ApiKeyMissing =
            "No API key found. Copy appsettings.example.json to appsettings.json and fill in " +
            HttpClientSettings.ApiKeySettingName + ", or use user secrets / an environment variable (see README.md).";
    }
}
