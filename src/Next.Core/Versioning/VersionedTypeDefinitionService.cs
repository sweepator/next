using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Next.Abstractions.Core.Versioning
{
    public abstract class VersionedTypeDefinitionService<TTypeCheck, TAttribute, TDefinition> : IVersionedTypeDefinitionService<TAttribute, TDefinition>
        where TAttribute : VersionedTypeAttribute
        where TDefinition : VersionedTypeDefinition
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Regex NameRegex = new Regex(
            @"^(Old){0,1}(?<name>[\p{L}\p{Nd}]+?)(V(?<version>[0-9]+)){0,1}$",
            RegexOptions.Compiled);
        
        private readonly object _syncRoot = new object();
        private readonly ConcurrentDictionary<Type, List<TDefinition>> _definitionsByType = new ConcurrentDictionary<Type, List<TDefinition>>();
        private readonly ConcurrentDictionary<string, Dictionary<int, TDefinition>> _definitionByNameAndVersion = new ConcurrentDictionary<string, Dictionary<int, TDefinition>>(); 
        

        public void Load(params Type[] types)
        {
            Load((IReadOnlyCollection<Type>) types);
        }

        public void Load(IEnumerable<Type> types)
        {
            if (types == null)
            {
                return;
            }

            var invalidTypes = types
                .Where(t => !typeof(TTypeCheck).GetTypeInfo().IsAssignableFrom(t))
                .ToList();
            if (invalidTypes.Any())
            {
                throw new ArgumentException($"The following types are not of type '{typeof(TTypeCheck).Name}': {string.Join(", ", invalidTypes.Select(t => t.Name))}");
            }

            lock (_syncRoot)
            {
                var definitions = types
                    .Distinct()
                    .Where(t => !_definitionsByType.ContainsKey(t))
                    .SelectMany(CreateDefinitions)
                    .ToList();
                if (!definitions.Any())
                {
                    return;
                }
                
                foreach (var definition in definitions)
                {
                    var typeDefinitions = _definitionsByType.GetOrAdd(
                        definition.Type,
                        _ => new List<TDefinition>());
                    typeDefinitions.Add(definition);

                    if (!_definitionByNameAndVersion.TryGetValue(definition.Name, out var versions))
                    {
                        versions = new Dictionary<int, TDefinition>();
                        _definitionByNameAndVersion.TryAdd(definition.Name, versions);
                    }

                    if (versions.ContainsKey(definition.Version))
                    {
                        continue;
                    }

                    versions.Add(definition.Version, definition);
                }
            }
        }

        public IEnumerable<TDefinition> GetDefinitions(string name)
        {
            return _definitionByNameAndVersion.TryGetValue(name, out var versions)
                ? versions.Values.OrderBy(d => d.Version)
                : Enumerable.Empty<TDefinition>();
        }

        public IEnumerable<TDefinition> GetAllDefinitions()
        {
            return _definitionByNameAndVersion.SelectMany(kv => kv.Value.Values);
        } 

        public bool TryGetDefinition(string name, int version, out TDefinition definition)
        {
            if (_definitionByNameAndVersion.TryGetValue(name, out var versions))
            {
                return versions.TryGetValue(version, out definition);
            }

            definition = null;

            return false;
        }

        public TDefinition GetDefinition(string name, int version)
        {
            if (!TryGetDefinition(name, version, out var definition))
            {
                throw new ArgumentException($"No versioned type definition for '{name}' with version {version} in '{GetType().Name}'");
            }

            return definition;
        }

        public TDefinition GetDefinition(Type type)
        {
            if (!TryGetDefinition(type, out var definition))
            {
                throw new ArgumentException($"No definition for type '{type.Name}'.");
            }

            return definition;
        }

        public IEnumerable<TDefinition> GetDefinitions(Type type)
        {
            if (!TryGetDefinitions(type, out var definitions))
            {
                throw new ArgumentException($"No definition for type '{type.Name}'.");
            }

            return definitions;
        }

        public bool TryGetDefinition(Type type, out TDefinition definition)
        {
            if (!TryGetDefinitions(type, out var definitions))
            {
                definition = default;
                return false;
            }

            if (definitions.Count() > 1)
            {
                throw new InvalidOperationException($"Type '{type.Name}' has multiple definitions: {string.Join(", ", definitions.Select(d => d.ToString()))}");
            }

            definition = definitions.Single();
            return true;
        }

        public bool TryGetDefinitions(Type type, out IEnumerable<TDefinition> definitions)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (!_definitionsByType.TryGetValue(type, out var list))
            {
                definitions = default;
                return false;
            }

            definitions = list;
            return true;
        }

        protected abstract TDefinition CreateDefinition(int version, Type type, string name);

        private IEnumerable<TDefinition> CreateDefinitions(Type versionedType)
        {
            var hasAttributeDefinition = false;
            foreach (var definitionFromAttribute in CreateDefinitionFromAttribute(versionedType))
            {
                hasAttributeDefinition = true;
                yield return definitionFromAttribute;
            }

            if (hasAttributeDefinition) yield break;
            
            yield return CreateDefinitionFromName(versionedType);
        }

        private TDefinition CreateDefinitionFromName(Type versionedType)
        {
            var match = NameRegex.Match(versionedType.Name);
            if (!match.Success)
            {
                throw new ArgumentException($"Versioned type name '{versionedType.Name}' is not a valid name");
            }

            var version = 1;
            var groups = match.Groups["version"];
            if (groups.Success)
            {
                version = int.Parse(groups.Value);
            }

            var name = match.Groups["name"].Value;
            return CreateDefinition(
                version,
                versionedType,
                name);
        }

        private IEnumerable<TDefinition> CreateDefinitionFromAttribute(Type versionedType)
        {
            return versionedType
                .GetTypeInfo()
                .GetCustomAttributes()
                .OfType<TAttribute>()
                .Select(a => CreateDefinition(a.Version, versionedType, a.Name));
        }
    }
}