using Next.Abstractions.Serialization.Metadata;
using Next.Cqrs.Commands;

namespace Next.Cqrs.Serialization
{
    internal class IgnoreBaseCommandResponseProperties : SerializerMetadataProfile<ICommandResponse>
    {
        public IgnoreBaseCommandResponseProperties()
        {
            IgnoreProperty(o => o.Errors)
                .IgnoreProperty(o => o.IsSuccess);
        }
    }
}