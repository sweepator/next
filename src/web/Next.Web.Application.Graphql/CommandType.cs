using HotChocolate.Types;
using Next.Cqrs.Commands;

namespace Next.Web.Application.Graphql
{
    public class CommandType<TCommand, TCommandResponse> : InputObjectType<TCommand>
        where TCommand: ICommand<TCommandResponse>
        where TCommandResponse: ICommandResponse
    {
        protected override void Configure(IInputObjectTypeDescriptor<TCommand> descriptor)
        {
            var commandName = typeof(TCommand).Name.ToLower();
                
            descriptor.BindFieldsImplicitly();
            descriptor.Name(commandName);
        }
    }
}