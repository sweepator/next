using Next.Abstractions.Serialization.Metadata;
using Next.Cqrs.Commands;

namespace Next.Cqrs.Serialization
{
    internal class IgnoreBaseCommandRequestProperties : SerializerMetadataProfile<IAggregateCommand>
    {
        public IgnoreBaseCommandRequestProperties()
        {
            IgnoreProperty(o => o.Id);
        }
    }
}