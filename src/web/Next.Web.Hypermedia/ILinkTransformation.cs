namespace Next.Web.Hypermedia
{
    public interface ILinkTransformation
    {
        string Transform(LinkTransformationContext context);
    }
}