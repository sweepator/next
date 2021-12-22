using Next.Abstractions.Serialization.Metadata;
using Next.HomeBanking.Application.Commands;

namespace Next.HomeBanking.Application.Serialization
{
    public class CreateAccountRequestProfiler : SerializerMetadataProfile<CreateAccountCommand>
    {
        public CreateAccountRequestProfiler()
        {
            Replace(p => p.Iban, 2);
        }
    }
}