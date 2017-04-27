namespace ThemePreviewer
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;

    public static class Extensions
    {
        public static void Forget(this Task task)
        {
        }

        public static Rect Round(this Rect rect)
        {
            return new Rect(
                Math.Round(rect.X), Math.Round(rect.Y),
                Math.Round(rect.Width), Math.Round(rect.Height));
        }

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

        public static string GetWindowsName(this FileVersionInfo fileVersion)
        {
            var major = fileVersion.FileMajorPart;
            var minor = fileVersion.FileMinorPart;
            if (major == 6 && minor == 0)
                return "Windows Vista";
            if (major == 6 && minor == 1)
                return "Windows 7";
            if (major == 6 && minor == 2)
                return "Windows 8";
            if (major == 6 && minor == 3)
                return "Windows 8.1";
            if (major == 10)
                return "Windows 10";
            return null;
        }
    }
}
