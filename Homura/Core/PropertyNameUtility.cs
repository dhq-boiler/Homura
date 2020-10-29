

using System;
using System.Linq.Expressions;

namespace Homura.Core
{
    /*
     * Reflection - get property name [duplicate] https://stackoverflow.com/questions/4657311/reflection-get-property-name
     * quizzer user137348 https://stackoverflow.com/users/137348/user137348
     * answerer max https://stackoverflow.com/users/356716/max
     */

    public static class PropertyNameUtility
    {
        // requires object instance, but you can skip specifying T
        public static string GetPropertyName<T>(Expression<Func<T>> exp)
        {
            return (((MemberExpression)(exp.Body)).Member).Name;
        }

        // requires explicit specification of both object type and property type
        public static string GetPropertyName<TObject, TResult>(Expression<Func<TObject, TResult>> exp)
        {
            // extract property name
            return (((MemberExpression)(exp.Body)).Member).Name;
        }

        // requires explicit specification of object type
        public static string GetPropertyName<TObject>(Expression<Func<TObject, object>> exp)
        {
            var body = exp.Body;
            var convertExpression = body as UnaryExpression;
            if (convertExpression != null)
            {
                if (convertExpression.NodeType != ExpressionType.Convert)
                {
                    throw new ArgumentException("Invalid property expression.", "exp");
                }
                body = convertExpression.Operand;
            }
            return ((MemberExpression)body).Member.Name;
        }
    }
}
