namespace MacBot.ConsoleApp.Models
{
    internal class ObjectStorageServiceSettings
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string ServiceUrl { get; set; }

        public ObjectStorageServiceSettings(string accessKey, string secretKey, string serviceUrl)
        {
            AccessKey = accessKey;
            SecretKey = secretKey;
            ServiceUrl = serviceUrl;
        }
    }
}
