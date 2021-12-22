using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Next.Abstractions.Bus;
using Next.Abstractions.EventSourcing;
using Next.Application.Contracts;
using Next.Application.Pipelines;

namespace Next.Cqrs.Bus
{
    internal abstract class NotificationMessageHandlerBase<TMessage> : IMessageHandler
    {
        private readonly IServiceProvider _serviceProvider;

        protected NotificationMessageHandlerBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected abstract TMessage CreateMessage(
            IServiceScope scope,
            MessageContext messageContext);

        public async Task Process(MessageContext messageContext)
        {
            using var scope = _serviceProvider.CreateScope();
            var pipelineEngine = scope.ServiceProvider.GetRequiredService<IPipelineEngine>();
            var message = CreateMessage(
                scope,
                messageContext);
            var transactionId = Guid.Parse(messageContext.Message.Headers[MessageHeaders.TransactionId]);

            //TODO: reflection refactor
            var notification =
                (INotification)Activator.CreateInstance(typeof(Notification<>).MakeGenericType(message.GetType()), message);
            
            //var notification = new Notification<TMessage>(message);

            using var domainTransaction = new DomainTransaction(transactionId);
            await pipelineEngine.Publish(notification);
        }
    }
}