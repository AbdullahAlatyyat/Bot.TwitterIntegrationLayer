using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lib.Twitter.Webhooks.Models.Twitter
{
    public class WebhookResult
    {
        [JsonProperty("environments")] public IList<EnvironmentRegistration> Environments { get; set; }
    }
}