namespace PresentationTheme.Aero.Win10
{
    using System;

    internal static class DoubleUtil
    {
        /// <devdoc>Smallest such that 1.0+DBL_EPSILON! = 1.0</devdoc>
        internal const double DBL_EPSILON = 2.2204460492503131e-016;

        /// <summary>
        ///   Returns whether or not two doubles are "close". That is, whether
        ///   or not they are within epsilon of each other. Note that this epsilon
        ///   is proportional to the numbers themselves to that AreClose survives
        ///   scalar multiplication.
        /// </summary>
        /// <param name="value1">The first double to compare.</param>
        /// <param name="value2">The second double to compare.</param>
        /// <returns>bool - the result of the AreClose comparision.</returns>
        public static bool AreClose(double value1, double value2)
        {
            //in case they are Infinities (then epsilon check does not work)
            if (value1 == value2) return true;
            // This computes (|value1-value2| / (|value1| + |value2| + 10.0)) < DBL_EPSILON
            double eps = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * DBL_EPSILON;
            double delta = value1 - value2;
            return -eps < delta && eps > delta;
        }

        /// <summary>
        ///   LessThan - Returns whether or not the first double is less than
        ///   the second double. That is, whether or not the first is strictly
        ///   less than *and* not within epsilon of the other number. Note that
        ///   this epsilon is proportional to the numbers themselves to that
        ///   AreClose survives scalar multiplication.
        /// </summary>
        /// <param name="value1">The first value to compare.</param>
        /// <param name="value2">The second value to compare.</param>
        /// <returns>The result of the comparison.</returns>
        public static bool LessThan(double value1, double value2)
        {
            return value1 < value2 && !AreClose(value1, value2);
        }

        /// <summary>
        ///   Returns whether or not the first double is greater than the second
        ///   double. That is, whether or not the first is strictly greater than
        ///   *and* not within epsilon of the other number. Note that this epsilon
        ///   is proportional to the numbers themselves to that AreClose survives
        ///   scalar multiplication.
        /// </summary>
        /// <param name="value1">The first value to compare.</param>
        /// <param name="value2">The second value to compare.</param>
        /// <returns>The result of the comparison.</returns>
        public static bool GreaterThan(double value1, double value2)
        {
            return value1 > value2 && !AreClose(value1, value2);
        }

        /// <summary>
        ///   Returns whether or not the first double is less than or close to
        ///   the second double. That is, whether or not the first is strictly
        ///   less than or within epsilon of the other number. Note that this
        ///   epsilon is proportional to the numbers themselves to that AreClose
        ///   survives scalar multiplication.
        /// </summary>
        /// <param name="value1">The first value to compare.</param>
        /// <param name="value2">The second value to compare.</param>
        /// <returns>The result of the comparison.</returns>
        public static bool LessThanOrClose(double value1, double value2)
        {
            return value1 < value2 || AreClose(value1, value2);
        }

        /// <summary>
        ///   GreaterThanOrClose - Returns whether or not the first double is
        ///   greater than or close to the second double. That is, whether or
        ///   not the first is strictly greater than or within epsilon of the
        ///   other number. Note that this epsilon is proportional to the numbers
        ///   themselves to that AreClose survives scalar multiplication.
        /// </summary>
        /// <param name="value1">The first value to compare.</param>
        /// <param name="value2">The second value to compare.</param>
        /// <returns>The result of the GreaterThanOrClose comparision.</returns>
        public static bool GreaterThanOrClose(double value1, double value2)
        {
            return value1 > value2 || AreClose(value1, value2);
        }
    }
}
