using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lib.Twitter.Webhooks.Authentication;
using Lib.Twitter.Webhooks.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Extensions.Options;

namespace Lib.Twitter.Adapter
{
    public class TwitterAdapter
    {
        private DirectMessageSender _sender;

        public TwitterAdapter(IOptions<TwitterOptions> options)
        {
            _sender = new DirectMessageSender(options.Value);
        }

        public async Task<ResourceResponse[]> SendActivitiesAsync(Activity[] activities)
        {
            var responses = new List<ResourceResponse>();
            foreach (var activity in activities)
            {
                await _sender.SendAsync(long.Parse(activity.Recipient.Id), activity.Text,
                    activity.SuggestedActions?.Actions?.Select(x => x.Title).ToList());
                responses.Add(new ResourceResponse(activity.Id));
            }

            return responses.ToArray();
        }
    }
}
