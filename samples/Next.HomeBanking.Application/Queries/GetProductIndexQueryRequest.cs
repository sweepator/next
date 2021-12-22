using Next.Cqrs.Queries;

namespace Next.HomeBanking.Application.Queries
{
    /*public class GetProductIndexQueryRequest : QueryRequest<BankAccountIndexProjection>
    {
        public string Name { get; set; }

        public override IQueryPredicate Map()
        {
            var queryPredicate = new QueryPredicate();

            if (!string.IsNullOrWhiteSpace(Name))
            {
                queryPredicate.AddFilter(nameof(Name), Name);
            }

            return queryPredicate;
        }
    }*/
}