using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Next.Cqrs.Commands;

namespace Next.Web.Application.Error
{
    public class ProblemDetailsProfileDefaultConventions : IProblemDetailsProfile
    {
        public void Build(
            ProblemDetails problemDetails, 
            ICommandResponse commandResponse)
        {
            var error = commandResponse.Errors.First();

            if (error.Code.Contains("notfound", StringComparison.InvariantCultureIgnoreCase))
            {
                problemDetails.Status = StatusCodes.Status404NotFound;
                return;
            }
            
            if (error.Code.Contains("invalid", StringComparison.InvariantCultureIgnoreCase)
                || error.Code.Contains("already", StringComparison.InvariantCultureIgnoreCase))
            {
                problemDetails.Status = StatusCodes.Status400BadRequest;
            }
        }
    }
}