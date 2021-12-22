using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Next.Abstractions.Domain;
using Next.Cqrs.Commands;
using Next.Cqrs.Queries;
using Next.Cqrs.Queries.Projections;
using Next.Web.Application.Error;

namespace Next.Web.Application.PortAdapters
{
    public class HttpPortAdapter: IHttpPortAdapter
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICommandProcessor _commandProcessor;
        private readonly ICommandBus _commandBus;
        private readonly IQueryProcessor _queryProcessor;
        private readonly IProblemDetailsFactory _problemDetailsFactory;

        public HttpPortAdapter(
            IActionContextAccessor actionContextAccessor,
            ICommandProcessor commandProcessor,
            ICommandBus commandBus,
            IQueryProcessor queryProcessor,
            IProblemDetailsFactory problemDetailsFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _commandProcessor = commandProcessor;
            _commandBus = commandBus;
            _queryProcessor = queryProcessor;
            _problemDetailsFactory = problemDetailsFactory;
        }
        
        public async Task<IActionResult> Execute<TCommandResponse>(
            ICommand<TCommandResponse>   command, 
            CancellationToken cancellationToken = default)
            where TCommandResponse : ICommandResponse
        {
            var actionResult = ValidateModelState();
            if (actionResult != null)
            {
                return actionResult;
            }
            
            var response = await _commandProcessor.Execute(
                command, 
                cancellationToken);

            if (!response.IsSuccess)
            {
                return ProcessProblemDetails(response);
            }
            
            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<IActionResult> Send<TCommandResponse>(
            ICommand<TCommandResponse> command, 
            CancellationToken cancellationToken = default) 
            where TCommandResponse : ICommandResponse
        {
            var actionResult = ValidateModelState();
            if (actionResult != null)
            {
                return actionResult;
            }
            
            var transactionId = await _commandBus.Send(
                command,
                cancellationToken);

            return new ObjectResult(new
            {
                TransactionId = transactionId
            })
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<IActionResult> Execute<TProjectionModel>(
            IQueryRequest<TProjectionModel> queryRequest,
            CancellationToken cancellationToken = default)
            where TProjectionModel: IProjectionModel
        {
            var response = await _queryProcessor.Execute(
                queryRequest, 
                cancellationToken);
            
            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status200OK
            };
        }
        
        public async Task<IActionResult> ExecuteSingle<TProjectionModel>(
            IQueryRequest<TProjectionModel> queryRequest,
            CancellationToken cancellationToken = default)
            where TProjectionModel: IProjectionModel
        {
            var response = await _queryProcessor.Execute(
                queryRequest, 
                cancellationToken);
            
            var result = response.FirstOrDefault();

            if (result != null)
            {
                return new ObjectResult(result)
                {
                    StatusCode = StatusCodes.Status200OK
                };
            }

            return GetNotFoundResult();
        }
        
        private IActionResult ProcessProblemDetails(ICommandResponse response)
        {
            var problemDetails = _problemDetailsFactory.Create(response);
            return new ObjectResult(problemDetails);
        }
        
        private static IActionResult GetNotFoundResult()
        {
            var errorTypeCode = DomainErrors.NotFound.Code.ToSnakeCase().Replace("_", "-").ToLower();
            return new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = ErrorTypes.GetErrorType(errorTypeCode),
                Title = DomainErrors.NotFound.Code,
                Detail = DomainErrors.NotFound.Message
            });
        }
        
        private IActionResult ValidateModelState()
        {
            var modelState = _actionContextAccessor.ActionContext.ModelState;
            if (!modelState.IsValid)
            {
                var problemDetails = _problemDetailsFactory.Create(modelState);
                return new ObjectResult(problemDetails);
            }

            return null;
        }
    }
}