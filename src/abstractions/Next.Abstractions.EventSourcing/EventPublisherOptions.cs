namespace Next.Abstractions.EventSourcing
{
    public class EventPublisherOptions
    {
        public bool BackgroundProcessorEnabled { get; set; }
        
        public int BackgroundLockInSeconds { get; set; } = 10;
        
        public int BackgroundBatchSize { get; set; } = 100;

        public bool InlineProcessorEnabled { get; set; } = true;
    }
}