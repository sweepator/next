using System.Collections.Generic;
using Next.Abstractions.Domain;
using Next.Cqrs.Queries.Projections;
using Next.HomeBanking.Domain.Aggregates;
using Next.HomeBanking.Domain.Events;

namespace Next.HomeBanking.Application.Queries
{
    public class BankAccountTransactionProjectionLocator : IProjectionModelLocator
    {
        public IEnumerable<object> GetProjectionModelIds(IDomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case IDomainEvent<BankAccountAggregate, BankAccountId, TransactionCreated> @event:
                    yield return @event.AggregateEvent.Id;
                    break;
                case IDomainEvent<BankAccountAggregate, BankAccountId, TransactionStarted>  @event:
                    yield return  @event.AggregateEvent.Id;
                    break;
                case IDomainEvent<BankAccountAggregate, BankAccountId, TransactionFinished>  @event:
                    yield return  @event.AggregateEvent.Id;
                    break;
                case IDomainEvent<BankAccountAggregate, BankAccountId, TransactionConfirmed>  @event:
                    yield return  @event.AggregateEvent.Id;
                    break;
                case IDomainEvent<BankAccountAggregate, BankAccountId, TransactionCancelled>  @event:
                    yield return  @event.AggregateEvent.Id;
                    break;
            }
        }
    }
}