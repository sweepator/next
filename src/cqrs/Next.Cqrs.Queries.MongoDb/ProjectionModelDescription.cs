using System;
using System.Collections.Generic;
using Next.Abstractions.Domain.ValueObjects;

namespace Next.Cqrs.Queries.MongoDb
{
    public class ProjectionModelDescription : ValueObject
    {
        public ProjectionModelDescription(RootCollectionName rootCollectionName)
        {
            if (rootCollectionName == null) throw new ArgumentNullException(nameof(rootCollectionName));

            RootCollectionName = rootCollectionName;
        }

        public RootCollectionName RootCollectionName { get; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return RootCollectionName;
        }
    }
}