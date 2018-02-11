namespace PresentationTheme.Aero.Win8
{
    using System;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Media;
    using Expression = System.Linq.Expressions.Expression;

    internal static class LayoutHelper
    {
        private static readonly Func<Visual, double> GetDpiXDelegate;

        static LayoutHelper()
        {
            var getDpiMethod = typeof(Visual).GetMethod("GetDpi", BindingFlags.NonPublic | BindingFlags.Instance);
            if (getDpiMethod != null) {
                var dpiScaleXProperty = getDpiMethod.ReturnType.GetProperty("DpiScaleX");

                var visualParam = Expression.Parameter(typeof(Visual));
                var lambda = Expression.Lambda<Func<Visual, double>>(
                    Expression.Property(
                        Expression.Call(visualParam, getDpiMethod),
                        dpiScaleXProperty),
                    visualParam);

                GetDpiXDelegate = lambda.Compile();
                return;
            }

            var feDpiScaleXProperty = typeof(FrameworkElement).GetProperty(
                "DpiScaleX", BindingFlags.NonPublic | BindingFlags.Static);
            if (feDpiScaleXProperty != null) {
                var lambda = Expression.Lambda<Func<Visual, double>>(
                    Expression.Property(null, feDpiScaleXProperty),
                    Expression.Parameter(typeof(Visual)));

                GetDpiXDelegate = lambda.Compile();
                return;
            }

            GetDpiXDelegate = _ => 1.0;
        }

        public static double GetDpiX(Visual visual)
        {
            return GetDpiXDelegate(visual);
        }

        public static double FloorLayoutValue(double value, double dpiScale)
        {
            double newValue;

            // If DPI == 1, don't use DPI-aware rounding.
            if (!DoubleUtil.AreClose(dpiScale, 1.0)) {
                newValue = Math.Floor(value * dpiScale) / dpiScale;
                // If rounding produces a value unacceptable to layout (NaN, Infinity or MaxValue), use the original value.
                if (double.IsNaN(newValue) ||
                    Double.IsInfinity(newValue) ||
                    DoubleUtil.AreClose(newValue, Double.MaxValue)) {
                    newValue = value;
                }
            } else {
                newValue = Math.Floor(value);
            }

            return newValue;
        }

        public static double RoundLayoutValue(double value, double dpiScale)
        {
            double newValue;

            // If DPI == 1, don't use DPI-aware rounding.
            if (!DoubleUtil.AreClose(dpiScale, 1.0)) {
                newValue = Math.Round(value * dpiScale) / dpiScale;
                // If rounding produces a value unacceptable to layout (NaN, Infinity or MaxValue), use the original value.
                if (double.IsNaN(newValue) ||
                    Double.IsInfinity(newValue) ||
                    DoubleUtil.AreClose(newValue, Double.MaxValue)) {
                    newValue = value;
                }
            } else {
                newValue = Math.Round(value);
            }

            return newValue;
        }
    }
}
