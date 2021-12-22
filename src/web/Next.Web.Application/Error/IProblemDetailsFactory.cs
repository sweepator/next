using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Next.Cqrs.Commands;

namespace Next.Web.Application.Error
{
    public interface IProblemDetailsFactory
    {
        ProblemDetails Create(ICommandResponse commandResponse);
        ProblemDetails Create(Exception exception);
        ProblemDetails Create(ModelStateDictionary modelState);
    }
}