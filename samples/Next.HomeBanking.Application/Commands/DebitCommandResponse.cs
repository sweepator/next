using System;
using Next.Abstractions.Domain;
using Next.Cqrs.Commands;

namespace Next.HomeBanking.Application.Commands
{
    public class DebitCommandResponse : CommandResponse
    {
        public Id TransactionId { get; }
        public decimal? Balance { get; }
        
        public DebitCommandResponse()
        {
        }
        
        public DebitCommandResponse(
            Id transactionId,
            decimal balance)
        {
            TransactionId = transactionId;
            Balance = balance;
        }
    }
}