using System;
using System.Linq.Expressions;

namespace Next.Abstractions.Serialization.Metadata
{
    public interface ISerializerMetadataProfileBuilder<T>
        where T: class
    {
        ISerializerMetadataProfile SerializerMetadataProfile { get; }
        
        ISerializerMetadataProfileBuilder<T> Ignore(Expression<Func<T, object>> propertyNameExpression);
        
        ISerializerMetadataProfileBuilder<T> Name(
            Expression<Func<T, object>> propertyNameExpression,
            string propertyName);
        
        ISerializerMetadataProfileBuilder<T> Format(
            Expression<Func<T, string>> propertyNameExpression,
            ISerializerPropertyFormatter serializerPropertyFormatter);
        
        ISerializerMetadataProfileBuilder<T> Format(
            Expression<Func<T, string>> propertyNameExpression,
            Func<T, string> func);
        
        ISerializerMetadataProfileBuilder<T> Replace(
            Expression<Func<T, string>> propertyNameExpression,
            int? replaceIndex = null,
            char replaceChar = '*');
    }
}