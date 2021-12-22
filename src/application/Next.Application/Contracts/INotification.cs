namespace Next.Application.Contracts
{
    public interface INotification<out TContent> : INotification
    {
        TContent Content { get; }
    }

    public interface INotification : MediatR.INotification
    {
    }
}