using System;
using Microsoft.AspNetCore.Mvc;

namespace Next.Web.Application.Error
{
    public abstract class ProblemDetailsExceptionProfile<TException>: IProblemDetailsExceptionProfile
        where TException: Exception
    {
        protected abstract void Build(
            ProblemDetails problemDetails,
            TException exception);
        
        public void Build(
            ProblemDetails problemDetails, 
            Exception exception)
        {
            if (exception is TException ex)
            {
                Build(
                    problemDetails,
                    ex);
            }
        }
    }
}