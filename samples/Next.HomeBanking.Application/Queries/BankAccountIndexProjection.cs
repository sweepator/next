using Next.Abstractions.Domain;
using Next.HomeBanking.Domain.Aggregates;
using Next.HomeBanking.Domain.Events;
using Next.Cqrs.Queries.Projections;

namespace Next.HomeBanking.Application.Queries
{
    public class BankAccountIndexProjection: 
        IProjectionModel, 
        IProjectionModelFor<BankAccountAggregate, BankAccountId, BankAccountCreated>
    {
        public string Id { get; private set; }
        public string Iban { get; private set; }
        public int? Version { get; set; }

        public void Apply(
            IProjectionModelContext context, 
            IDomainEvent<BankAccountAggregate, BankAccountId, BankAccountCreated> domainEvent)
        {
            Id = domainEvent.AggregateIdentity.Value;
            Iban = domainEvent.AggregateEvent.Iban;
            Version = domainEvent.Version;
        }
    }
}