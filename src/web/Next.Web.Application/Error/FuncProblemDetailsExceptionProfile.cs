using System;
using Microsoft.AspNetCore.Mvc;

namespace Next.Web.Application.Error
{
    internal class FuncProblemDetailsExceptionProfile<TException> : ProblemDetailsExceptionProfile<TException>
        where TException : Exception
    {
        private readonly Action<ProblemDetails, TException> _func;

        internal FuncProblemDetailsExceptionProfile(Action<ProblemDetails, TException> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }
        
        protected override void Build(
            ProblemDetails problemDetails, 
            TException exception)
        {
            _func(problemDetails, exception);
        }
    }
}