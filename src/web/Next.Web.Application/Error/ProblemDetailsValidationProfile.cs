using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Next.Cqrs.Commands;

namespace Next.Web.Application.Error
{
    public class ProblemDetailsValidationProfile : IProblemDetailsProfile
    {
        public void Build(
            ProblemDetails problemDetails, 
            ICommandResponse commandResponse)
        {
            var validationErrors = commandResponse
                .Errors
                .Where(o =>
                    !string.IsNullOrEmpty(o.Category) &&
                    o.Category.Equals("Validation", StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            
            if (validationErrors.Any())
            {
                problemDetails.Type = ErrorTypes.ValidationError;
                problemDetails.Title = "Your request parameters didn't validate.";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Detail = null;
                problemDetails.Extensions.Clear();
                
                problemDetails.Extensions.Add( 
                    "invalid-params",
                    validationErrors.Select(o => new
                    {
                        name = o.Metadata.ContainsKey("PropertyName") ? o.Metadata["PropertyName"] : null,
                        reason = o.Message
                    }));
            }
        }
    }
}