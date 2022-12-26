using Microsoft.Bot.Connector.DirectLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.DirectlineAPI
{
    public class DirectlineConnector
    {
        // This gives a name to the bot Channel.
        private readonly string _channelId;
        private readonly DirectLineClient _client;
        private readonly Conversation _conversation;
        // track activities
        private string _watermark = null;
        public string ConversationId
        {
            get
            {
                return _conversation.ConversationId;
            }
        }

        public DirectlineConnector(DirectLineClient directLineClient, string channelId)
        {
            this._channelId = channelId;
            // Create a new Direct Line client.
            this._client = directLineClient;
            _client.HttpClient.Timeout = new TimeSpan(0, 3, 0);
            // Start the conversation.
            this._conversation = this._client.Conversations.StartConversationAsync().Result;
        }


        public async Task<string> SendText(string userId, string userName, string _text)
        {
            Activity tActivity = new Activity
            {
                From = new ChannelAccount(userId, userName),
                Text = _text,
                ChannelId = this._channelId,
                Type = ActivityTypes.Message,
            };
            var tResp = await _client.Conversations.PostActivityAsync(_conversation.ConversationId, tActivity);
            if (tResp == null)
            {
                throw new NotSupportedException();
            }
            return tResp.Id;
        }
        public async Task<string> SendAttachment(string userId, string _contentType, string _contentURL, object _content, string _name, string _thumbnailUrl)
        {
            ResourceResponse tResp = await _client.Conversations.UploadAsync(_conversation.ConversationId, new MemoryStream(_content as byte[]), userId, _contentType);
            if (tResp == null)
            {
                throw new NotSupportedException();
            }

            return tResp.Id;
        }
        public async Task<IEnumerable<Activity>> ReadBotMessages(string userId)
        {
            // Retrieve the activity set from the bot.
            ActivitySet activitySet = (await _client.Conversations.GetActivitiesAsync(_conversation.ConversationId, _watermark));
            _watermark = activitySet?.Watermark;

            // Extract the activies sent from our bot.
            IEnumerable<Activity> activities = from x in activitySet.Activities
                                               where x.From.Id != userId
                                               select x;

            foreach (var item in activities)
            {
                item.Recipient = new ChannelAccount(userId, null);
            }

            return activities;
        }
    }
}
