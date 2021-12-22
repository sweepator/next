using System;
using System.Linq.Expressions;
using Next.Cqrs.Queries;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Application.Queries
{
    public class GetAccountDetailsRequest : QueryableRequest<BankAccountDetailsProjection>
    {
        public BankAccountId Id { get; set; }

        public override Expression<Func<BankAccountDetailsProjection, bool>> GetQueryFilter()
        {
            Expression<Func<BankAccountDetailsProjection, bool>> expression = o =>
                o.Id.Equals(Id.Value);

            return expression;
        }
    }
}