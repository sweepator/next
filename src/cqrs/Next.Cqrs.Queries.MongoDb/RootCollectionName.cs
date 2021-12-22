using System;
using Next.Abstractions.Domain.ValueObjects;

namespace Next.Cqrs.Queries.MongoDb
{
    public class RootCollectionName : SingleValueObject<string>
    {
        public RootCollectionName(string value) : base(value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
        }
    }
}