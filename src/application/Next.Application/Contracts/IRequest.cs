namespace Next.Application.Contracts
{
    public interface IRequest: MediatR.IRequest<MediatR.Unit>
    {
    }

    public interface IRequest<out TResponse> : MediatR.IRequest<TResponse>
        where TResponse : IResponse
    {
    }
}
