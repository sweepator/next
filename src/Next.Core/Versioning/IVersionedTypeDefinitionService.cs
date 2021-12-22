using System;
using System.Collections.Generic;

namespace Next.Abstractions.Core.Versioning
{
    public interface IVersionedTypeDefinitionService<TAttribute, TDefinition>
        where TAttribute : VersionedTypeAttribute
        where TDefinition : VersionedTypeDefinition
    {
        void Load(IEnumerable<Type> types);
        IEnumerable<TDefinition> GetDefinitions(string name);
        bool TryGetDefinition(string name, int version, out TDefinition definition);
        IEnumerable<TDefinition> GetAllDefinitions();
        TDefinition GetDefinition(string name, int version);
        TDefinition GetDefinition(Type type);
        IEnumerable<TDefinition> GetDefinitions(Type type);
        bool TryGetDefinition(Type type, out TDefinition definition);
        bool TryGetDefinitions(Type type, out IEnumerable<TDefinition> definitions);
        void Load(params Type[] types);
    }
}