using Next.HomeBanking.Domain.Aggregates;
using Next.Cqrs.Commands;

namespace Next.HomeBanking.Application.Commands
{
    public class CreateAccountCommand : CreateAggregateCommand<BankAccountAggregate,BankAccountId, CreateAccountCommandResponse>
    {
        public string Owner { get; set; }
        public string Iban { get; set; }

        public override BankAccountId GetIdentity() => BankAccountId.New;
    }
}