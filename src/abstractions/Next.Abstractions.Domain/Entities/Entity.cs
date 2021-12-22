using System;
using System.Collections.Generic;
using Next.Abstractions.Domain.ValueObjects;

namespace Next.Abstractions.Domain.Entities
{
    public abstract class Entity<TIdentity> : ValueObject, IEntity<TIdentity>
        where TIdentity : IIdentity
    {
        protected Entity(TIdentity id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            Id = id;
        }

        public TIdentity Id { get; init; }
        
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}