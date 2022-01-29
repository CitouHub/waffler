namespace Waffler.Service.Infrastructure
{
    public interface IConfigCache
    {
        string GetApiKey();
        void SetApiKey(string apiKey);
    }

    public class ConfigCache : IConfigCache
    {
        private string ApiKey { get; set; }

        public string GetApiKey()
        {
            return ApiKey;
        }

        public void SetApiKey(string apiKey)
        {
            ApiKey = apiKey;
        }
    }
}