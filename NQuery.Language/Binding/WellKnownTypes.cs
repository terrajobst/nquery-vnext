using System;

namespace NQuery.Language.Binding
{
    internal static class WellKnownTypes
    {
        private static class UnknownType { }
        private static class NullType { }
        private static class MissingType { }

        public static Type Unknown = typeof(UnknownType);
        public static Type Null = typeof(NullType);
        public static Type Missing = typeof(MissingType);

        public static bool IsMissing(this Type type)
        {
            return type == Missing;
        }

        public static bool IsNull(this Type type)
        {
            return type == Null;
        }

        public static bool IsUnknown(this Type type)
        {
            return type == Unknown;
        }
    }
}