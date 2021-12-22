using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Next.Web.Hypermedia
{
    public class LinksEvaluator : ILinksEvaluator
    {
        private readonly LinksOptions _options;
        private readonly ILinkTransformationContextFactory _contextFactory;
        public LinksEvaluator(
            IOptions<LinksOptions> options, 
            ILinkTransformationContextFactory contextFactory)
        {
            _options = options.Value;
            _contextFactory = contextFactory;
        }

        public void BuildLinks(
            IEnumerable<ILinkSpec> linkSpecs, 
            ILinkedResource resource)
        {
            foreach (var linkSpec in linkSpecs)
            {
                var context = _contextFactory.CreateContext(linkSpec);
                
                resource.AddLink(new Link(
                    linkSpec.Id,
                    _options.HrefTransformation.Transform(context),
                    linkSpec.HttpMethod.ToString(),
                    linkSpec.External ? true: null,
                    linkSpec.Parameters
                    ));
            }
        }
    }
}