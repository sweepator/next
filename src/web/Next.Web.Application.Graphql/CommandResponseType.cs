using System;
using HotChocolate.Types;
using Next.Cqrs.Commands;

namespace Next.Web.Application.Graphql
{
    public class CommandResponseType<TCommandResponse> : ObjectType<TCommandResponse>
        where TCommandResponse: ICommandResponse
    {
        protected override void Configure(IObjectTypeDescriptor<TCommandResponse> descriptor)
        {
            var commandResponseName = typeof(TCommandResponse).Name.ToLower();
                
            descriptor.BindFieldsImplicitly();
            descriptor.Name(commandResponseName);
        }
    }
}