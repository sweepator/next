using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using Next.Abstractions.Validation;
using Next.Application.Pipelines;
using Next.Cqrs.Commands;

namespace Next.Application.Validation
{
    public class ValidateStep<TRequest, TResponse> : PipelineStep<TRequest, TResponse>
        where TRequest : ICommand<TResponse>
        where TResponse : ICommandResponse, new()
    {
        private readonly IValidatorFactory _validatorFactory;
        private readonly ValidationStepOptions _validationOptions;

        public ValidateStep(
            IValidatorFactory validatorFactory,
            IOptionsSnapshot<ValidationStepOptions> validationOptions)
        {
            _validatorFactory = validatorFactory;
            _validationOptions = validationOptions.Get(typeof(TRequest).Name);
        }

        public override async Task<TResponse> Execute(
            TRequest request, 
            IOperationContext context, 
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken = default)
        {
            if (!_validationOptions.IsEnabled)
            {
                return await next();
            }
            
            var validators = _validatorFactory.GetValidators(request);

            if (validators == null)
            {
                return await next();
            }
            
            foreach (var validator in validators)
            {
                var validationResult = validator.Validate(request);
                
                if (!validationResult.IsValid)
                {
                    var response = new TResponse();
                    response.AddErrors(validationResult.ValidationErrors);
                    return response;
                }
            }

            return await next();
        }
    }
}