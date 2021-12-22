using System;
using System.Linq;
using System.Linq.Expressions;

namespace Next.Abstractions.Serialization.Metadata
{
    public class SerializerMetadataProfileBuilder<T> : ISerializerMetadataProfileBuilder<T>
        where T : class
    {
        private readonly SerializerMetadataProfile<T> _serializerMetadataProfile = new SerializerMetadataProfile<T>();
        public ISerializerMetadataProfile SerializerMetadataProfile => _serializerMetadataProfile;
        
        public ISerializerMetadataProfileBuilder<T> Ignore(Expression<Func<T, object>> propertyNameExpression)
        {
            _serializerMetadataProfile.IgnoreProperty(propertyNameExpression);
            return this;
        }

        public ISerializerMetadataProfileBuilder<T> Name(
            Expression<Func<T, object>> propertyNameExpression, 
            string propertyName)
        {
            _serializerMetadataProfile.PropertyName(
                propertyNameExpression,
                propertyName);
            return this;
        }

        public ISerializerMetadataProfileBuilder<T> Format(
            Expression<Func<T, string>> propertyNameExpression,
            ISerializerPropertyFormatter serializerPropertyFormatter)
        {
            _serializerMetadataProfile.PropertyNameFormatter(
                propertyNameExpression,
                serializerPropertyFormatter);
            return this;
        }
        
        public ISerializerMetadataProfileBuilder<T> Format(
            Expression<Func<T, string>> propertyNameExpression,
            Func<T, string> func)
        {
            _serializerMetadataProfile.PropertyNameFormatter(
                propertyNameExpression,
                new FuncSerializerPropertyFormatter<T>(func));
            return this;
        }

        public ISerializerMetadataProfileBuilder<T> Replace(
            Expression<Func<T, string>> propertyNameExpression, 
            int? replaceIndex = null,
            char replaceChar = '*')
        {
            _serializerMetadataProfile.Replace(
                propertyNameExpression,
                replaceIndex,
                replaceChar);
            return this;
        }
    }
}