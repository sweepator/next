using System;
using Next.Abstractions.Domain;
using Next.Cqrs.Commands;

namespace Next.HomeBanking.Application.Commands
{
    public class DepositCommandResponse : CommandResponse
    {
        public Id TransactionId { get; }
        public decimal? Balance { get; }

        public DepositCommandResponse()
        {
        }
        
        public DepositCommandResponse(
            Id transactionId,
            decimal balance)
        {
            TransactionId = transactionId;
            Balance = balance;
        }
    }
}