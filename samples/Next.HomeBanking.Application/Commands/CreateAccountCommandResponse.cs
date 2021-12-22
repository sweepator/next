using Next.HomeBanking.Domain.Aggregates;
using Next.Cqrs.Commands;

namespace Next.HomeBanking.Application.Commands
{
    public class CreateAccountCommandResponse : CommandResponse
    {
        public BankAccountId Id { get; }
        
        public CreateAccountCommandResponse()
        {
        }
        
        public CreateAccountCommandResponse(BankAccountId id)
        {
            Id = id;
        }
    }
}