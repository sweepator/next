using Microsoft.AspNetCore.Mvc;
using Next.Cqrs.Commands;

namespace Next.Web.Application.Error
{
    public interface IProblemDetailsProfile
    {
        void Build(
            ProblemDetails problemDetails, 
            ICommandResponse commandResponse);
    }
}