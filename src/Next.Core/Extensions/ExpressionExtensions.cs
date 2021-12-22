using System.Reflection;

namespace System.Linq.Expressions
{
    public static class ExpressionExtensions
    {
        public static string GetPropertyName<TSource, TValue>(this Expression<Func<TSource, TValue>> property)
        {
            MemberExpression memberExpression;

            if (property.Body is UnaryExpression unaryExpression )
            {
                memberExpression = (MemberExpression)(unaryExpression.Operand);
            }
            else
            {
                memberExpression = (MemberExpression)(property.Body);
            }

            return ((PropertyInfo)memberExpression.Member).Name;
        }
    }
}