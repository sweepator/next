using System;
using Microsoft.AspNetCore.Mvc;
using Next.Cqrs.Commands;

namespace Next.Web.Application.Error
{
    internal class FuncProblemDetailsProfile<TCommandResponse> : ProblemDetailsProfile<TCommandResponse>
        where TCommandResponse : ICommandResponse
    {
        private readonly Action<ProblemDetails, TCommandResponse> _func;

        internal FuncProblemDetailsProfile(Action<ProblemDetails, TCommandResponse> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }
        
        protected override void Build(
            ProblemDetails problemDetails, 
            TCommandResponse exception)
        {
            _func(problemDetails, exception);
        }
    }
}