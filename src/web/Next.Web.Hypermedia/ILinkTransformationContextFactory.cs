namespace Next.Web.Hypermedia
{
    public interface ILinkTransformationContextFactory
    {
        LinkTransformationContext CreateContext(ILinkSpec spec);
    }
}