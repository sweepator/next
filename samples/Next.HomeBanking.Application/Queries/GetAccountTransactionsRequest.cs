using System;
using System.Linq.Expressions;
using Next.Cqrs.Queries;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Application.Queries
{
    public class GetAccountTransactionsRequest : QueryableRequest<BankAccountTransactionProjection>
    {
        public BankAccountId Id { get; set; }
        
        public decimal? Value { get; set; }

        public override Expression<Func<BankAccountTransactionProjection, bool>> GetQueryFilter()
        {
            Expression<Func<BankAccountTransactionProjection, bool>> expression = o =>
                o.AccountId.Equals(Id.Value);

            return expression;
        }
    }
}