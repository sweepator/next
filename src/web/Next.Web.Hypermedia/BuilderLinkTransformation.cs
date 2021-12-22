using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Next.Web.Hypermedia
{
    public class BuilderLinkTransformation : ILinkTransformation
    {
        private readonly IList<Func<LinkTransformationContext, string>> _transforms;
        
        public BuilderLinkTransformation(IList<Func<LinkTransformationContext,string>> transforms)
        {
            _transforms = transforms;
        }

        public string Transform(LinkTransformationContext context)
        {
            var builder = _transforms.Aggregate(new StringBuilder(), (sb, transform) =>
            {
                sb.Append(transform(context));
                return sb;
            });
            return builder.ToString();
        }
    }
}