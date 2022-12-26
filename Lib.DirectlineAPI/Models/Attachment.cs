using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.DirectlineAPI.Models
{
    public class Attachment
    {
        public Attachment(string _contentType, string _contentUrl, AttachmentContent _content, string _name, string _thumbnailUrl)
        {
            ContentType = _contentType;
            ContentUrl = _contentUrl;
            Content = _content;
            Name = _name;
            ThumbnailUrl = _thumbnailUrl;
        }

        [JsonProperty(PropertyName = "contentType")]
        public string ContentType { get; set; }
        [JsonProperty(PropertyName = "contentUrl")]
        public string ContentUrl { get; set; }
        [JsonProperty(PropertyName = "content")]
        public AttachmentContent Content { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "thumbnailUrl")]
        public string ThumbnailUrl { get; set; }
    }
}
