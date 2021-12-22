using System;
using System.Collections.Generic;

namespace Next.Web.Hypermedia
{
    public class LinksOptions
    {
        private const string DefaultPolicyName = "Default";
        
        private IDictionary<string, ILinksPolicy> Policies { get; } = new Dictionary<string, ILinksPolicy>();
        public ILinkTransformation HrefTransformation { get; } = new LinkTransformationBuilder()
            .AddRoutePath()
            .Build();
        
        public ILinksPolicy GetPolicy<TResource>()
        {
            return GetPolicy<TResource>(DefaultPolicyName);
        }
        
        public ILinksPolicy GetPolicy(Type resourceType)
        {
            return GetPolicy(
                resourceType,
                DefaultPolicyName);
        }
        
        public void AddPolicy<TResource>(ILinksPolicy policy)
        {
            AddPolicy<TResource>(
                DefaultPolicyName, 
                policy);
        }
        
        public ILinksPolicy GetPolicy(
            Type resourceType,
            string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Policy name cannot be null", nameof(name));
            }
                
            var policyName = CreateFullPolicyName(
                resourceType,
                name);
            
            return Policies.ContainsKey(policyName) ? Policies[policyName] : null;
        }

        public ILinksPolicy GetPolicy<TResource>(string name)
        {
            return GetPolicy(
                typeof(TResource),
                name);
        }
        
        public void AddPolicy<TResource>(
            string name, 
            Action<LinksPolicyBuilder<TResource>> configurePolicy)
        {
            if (configurePolicy == null)
            {
                throw new ArgumentNullException(nameof(configurePolicy));
            }

            var builder = new LinksPolicyBuilder<TResource>();
            configurePolicy(builder);

            AddPolicy<TResource>(name, builder.Build());
        }
        
        public void AddPolicy<TResource>(Action<LinksPolicyBuilder<TResource>> configurePolicy)
        {
            AddPolicy(DefaultPolicyName, configurePolicy);
        }

        private void AddPolicy<TResource>(
            string name, 
            ILinksPolicy policy)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Policy name cannot be null.", nameof(name));
            }

            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            var policyName = CreateFullPolicyName(
                typeof(TResource),
                name);
            Policies[policyName] = policy;
        }
        
        private static string CreateFullPolicyName(
            Type resourceType,
            string name)
        {
            return $"{name}:{resourceType.FullName}";
        }
    }
}