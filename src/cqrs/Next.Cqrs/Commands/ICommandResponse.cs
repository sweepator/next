using System.Collections.Generic;
using Next.Application.Contracts;
using Next.Core.Errors;

namespace Next.Cqrs.Commands
{
    public interface ICommandResponse: IResponse
    {
        bool IsSuccess{ get; }
        
        IEnumerable<Error> Errors { get; }

        void AddError(Error error);
        
        void AddErrors(IEnumerable<Error> errors);
    }
}
