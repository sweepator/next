using Next.Abstractions.Domain;

namespace Next.HomeBanking.Domain.Aggregates
{
    public class BankAccountId : Identity<BankAccountId>
    {
        public BankAccountId(string value) 
            : base(value)
        {
        }
    }
}