using Next.Core.Errors;
using System.Collections.Generic;
using System.Linq;

namespace Next.Cqrs.Commands
{
    public class CommandResponse : ICommandResponse
    {
        private List<Error> _errors;

        private static readonly CommandResponse BaseSuccess = new CommandResponse();

        public bool IsSuccess => _errors == null || !_errors.Any();

        public IEnumerable<Error> Errors => _errors;

        public void AddError(Error error)
        {
            _errors ??= new List<Error>();
            _errors.Add(error);
        }

        public void AddErrors(IEnumerable<Error> errors)
        {
            _errors ??= new List<Error>();
            _errors.AddRange(errors);
        }
        
        public static TResponse Success<TResponse>()
            where TResponse : CommandResponse, new()
        {
            return new TResponse();
        }
        
        public static CommandResponse Success()
        {
            return BaseSuccess;
        }

        public static TResponse Fail<TResponse>(string errorCode)
            where TResponse : ICommandResponse, new()
        {
            var response = new TResponse();
            response.AddError(new Error(errorCode));
            return response;
        }

        public static TResponse Fail<TResponse>(Error error)
            where TResponse : ICommandResponse, new()
        {
            var response = new TResponse();
            response.AddError(error);
            return response;
        }
        
        public static CommandResponse Fail(Error error)
        {
            var response = new CommandResponse();
            response.AddError(error);
            return response;
        }

        public static CommandResponse Error(Error error)
        {
            var response = new CommandResponse();
            response.AddError(error);
            return response;
        }
    }

    public abstract class CommandResponse<T> : CommandResponse
    {
        public T Content { get; }

        public CommandResponse(T content)
        {
            this.Content = content;
        }
    }
}
