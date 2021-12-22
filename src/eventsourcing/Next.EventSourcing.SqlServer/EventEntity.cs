using System;
using Next.Abstractions.EventSourcing;

namespace Next.EventSourcing.SqlServer
{
    internal class EventEntity
    {
        public string aggregate_id { get; set; }
        public string aggregate_name { get; set; }
        public string name { get; set; }
        public int version { get; set; }
        public DateTime timestamp { get; set; }
        public string data { get; set; }
        public string metadata { get; set; }
        public bool committed { get; set; }
        public Guid transaction_id { get; set; }
        public int total_events { get; set; }
    }
}