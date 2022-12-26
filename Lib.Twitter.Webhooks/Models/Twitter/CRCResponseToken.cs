using Newtonsoft.Json;

namespace Lib.Twitter.Webhooks.Models.Twitter
{
    internal class CRCResponseToken
    {
        [JsonProperty("response_token")]
        public string Token { get; set; }
    }
}
