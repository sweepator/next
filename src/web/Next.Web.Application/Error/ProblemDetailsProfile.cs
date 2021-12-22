using Microsoft.AspNetCore.Mvc;
using Next.Cqrs.Commands;

namespace Next.Web.Application.Error
{
    public abstract class ProblemDetailsProfile<TCommandResponse>: IProblemDetailsProfile
        where TCommandResponse: ICommandResponse
    {
        protected abstract void Build(
            ProblemDetails problemDetails,
            TCommandResponse commandResponse);
        
        public void Build(
            ProblemDetails problemDetails, 
            ICommandResponse commandResponse)
        {
            if (commandResponse is TCommandResponse response)
            {
                Build(
                    problemDetails,
                    response);
            }
        }
    }
}