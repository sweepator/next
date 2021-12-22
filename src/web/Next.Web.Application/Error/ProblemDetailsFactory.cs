using System;
using System.Collections.Generic;
using System.Linq;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Hosting;
using Next.Cqrs.Commands;

namespace Next.Web.Application.Error
{
    internal class ProblemDetailsFactory : IProblemDetailsFactory
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEnumerable<IProblemDetailsProfile> _profiles;
        private readonly IEnumerable<IProblemDetailsExceptionProfile> _exceptionProfiles;

        public ProblemDetailsFactory(
            IWebHostEnvironment webHostEnvironment,
            IEnumerable<IProblemDetailsProfile> profiles,
            IEnumerable<IProblemDetailsExceptionProfile> exceptionProfiles)
        {
            _webHostEnvironment = webHostEnvironment;
            _profiles = profiles;
            _exceptionProfiles = exceptionProfiles;
        }
        
        public ProblemDetails Create(ICommandResponse commandResponse)
        {
            var error = commandResponse.Errors.First();
            var errorTypeCode = error.Code.ToSnakeCaseAsSpan().Replace("_", "-").ToLower();
            
            // setup default problem details
            var problemDetails = new ProblemDetails()
            {
                Type = ErrorTypes.GetErrorType(errorTypeCode),
                Detail = error.Message,
                Title = error.Code,
                Status = StatusCodes.Status422UnprocessableEntity
            };

            if (error.Metadata != null)
            {
                foreach (var (key, value) in error.Metadata)
                {
                    problemDetails.Extensions.Add(key, value);
                }
            }

            // run problem details profiles
            foreach (var problemDetailsProfile in _profiles)
            {
                problemDetailsProfile.Build(
                    problemDetails,
                    commandResponse);
            }

            return problemDetails;
        }

        public ProblemDetails Create(Exception exception)
        {
            // setup default problem details
            var problemDetails = new StatusCodeProblemDetails(StatusCodes.Status500InternalServerError)
            {
                Type = ErrorTypes.ServiceUnavailable
            };

            if (_webHostEnvironment.IsDevelopment())
            {
                problemDetails.Detail = exception.ToString();
            }
            
            // run problem details exception profiles
            foreach (var exceptionProfile in _exceptionProfiles)
            {
                exceptionProfile.Build(
                    problemDetails,
                    exception);
            }

            return problemDetails;
        }

        public ProblemDetails Create(ModelStateDictionary modelState)
        {
            var invalidFieldName = GetFirstModelStateErrorField(modelState);

            if (!string.IsNullOrEmpty(invalidFieldName))
            {
                var problemDetails = new ValidationProblemDetails(modelState)
                {
                    Status = StatusCodes.Status400BadRequest, 
                    Type = ErrorTypes.InvalidField
                };
                problemDetails.Extensions.Add(
                    "fieldName",
                    invalidFieldName.ToCamelCase());
                return problemDetails;
            }

            return new ProblemDetails
            {
                Title = "Command request body is required.",
                Type = ErrorTypes.InvalidRequest,
                Status = StatusCodes.Status400BadRequest, 
            };
        }
        
        private static string GetFirstModelStateErrorField(ModelStateDictionary modelState)
        {
            if (modelState.Keys.Any())
            {
                foreach (var modelStateKey in modelState.Keys)
                {
                    var modelStateVal = modelState[modelStateKey];
                    if (modelStateVal.Errors.Any())
                    {
                        //format field name when we are handling with json paths
                        var index = modelStateKey.LastIndexOf(".", StringComparison.Ordinal);
                        return index >= 0 ? modelStateKey[(index + 1)..] : modelStateKey;
                    }
                }
            }

            return null;
        }
    }
}