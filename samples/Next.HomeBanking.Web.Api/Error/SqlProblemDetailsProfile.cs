using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Next.HomeBanking.Domain.Aggregates;
using Next.Cqrs.Commands;
using Next.Web.Application.Error;

namespace Next.HomeBanking.Web.Api.Error
{
    public class SqlProblemDetailsProfile: ProblemDetailsExceptionProfile<SqlException>
    {
        protected override void Build(
            ProblemDetails problemDetails, 
            SqlException exception)
        {
            if (exception.Number == 2601 &&
                exception.Message.Contains(
                    "Cannot insert duplicate key row in object 'dbo.Projection-BankAccountIndex'", 
                    StringComparison.InvariantCultureIgnoreCase))
            {
                var errorTypeCode = BankAccountAggregate.IbanAlreadyExists.Code.ToSnakeCase().Replace("_", "-").ToLower();
                problemDetails.Type = ErrorTypes.GetErrorType(errorTypeCode);
                problemDetails.Title = BankAccountAggregate.IbanAlreadyExists.Code;
                problemDetails.Status = StatusCodes.Status412PreconditionFailed;
            }
        }
    }
}