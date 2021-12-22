namespace Next.Cqrs.Bus
{
    public static class MessageHeaders
    {
        public const string AggregateId = "next.aggregateId";
        public const string AggregateType = "next.aggregateType";
        public const string AggregateEvent = "next.aggregateEvent";
        public const string TransactionId = "next.transactionId";
        public const string EventVersion = "next.eventVersion";
        public const string EventSchemaVersion = "next.eventSchemaVersion";
        public const string DateUtc = "next.dateUtc";
    }
}