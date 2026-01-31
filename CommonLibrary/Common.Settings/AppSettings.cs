namespace Common.Settings
{
    public class AppSettings
    {
        public string SmsToken { get; set; }
        public string SmsServiceUrl { get; set; }
        public string StorageConnection { get; set; }
        public string StorageContainerName { get; set; }
        public string GoogleClientId { get; set; }
        public string GoogleClientSecret { get; set; }
        public bool EnableMicrosoftCheckJwt { get; set; }

        public string? AzureBlobContainerName { get; set; }
        public string? AzureBlobConnection { get; set; }
        public string? AzureBlobRootFolder { get; set; }
        public string? AzureBlobUrl { get; set; }
        public string? ONESIGNAL_APP_ID { get; set; }
        public string? ONESIGNAL_API_KEY { get; set; }
    }

    public class StrJWT
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
    }

    public class ConnectionStrings
    {
        public string WebApiDatabase { get; set; }
    }

    public class ApplicationInsights
    {
        public string ConnectionString { get; set; }
    }
}