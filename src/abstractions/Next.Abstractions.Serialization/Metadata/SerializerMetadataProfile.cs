using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Next.Abstractions.Serialization.Metadata
{
    public class SerializerMetadataProfile<T>: ISerializerMetadataProfile
        where T: class
    {
        private readonly List<string> _ignoredProperties = new List<string>();
        private readonly Dictionary<string, string> _propertyNames = new Dictionary<string,string>();
        private readonly Dictionary<string, ISerializerPropertyFormatter> _propertyFormatters = new Dictionary<string,ISerializerPropertyFormatter>();
			 
        public IEnumerable<string> GetIgnoredProperties(Type targetType)
        {
            return typeof(T).IsAssignableFrom(targetType) ? _ignoredProperties : new List<string>();
        }
			 
        public IDictionary<string, string> GetPropertyNames(Type targetType)
        {
            return typeof(T).IsAssignableFrom(targetType) ? _propertyNames : new Dictionary<string, string>();
        }

        public IDictionary<string, ISerializerPropertyFormatter> GetPropertyFormatters(Type targetType)
        {
            return typeof(T).IsAssignableFrom(targetType) ? _propertyFormatters : new Dictionary<string, ISerializerPropertyFormatter>();
        }

        public SerializerMetadataProfile<T> IgnoreProperty(Expression<Func<T, object>> propertyNameExpression)
        {
            if (propertyNameExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyNameExpression));
            }
            
            _ignoredProperties.Add(propertyNameExpression.GetPropertyName());
            return this;
        }
			 
        public SerializerMetadataProfile<T> PropertyName(
            Expression<Func<T, object>> propertyNameExpression,
            string propertyName)
        {
            if (propertyNameExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyNameExpression));
            }
            
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            
            _propertyNames.Add(
                propertyNameExpression.GetPropertyName(),
                propertyName);
            return this;
        }
        
        public SerializerMetadataProfile<T> PropertyNameFormatter(
            Expression<Func<T, string>> propertyNameExpression,
            ISerializerPropertyFormatter serializerPropertyFormatter)
        {
            if (propertyNameExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyNameExpression));
            }
            
            if (serializerPropertyFormatter == null)
            {
                throw new ArgumentNullException(nameof(serializerPropertyFormatter));
            }
            
            _propertyFormatters.Add(
                propertyNameExpression.GetPropertyName(),
                serializerPropertyFormatter);
            return this;
        }
        
        public SerializerMetadataProfile<T> Replace(
            Expression<Func<T, string>> propertyNameExpression, 
            int? replaceIndex = null,
            char replaceChar = '*')
        {
            if (propertyNameExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyNameExpression));
            }
            
            var propFunc = propertyNameExpression.Compile();
            
            PropertyNameFormatter(
                propertyNameExpression,
                new FuncSerializerPropertyFormatter<T>(o =>
                {
                    var val = propFunc.Invoke(o);
                    var replacement = string.Empty;

                    for (var i = 0; i < 7; i++)
                    {
                        replacement += replaceChar;
                    }
                    
                    if (!replaceIndex.HasValue)
                    {
                        return replacement;
                    }

                    return val.Length > replaceIndex.Value
                        ? val.Substring(0, replaceIndex.Value) + replacement
                        : replacement;
                }));
            
            return this;
        }
    }
}