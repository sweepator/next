using Microsoft.AspNetCore.Http;
using Next.Abstractions.Domain;
using Next.Application.Pipelines;

namespace Next.Web.Application.Extensions
{
    public static class HttpContextExtensions
    {
        public static TAggregateRoot GetAggregateRoot<TAggregateRoot>(this HttpContext httpContext)
            where TAggregateRoot: class, IAggregateRoot
        {
            var aggregateRoot = httpContext
                .Features
                .Get<IOperationContext>()
                .Features
                .Get<TAggregateRoot>();

            return aggregateRoot;
        }
    }
}