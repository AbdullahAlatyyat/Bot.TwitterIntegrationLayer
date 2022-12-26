using Lib.DirectlineAPI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lib.Twitter.Adapter
{
    public class DirectLineRepo
    {
        Dictionary<string, DirectlineConnector> _repo = new Dictionary<string, DirectlineConnector>();

        private readonly string _secret;
        public DirectLineRepo(string clientSecret)
        {
            _secret = clientSecret;
        }

        public DirectlineConnector GetClientConversation(string id)
        {
            if (_repo.ContainsKey(id))
            {
                return _repo[id];
            }
            else
            {
                // Create New Clinet 
                var client = new Microsoft.Bot.Connector.DirectLine.DirectLineClient(_secret);
                var botConnector = new DirectlineConnector(client, "Twitter");
                _repo.TryAdd(id, botConnector);
                return botConnector;
            }
        }
    }
}
