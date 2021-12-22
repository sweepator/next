using Next.Abstractions.Domain;
using Next.Application.Contracts;
using Next.HomeBanking.Application.Commands;
using Next.HomeBanking.Domain.Aggregates;
using Next.HomeBanking.Domain.Events;

namespace Next.HomeBanking.Web.Api.Mapping
{
    internal static class MappingExtensions
    {
        internal static ProcessAccountCommand ToProcessProductCommand(this Notification<DomainEvent<BankAccountAggregate, BankAccountId, BankAccountCreated>> notification)
        {
            return new()
            {
                Id = notification.Content.AggregateIdentity
            };
        }
    }
}