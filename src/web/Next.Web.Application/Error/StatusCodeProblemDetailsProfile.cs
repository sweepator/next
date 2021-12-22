using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Next.Cqrs.Commands;

namespace Next.Web.Application.Error
{
    public class StatusCodeProblemDetailsProfile : IProblemDetailsProfile
    {
        private readonly string _errorCode;
        private readonly int _statusCode;

        public StatusCodeProblemDetailsProfile(
            string errorCode,
            int statusCode)
        {
            _errorCode = errorCode;
            _statusCode = statusCode;
        }

        public void Build(
            ProblemDetails problemDetails, 
            ICommandResponse commandResponse)
        {
            var error = commandResponse.Errors.First();

            if (error.Code.Equals(_errorCode, StringComparison.InvariantCultureIgnoreCase))
            {
                problemDetails.Status = _statusCode;
            }
        }
    }
}