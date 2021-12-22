using System;
using System.Linq.Expressions;
using Next.Cqrs.Queries;

namespace Next.HomeBanking.Application.Queries
{
    public class GetAccountsRequest : QueryableRequest<BankAccountProjection>
    {
        public bool? Enabled { get; set; }
        public string Owner { get; set; }

        public override IQueryPredicate Map()
        {
            var queryPredicate = new QueryPredicate();
            
            if (Enabled.HasValue)
            {
                queryPredicate.AddFilter(nameof(Enabled), Enabled.Value);
            }

            if (!string.IsNullOrWhiteSpace(Owner))
            {
                queryPredicate.AddFilter(nameof(Owner), Owner);
            }

            return queryPredicate;
        }

        public override Expression<Func<BankAccountProjection, bool>> GetQueryFilter()
        {
            var enabled = Enabled.GetValueOrDefault();
            var hasEnableFilter = Enabled.HasValue;
            var owner = string.IsNullOrEmpty(Owner) ? string.Empty : Owner;

            return o => 
                (!hasEnableFilter || (hasEnableFilter && o.Enabled.Equals(enabled))) &&
                (o.Owner.StartsWith(owner));
        }
    }
}