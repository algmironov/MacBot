namespace MacBot.ConsoleApp.Models
{
    internal class ObjectStorageServiceSettings(string? tenantId, string accessKey, string secretKey, string serviceUrl)
    {
        public string? TenantId { get; set; } = tenantId;
        public string AccessKey { get; set; } = accessKey;
        public string SecretKey { get; set; } = secretKey;
        public string ServiceUrl { get; set; } = serviceUrl;

        public ObjectStorageServiceSettings(string accessKey, string secretKey, string serviceUrl): this(tenantId: null, accessKey, secretKey, serviceUrl)
        {
            AccessKey = accessKey;
            SecretKey = secretKey;
            ServiceUrl = serviceUrl;
        }

    }



}
