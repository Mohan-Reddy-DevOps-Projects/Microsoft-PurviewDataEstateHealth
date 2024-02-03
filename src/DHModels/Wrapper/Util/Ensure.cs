#nullable disable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Util
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    public static class Ensure
    {
        public static void IsTrue(bool condition, string message)
        {
            if (!condition)
            {
                string method = GetFirstForeignMethodOnStack();
                string msg = string.Format(CultureInfo.InvariantCulture, "Condition '{0}' should not be false in {1}", message, method);
                throw new InvalidOperationException(msg);
            }
        }

        public static void IsNotNull(object value, string who)
        {
            if (value == null)
            {
                string method = GetFirstForeignMethodOnStack();
                string msg = string.Format(CultureInfo.InvariantCulture, "'{0}' may not be null in {1}", who, method);
                throw new ArgumentNullException(who, msg);
            }
        }

        public static void IsNotNullOrWhitespace(string value, string who)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                string method = GetFirstForeignMethodOnStack();
                string msg = string.Format(CultureInfo.InvariantCulture, "'{0}' may not be null in {1}", who, method);
                throw new ArgumentNullException(who, msg);
            }
        }

        public static void IsNotDefault<T>(T value, string who)
        {
            if (EqualityComparer<T>.Default.Equals(value, default))
            {
                string method = GetFirstForeignMethodOnStack();
                string msg = string.Format(CultureInfo.InvariantCulture, "'{0}' may not be default value in {1}", who, method);
                throw new ArgumentOutOfRangeException(who, msg);
            }
        }

        public static void IsNull(object value, string who)
        {
            if (value != null)
            {
                string method = GetFirstForeignMethodOnStack();
                string msg = string.Format(CultureInfo.InvariantCulture, "'{0}' should be null, but it is currently '{1} in {2}", who, value, method);
                throw new ArgumentOutOfRangeException(who, value, msg);
            }
        }

        public static void ArgNotNull(object value, string name)
        {
            if (value == null)
            {
                string method = GetFirstForeignMethodOnStack();
                string msg = string.Format(CultureInfo.InvariantCulture, "Argument '{0}' may not be null in {1}", name, method);
                throw new ArgumentNullException(name, msg);
            }
        }

        public static void ArgNotNullOrEmpty(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                string method = GetFirstForeignMethodOnStack();
                string msg = string.Format(CultureInfo.InvariantCulture, "'{0}' may not be null or empty in {1}", name, method);
                throw new ArgumentNullException(name, msg);
            }
        }

        public static void ArgNotNullOrWhiteSpace(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                string method = GetFirstForeignMethodOnStack();
                string msg = string.Format(CultureInfo.InvariantCulture, "'{0}' may not be null or whitespace in {1}", name, method);
                throw new ArgumentNullException(name, msg);
            }
        }

        public static void ArgSatisfiesCondition(string value, string name, bool condition)
        {
            if (!condition)
            {
                string method = GetFirstForeignMethodOnStack();
                string msg = string.Format(CultureInfo.InvariantCulture, "Argument '{0}' (value: {1}) does not meet condidtion in {2}", name, value, method);
                throw new ArgumentOutOfRangeException(name, value, msg);
            }
        }

        public static void ArgIsNotNegative(long value, string name, int stackFramesToSkip)
        {
            if (value < 0)
            {
                string method = GetFirstForeignMethodOnStack();
                string msg = string.Format(CultureInfo.InvariantCulture, "Constraint: '{0}' >= 0 has been violated in {1}", name, method);
                throw new ArgumentOutOfRangeException(name, value, msg);
            }
        }

        public static void ArgIsPositive(long value, string name, int stackFramesToSkip)
        {
            if (value <= 0)
            {
                string method = GetFirstForeignMethodOnStack();
                string msg = string.Format(CultureInfo.InvariantCulture, "Constraint: '{0}' > 0 has been violated in {1}", name, method);
                throw new ArgumentOutOfRangeException(name, value, msg);
            }
        }

        public static void ArgIsInRange<T>(T value, T begin, T end, string name, int stackFramesToSkip) where T : IComparable<T>
        {
            if (value.CompareTo(begin) < 0 || value.CompareTo(end) > 0)
            {
                string method = GetFirstForeignMethodOnStack();
                string msg = string.Format(CultureInfo.InvariantCulture, "'{0}' == '{1}' which is not in the range '{2}'..'{3}' in {4}", name, value, begin, end, method);
                throw new ArgumentOutOfRangeException(name, value, msg);
            }
        }

        private static string GetFirstForeignMethodOnStack(int stackFramesToSkip)
        {
            string ret = null;

            // 5: Reasonable upper limit so that we won't work too hard
            for (int framesToSkip = stackFramesToSkip + 1; framesToSkip < stackFramesToSkip + 5; framesToSkip++)
            {
                var callPoint = new StackFrame(framesToSkip, true);
                string callPointMethod = GetFullyQualifiedMemberName(callPoint.GetMethod());
                ret = "method '" + callPointMethod + "' (" + callPoint + ")";

                if (!callPointMethod.Contains("Microsoft.DataPipeline.Utilities.Validation.", StringComparison.OrdinalIgnoreCase) &&
                    !callPointMethod.Contains("Microsoft.DataPipeline.Utilities.Ensure.", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }

            return ret;
        }

        private static string GetFirstForeignMethodOnStack()
        {
            // 2: Counting this method and our caller (which is known to be non-foreign)
            return GetFirstForeignMethodOnStack(2);
        }

        private static string GetFullyQualifiedMemberName(MemberInfo member)
        {
            var type = member.DeclaringType;
            var assembly = type.Assembly;
            string assemblyName = assembly.ManifestModule.Name;
            string ret = string.Format(CultureInfo.InvariantCulture, "{0}!{1}.{2}", assemblyName, type.FullName, member.Name);
            return ret;
        }
    }
}