namespace ThemePreviewer
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class ExpressionExtensions
    {
        public static PropertyInfo TryGetProperty<T, TValue>(
            this Expression<Func<T, TValue>> expression)
        {
            var propertyName = ((MemberExpression)expression.Body).Member.Name;
            var property = typeof(T).GetProperty(propertyName);
            if (property == null || property.PropertyType != typeof(TValue))
                return null;

            return property;
        }

        public static bool CreateDelegates<T, TValue>(
            this Expression<Func<T, TValue>> expression,
            out Func<T, TValue> getter, out Action<T, TValue> setter)
        {
            getter = null;
            setter = null;

            var property = expression.TryGetProperty();
            if (property == null || !property.CanRead || !property.CanWrite)
                return false;

            getter = (Func<T, TValue>)property.GetGetMethod().CreateDelegate(typeof(Func<T, TValue>));
            setter = (Action<T, TValue>)property.GetSetMethod().CreateDelegate(typeof(Action<T, TValue>));
            return true;
        }
    }
}
