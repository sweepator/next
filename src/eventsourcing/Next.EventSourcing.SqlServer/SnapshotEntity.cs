using System;

namespace Next.EventSourcing.SqlServer
{
    internal class SnapshotEntity
    {
        public string aggregate_id { get; set; }
        public string aggregate_name { get; set; }
        public int aggregate_version { get; set; }
        public string name { get; set; }
        public DateTime timestamp { get; set; }
        public string data { get; set; }
        public string metadata { get; set; }
    }
}