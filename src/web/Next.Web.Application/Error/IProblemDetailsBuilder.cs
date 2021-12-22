using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Next.Cqrs.Commands;

namespace Next.Web.Application.Error
{
    public interface IProblemDetailsBuilder
    {
        IProblemDetailsBuilder AddProfiles(Assembly assembly);
        
        IProblemDetailsBuilder AddProfile<TProfile>()
            where TProfile : class, IProblemDetailsProfile;
        
        IProblemDetailsBuilder AddExceptionProfile<TExceptionProfile>()
            where TExceptionProfile : class, IProblemDetailsExceptionProfile;
        
        IProblemDetailsBuilder MapError<TException>(Action<ProblemDetails, TException> func)
            where TException : Exception;
        
        IProblemDetailsBuilder Map<TCommandResponse>(Action<ProblemDetails, TCommandResponse> func)
            where TCommandResponse : ICommandResponse;

        IProblemDetailsBuilder Map(
            string errorCode,
            int statusCode);
    }
}