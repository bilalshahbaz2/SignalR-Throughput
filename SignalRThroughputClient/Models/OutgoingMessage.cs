using System;
using System.Collections.Generic;

namespace SignalRThroughput.Models
{
    public class OutgoingMessage
    {
        public string Version { get; set; }
        public string Requestor { get; set; }
        public string Timestamp { get; set; }
        public string Datasource { get; set; }
        public string CorrelationId { get; set; }
        public string UserId { get; set; }
        public RequestType RequestType { get; set; }
        public ResponseBag ResponseBag { get; set; } = new ResponseBag();
        public String ConnectionId { get; set; }
        public int Key { get; set; }
        public long RecievedTime { get; set; }

    }

    public enum RequestType
    {
        Reference = 0,
        Subscription = 1,
        Search = 2,
        UnSubscribe = 3,
    }

    public class ResponseBag
    {
        public List<ResponseBagItem> Items { get; set; } = new List<ResponseBagItem>();
    }

    public class ResponseBagItem
    {

        public SecurityDefinition Security { get; set; }
        public String SequenceNo { get; set; } = "0";
        public Dictionary<string, FieldDescriptor> FieldValues { get; set; } = new Dictionary<string, FieldDescriptor>();
        public Dictionary<String, String> FieldErrors { get; set; } = new Dictionary<string, string>();

    }

    public class SecurityDefinition
    {
        public string SecurityIdentifier { get; set; }
        public string IdentifierType { get; set; }
        public string Message { get; set; } = "";
        public string LastUpdate { get; set; } = "";
    }

    public class FieldDescriptor
    {
        public String Key { get; set; } = "";
        public String Value { get; set; } = "";
        public bool HasError { get; set; } = false;
        public String Timestamp { get; set; } = "";
        public String OriginatingSource { get; set; } = "";
        public String Message { get; set; } = "";
        public String CollectorCode { get; set; } = "";
    }
}
