namespace Next.Application.Contracts
{
    public class Notification<TContent> : INotification<TContent>
    {
        public TContent Content { get; }

        public Notification(TContent content)
        {
            Content = content;
        }
    }
}