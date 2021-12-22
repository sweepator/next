using System;
using System.Collections.Generic;
using Next.Abstractions.Core.Versioning;

namespace Next.Abstractions.Domain
{
    public class AggregateEventDefinitionService :
        VersionedTypeDefinitionService<IAggregateEvent, AggregateEventVersionAttribute, AggregateEventDefinition>,
        IAggregateEventDefinitionService
    {
        public AggregateEventDefinitionService(IEnumerable<Type> loadedVersionedTypes)
        {
            Load(loadedVersionedTypes);
        }

        protected override AggregateEventDefinition CreateDefinition(
            int version, 
            Type type, 
            string name)
        {
            return new(version, type, name);
        }
    }
}