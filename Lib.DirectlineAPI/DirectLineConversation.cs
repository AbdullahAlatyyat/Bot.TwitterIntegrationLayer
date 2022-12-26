using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lib.DirectlineAPI.Models;
using Newtonsoft.Json.Linq;

namespace Lib.DirectlineAPI
{
    public delegate void MessageRecievedHandler(string _message, List<Attachment> _attachments, string _userId, string _conversationId);
    public class DirectLineConversation
    {
        private readonly DirectlineConnector _directlineClient;
        private readonly string _userId;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        public event MessageRecievedHandler OnMessageReceived;

        public DirectLineConversation(DirectlineConnector directlineClient, string userId)
        {
            this._directlineClient = directlineClient;
            this._userId = userId;
            StartPullingMessages(_cts.Token);
        }

        public string SendText(string _text)
        {
            try
            {
                return _directlineClient.SendText(this._userId, this._userId, _text).Result;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public string SendAttachment(string _contentType, string _contentURL, object _content, string _name, string _thumbnailUrl)
        {
            try
            {
                return _directlineClient.SendAttachment(this._userId, _contentType, _contentURL, _content, _name, _thumbnailUrl).Result;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public void StopConversation()
        {
            this._cts.Cancel();
        }
        private void StartPullingMessages(CancellationToken cancellationToken)
        {
            var readerTask = new Task(async () =>
            {
                do
                {
                    var tActivities = await _directlineClient.ReadBotMessages(this._userId);
                    foreach (var activity in tActivities)
                    {
                        OnMessageReceived(activity.Text, parseAttachements(activity.Attachments), this._userId, this._directlineClient.ConversationId);
                    }
                } while (!cancellationToken.IsCancellationRequested);

            });
            readerTask.Start();
            readerTask.Wait();
        }

        private List<Attachment> parseAttachements(IList<Microsoft.Bot.Connector.DirectLine.Attachment> _attachments)
        {
            List<Attachment> tRetAttachements = new List<Attachment>();
            if (_attachments != null)
            {
                foreach (var iAttach in _attachments)
                {
                    var tAttach = new Attachment(
                        iAttach.ContentType,
                        iAttach.ContentUrl,
                        ((JObject)iAttach.Content).ToObject<AttachmentContent>(),
                        iAttach.Name,
                        iAttach.ThumbnailUrl);
                    tRetAttachements.Add(tAttach);
                }
            }
            return tRetAttachements;
        }
    }
}
