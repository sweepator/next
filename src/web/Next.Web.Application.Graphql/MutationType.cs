using System.Linq;
using System.Reflection;
using HotChocolate.Types;
using Next.Cqrs.Commands;
using Next.Cqrs.Configuration;

namespace Next.Web.Application.Graphql
{
    public class MutationType: ObjectType
    {
        private readonly IDomainMetadataInfo _domainMetadataInfo;
            
        private static readonly MethodInfo AddCommandDescriptorMethod =
            typeof(MutationType)
                .GetTypeInfo()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(AddCommandDescriptor) && m.GetGenericArguments().Length == 2);

        public MutationType(IDomainMetadataInfo domainMetadataInfo)
        {
            _domainMetadataInfo = domainMetadataInfo;
        }

        private void AddCommandDescriptor<TCommand, TCommandResponse>(IObjectTypeDescriptor descriptor)
            where TCommand:ICommand<TCommandResponse>
            where TCommandResponse: ICommandResponse
        {
            var commandName = typeof(TCommand).Name.ToLower();

            descriptor
                .Field(commandName)
                .Name(commandName)
                .Argument("input", a => a
                    .Type<NonNullType<CommandType<TCommand, TCommandResponse>>>())
                .ResolveWith<CommandResolvers<TCommand, TCommandResponse>>(r =>
                    r.Execute(default, default, default));
        }
            
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name("mutation");

            foreach (var commandType in _domainMetadataInfo.CommandTypes)
            {
                var commandResponseType = commandType.BaseType
                    .GetGenericArguments()
                    .Single(o => typeof(ICommandResponse).IsAssignableFrom(o));
                
                var method = AddCommandDescriptorMethod.MakeGenericMethod(
                    commandType,
                    commandResponseType);
                method.Invoke(this, new object[] { descriptor });
            }
        }
    }
}