using System;
using System.Threading.Tasks;
using MassTransit;
using Next.HomeBanking.Domain.Events;
using Next.Cqrs.MassTransit;

namespace Next.HomeBanking.Messaging
{
    public class BankAccountCreatedConsumer : IAggregateEventConsumer<BankAccountCreated>
    {
        public Task Consume(ConsumeContext<BankAccountCreated> context)
        {
            return Task.CompletedTask;
        }
    }
}