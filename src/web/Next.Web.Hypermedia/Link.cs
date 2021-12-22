using System;

namespace Next.Web.Hypermedia
{
    public class Link
    {
        public string Rel { get; }
        public string Href { get; }
        public string Method { get; }
        public bool? External { get; }
        public object Parameters { get; }

        public Link(
            string rel, 
            string href,
            string method,
            bool? external = null,
            object parameters = null)
        {
            Rel = rel ?? throw new ArgumentNullException(nameof(rel));
            Href = href ?? throw new ArgumentNullException(nameof(href));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            External = external;
            Parameters = parameters;
        }
    }
}