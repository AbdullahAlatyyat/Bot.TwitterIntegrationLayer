using Lib.Twitter.Webhooks.Authentication;
using Lib.Twitter.Webhooks.Models;
using Lib.Twitter.Webhooks.Services;
using Lib.Twitter.Adapter;
using Lib.Twitter.Adapter.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IMiddleware = Microsoft.AspNetCore.Http.IMiddleware;

namespace Lib.Twitter.Adapter.Hosting
{
    public class WebhookMiddleware : IMiddleware
    {
        private readonly TwitterAdapter _adapter;
        private readonly TwitterOptions _options;
        private readonly WebhookInterceptor _interceptor;
        DirectLineRepo _directLineRepo;

        public WebhookMiddleware(IOptions<TwitterOptions> options,
            TwitterAdapter adapter, DirectLineRepo directLineRepo)
        {
            _directLineRepo = directLineRepo;
            _adapter = adapter;
            _options = options.Value;
            _interceptor = new WebhookInterceptor(_options.ConsumerSecret);
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var result = await _interceptor.InterceptIncomingRequest(context.Request, OnDirectMessageReceived);
            if (result.IsHandled)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync(result.Response);
            }
            else
            {
                await next(context);
            }
        }

        private async void OnDirectMessageReceived(DirectMessageEvent obj)
        {
            // Important: Ignore messages originating from the bot else recursion happens
            if (obj.Sender.ScreenName == _options.BotUsername) return;

            if (_options.AllowedUsernamesConfigured()
                && _options.AllowedUsernames.Contains(obj.Sender.ScreenName)
                || !_options.AllowedUsernamesConfigured())
            {
                // Only respond if sender is different than the bot
                // Get the user connection 
                var userId = obj.Sender.Id;
                var directlineConnector = _directLineRepo.GetClientConversation(userId);
                // Send Activity and wait messages
                bool isUri = Uri.IsWellFormedUriString(obj.MessageText, UriKind.Absolute);
                if (isUri)
                {
                    object tContent;
                    string tContentType;
                    using (var client = new WebClient())
                    {
                        var success = DownloadAttachment(obj.Event.message_create.message_data.attachment.media.media_url, out tContent, out tContentType);
                    }
                    await directlineConnector.SendAttachment(userId, tContentType, obj.MessageText, tContent, obj.MessageText, obj.MessageText);
                }
                else
                {
                    await directlineConnector.SendText(userId, obj.Sender.Name, obj.MessageText);
                }
                var activites = await directlineConnector.ReadBotMessages(userId);
                activites = DataRepresentationHelper.handleActivities(activites.ToList());
                await _adapter.SendActivitiesAsync(activites.ToArray());
            }
        }

        public bool DownloadAttachment(string _url, out object _content, out string _contentType)
        {
            _content = new object();
            _contentType = "";
            try
            {
                using (var http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Add("Authorization", AuthHeaderBuilder.Build(_options, HttpMethod.Get, _url));
                    var file_content = http.GetAsync(_url).Result;
                    _content = file_content.Content.ReadAsByteArrayAsync().Result;
                    _contentType = file_content.Content.Headers.ContentType.MediaType;
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}