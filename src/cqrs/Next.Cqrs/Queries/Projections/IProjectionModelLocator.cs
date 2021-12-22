using System.Collections.Generic;
using Next.Abstractions.Domain;

namespace Next.Cqrs.Queries.Projections
{
    public interface IProjectionModelLocator
    {
        IEnumerable<object> GetProjectionModelIds(IDomainEvent domainEvent);
    }
}