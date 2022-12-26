using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.DirectLine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.Twitter.Adapter.Helpers
{
    public static class DataRepresentationHelper
    {
        public static List<Activity> handleActivities(List<Activity> activities)
        {
            if (activities != null && activities.Count > 0)
            {
                foreach (var act in activities)
                {
                    if (act.Type == ActivityTypes.Message)
                    {
                        if (string.IsNullOrEmpty(act.Text))
                        {
                            act.Text = null;
                        }
                        var tOutText = "";
                        List<CardAction> tOutActions = null;
                        List<Attachment> tAttachmentsToRemove = null;
                        if (handleActivity(act, out tOutText, out tOutActions, out tAttachmentsToRemove))
                        {
                            tAttachmentsToRemove.ForEach((att) =>
                            {
                                act.Attachments.Remove(att);
                            });

                            if (act.SuggestedActions != null && act.SuggestedActions.Actions != null && act.SuggestedActions.Actions.Count > 0)
                            {
                                tOutActions.AddRange(act.SuggestedActions.Actions);
                            }

                            try
                            {
                                tOutText = tOutText.Replace("**", "");
                            }
                            catch (Exception)
                            {
                            }

                            try
                            {
                                var regexPattern = @"(?<=\\d.)(\\s+\\n\\n)";
                                tOutText = Regex.Replace(tOutText, regexPattern, " ");
                            }
                            catch (Exception)
                            {
                            }

                            act.Text = tOutText;

                            act.SuggestedActions = new SuggestedActions()
                            {
                                Actions = tOutActions
                            };
                        }
                    }
                }
            }
            return activities;
        }


        private static bool handleActivity(Activity activity, out string text, out List<CardAction> cardActions, out List<Attachment> attachmentsToRemove)
        {
            text = string.IsNullOrEmpty(activity.Text) ? "" : activity.Text + "\r\n";
            cardActions = new List<CardAction>();
            attachmentsToRemove = new List<Attachment>();

            if (activity.Attachments != null && activity.Attachments.Count > 0)
            {
                foreach (var iAtt in activity.Attachments)
                {
                    bool isRemovable;
                    string tAttOutText;
                    List<CardAction> tAttOutActions;

                    if (handleAttachement(iAtt, out tAttOutText, out tAttOutActions, out isRemovable))
                    {
                        if (isRemovable)
                        {
                            attachmentsToRemove.Add(iAtt);
                        }
                        text += (string.IsNullOrWhiteSpace(text) ? "" : "\r\n") + tAttOutText;
                        if (tAttOutActions != null && tAttOutActions.Count > 0)
                        {
                            cardActions.AddRange(tAttOutActions);
                        }
                    }
                }
            }

            return true;
        }
        private static bool handleAttachement(Attachment attachment, out string text, out List<CardAction> cardActions, out bool removable)
        {
            removable = true;
            text = "";
            cardActions = new List<CardAction>();

            if (attachment.ContentType == "application/vnd.microsoft.card.adaptive")
            {
                if (attachment.Content.GetType() == typeof(Newtonsoft.Json.Linq.JObject))
                {
                    var tRet = false;
                    var tContentObj = (Newtonsoft.Json.Linq.JObject)attachment.Content;
                    var tBody = tContentObj["body"];
                    string tOutBodyText;
                    List<CardAction> tOutBodyActions;
                    if (handleAttachementBody(tBody, out tOutBodyText, out tOutBodyActions, out removable))
                    {
                        text += (string.IsNullOrWhiteSpace(text) ? "" : "\r\n") + tOutBodyText;
                        if (tOutBodyActions != null && tOutBodyActions.Count > 0)
                        {
                            cardActions.AddRange(tOutBodyActions);
                        }
                        tRet = true;
                    }

                    var tActionsObj = ((Newtonsoft.Json.Linq.JObject)attachment.Content)["actions"];
                    string tOutActionsText;
                    List<CardAction> tOutActionsActions;
                    if (handleAttachementActions(tActionsObj, out tOutActionsText, out tOutActionsActions))
                    {
                        text += (string.IsNullOrWhiteSpace(text) ? "" : "\r\n") + tOutActionsText;
                        if (tOutActionsActions != null && tOutActionsActions.Count > 0)
                        {
                            cardActions.AddRange(tOutActionsActions);
                        }
                        tRet = true;
                    }
                    return tRet;
                }
            }
            else if (attachment.ContentType == "application/pdf")
            {
                cardActions.Add(new CardAction()
                {
                    Type = ActionTypes.DownloadFile,
                    Title = attachment.ContentUrl as string,
                    Value = attachment.ContentUrl as string
                });
                text += (string.IsNullOrWhiteSpace(text) ? "" : "\r\n") + attachment.ContentUrl;
                return true;
            }

            return false;
        }
        private static bool handleAttachementBody(Newtonsoft.Json.Linq.JToken body, out string text, out List<CardAction> cardActions, out bool removable)
        {
            var tRet = false;
            text = "";
            removable = true;
            cardActions = new List<CardAction>();
            if (body != null)
            {
                foreach (var ibodyObj in body)
                {
                    if (ibodyObj.SelectToken("parseOnRepresentation") != null)
                    {
                        removable = (bool)((Newtonsoft.Json.Linq.JValue)ibodyObj["parseOnRepresentation"]).Value;
                    }
                    var type = ((Newtonsoft.Json.Linq.JValue)ibodyObj["type"]).Value as string;
                    string tOutText;
                    List<CardAction> tOutCardActions;
                    if (handleAttachmentBodyItem(ibodyObj, out tOutText, out tOutCardActions))
                    {
                        text += (string.IsNullOrWhiteSpace(text) ? "" : "\r\n") + tOutText;
                        if (tOutCardActions != null && tOutCardActions.Count > 0)
                        {
                            cardActions.AddRange(tOutCardActions);
                        }
                        tRet = true;
                    }
                }
            }

            return tRet;
        }

        private static bool handleAttachmentBodyItem(Newtonsoft.Json.Linq.JToken bodyItem, out string text, out List<CardAction> cardActions)
        {
            text = "";
            cardActions = new List<CardAction>();
            var type = (((Newtonsoft.Json.Linq.JValue)bodyItem["type"]).Value as string).ToLower();
            if (type == "textblock")
            {
                text += bodyItem["text"];
                return true;
            }
            if (type == "input.toggle")
            {
                var titleString = (string)bodyItem["title"];
                text += titleString;
                return true;
            }
            if (type == "input.choiceset")
            {
                var tJObj = (Newtonsoft.Json.Linq.JObject)bodyItem;
                if (tJObj["choices"] != null &&
                    tJObj["placeholder"] != null)
                {
                    text += (string.IsNullOrWhiteSpace(text) ? "" : "\r\n") + ((Newtonsoft.Json.Linq.JValue)tJObj["placeholder"]).Value as string;

                    var choices = tJObj["choices"];

                    foreach (var iChoice in choices)
                    {
                        cardActions.Add(new CardAction()
                        {
                            Type = ActionTypes.ImBack,
                            Title = ((Newtonsoft.Json.Linq.JValue)iChoice["title"]).Value as string,
                            Value = ((Newtonsoft.Json.Linq.JValue)iChoice["title"]).Value as string
                        });
                    }
                }
                return true;
            }
            else if (type == "container")
            {
                var items = bodyItem["items"];
                foreach (var iItem in items)
                {
                    string tOutText;
                    List<CardAction> tOutActions;
                    if (handleAttachmentBodyItem(iItem, out tOutText, out tOutActions))
                    {
                        text += (string.IsNullOrWhiteSpace(text) ? "" : "\r\n") + tOutText;
                        if (tOutActions != null && tOutActions.Count > 0)
                        {
                            cardActions.AddRange(tOutActions);
                        }
                    }
                }
                return true;
            }
            else if (type == "colum")
            {
                var items = bodyItem["columns"];
                foreach (var iItem in items)
                {
                    string tOutText;
                    List<CardAction> tOutActions;
                    if (handleAttachmentBodyItem(iItem, out tOutText, out tOutActions))
                    {
                        text += (string.IsNullOrWhiteSpace(text) ? "" : "\r\n") + tOutText;
                        if (tOutActions != null && tOutActions.Count > 0)
                        {
                            cardActions.AddRange(tOutActions);
                        }
                    }
                }
                return true;
            }
            else if (type == "columnset")
            {
                var items = bodyItem["columns"];
                foreach (var iItem in items)
                {
                    foreach (var columnItem in iItem["items"])
                    {
                        text += (string.IsNullOrWhiteSpace(text) ? "" : "\r\n") + columnItem["text"];
                    }
                }
                return true;
            }
            return false;
        }

        private static bool handleAttachementActions(Newtonsoft.Json.Linq.JToken actions, out string text, out List<CardAction> cardActions)
        {
            var tRet = false;
            text = "";
            cardActions = new List<CardAction>();

            if (actions != null)
            {
                foreach (var tObj in actions)
                {
                    var type = (((Newtonsoft.Json.Linq.JValue)tObj["type"]).Value as string).ToLower();
                    if (type == "action.submit")
                    {
                        if (tObj.GetType() == typeof(Newtonsoft.Json.Linq.JObject))
                        {
                            var tJObj = (Newtonsoft.Json.Linq.JObject)tObj;
                            if (tObj["data"] != null &&
                                tObj["title"] != null &&
                                tObj["id"] != null)
                            {
                                cardActions.Add(new CardAction()
                                {
                                    Type = ActionTypes.ImBack,
                                    Title = ((Newtonsoft.Json.Linq.JValue)tObj["title"]).Value as string,
                                    Value = ((Newtonsoft.Json.Linq.JValue)tObj["data"]).Value as string,
                                });
                            }
                        }
                        tRet = true;
                    }
                    else if (type == "action.toggle")
                    {
                        tRet = true;
                    }

                    else if (type == "action.openurl")
                    {

                        text = string.Concat(
                            text,
                            string.IsNullOrWhiteSpace(text) ? "" : "\r\n",
                            "[" + ((Newtonsoft.Json.Linq.JValue)tObj["title"])?.Value as string + "](" + ((Newtonsoft.Json.Linq.JValue)tObj["url"])?.Value as string + ")");
                        tRet = true;
                    }
                }
            }

            return tRet;
        }
    }
}
