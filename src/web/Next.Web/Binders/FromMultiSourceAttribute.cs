using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Next.Web.Binders
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public class FromMultiSourceAttribute : Attribute, IBindingSourceMetadata
    {
        private readonly Lazy<BindingSource> _bindingSourceLazy;
        
        public string DisplayName { get; set; }
        public string[] BindingSources { get; set; }

        public BindingSource BindingSource => _bindingSourceLazy.Value;

        public FromMultiSourceAttribute(string displayName)
        {
            DisplayName = displayName;
            _bindingSourceLazy = new Lazy<BindingSource>(() => CompositeBindingSource.Create(GetBindingSources(), DisplayName));
        }
        
        private IEnumerable<BindingSource> GetBindingSources()
        {
            var bsType = typeof(BindingSource);

            return BindingSources.Select(source => bsType.GetField(source)?.GetValue(null) as BindingSource);
        }
    }
}