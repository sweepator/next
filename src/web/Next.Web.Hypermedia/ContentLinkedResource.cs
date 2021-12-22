namespace Next.Web.Hypermedia
{
    public class ContentLinkedResource : LinkedResource, IContentLinkedResource
    {
        public object Content { get; }
        
        public ContentLinkedResource(object content)
        {
            Content = content;
        }
        
        public override object GetResource()
        {
            return Content;
        }
    }

    public class ContentLinkedResource<TContent> : ContentLinkedResource, IContentLinkedResource<TContent>
    {
        public new TContent Content => (TContent)base.Content;
        
        public ContentLinkedResource(TContent content) 
            : base(content)
        {
        }
    }
}