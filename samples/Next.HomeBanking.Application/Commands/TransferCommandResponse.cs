using Next.Abstractions.Domain;
using Next.Cqrs.Commands;

namespace Next.HomeBanking.Application.Commands
{
    public class TransferCommandResponse : CommandResponse
    {
        public Id TransactionId { get; }
        public decimal? Balance { get; }
        
        public TransferCommandResponse()
        {
        }
        
        public TransferCommandResponse(
            Id transactionId,
            decimal balance)
        {
            TransactionId = transactionId;
            Balance = balance;
        }
    }
}