using System;
using Microsoft.AspNetCore.Mvc;

namespace Next.Web.Application.Error
{
    public interface IProblemDetailsExceptionProfile
    {
        void Build(
            ProblemDetails problemDetails, 
            Exception exception);
    }
}