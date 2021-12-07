using SignalRThroughput.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRThroughput
{
    public static class MessageHelper
    {
        public static OutgoingMessage CreateOutgoing(int key)
        {
            var fields = new Dictionary<string, FieldDescriptor>();

            for(int i=1; i<=10; i++)
            {
                fields.Add($"Mnimonic-{i}", new FieldDescriptor()
                {
                    Value = $"StringString-{i}"
                });
            }


            var bagItem = new ResponseBagItem()
            {
                Security = new SecurityDefinition()
                {
                    SecurityIdentifier = $"SecurityIdentifier--{key}",
                    IdentifierType = $"IdentifierType--{key}"
                },
                FieldValues = fields,
                FieldErrors = null
            };

            var bag = new ResponseBag()
            {
                Items = new List<ResponseBagItem>() { bagItem }
            };


            return new OutgoingMessage()
            {
                Version = $"Version--{key}",
                Requestor = $"Requestor--{key}",
                Timestamp = $"TimeStamp--{key}",
                Datasource = $"Datasource--{key}",
                CorrelationId = $"CorrelationId--{key}",
                UserId = $"UserId--{key}",
                RequestType = RequestType.Subscription,
                ResponseBag = bag,
                ConnectionId = $"ConnectionId--{key}",
                Key = key
            };

            
        }
    }
}
