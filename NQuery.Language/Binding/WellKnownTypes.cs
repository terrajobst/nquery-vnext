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

        public static bool IsNumericType(this KnownType value)
        {
            switch (value)
            {
                case KnownType.SByte:
                case KnownType.Byte:
                case KnownType.Int16:
                case KnownType.UInt16:
                case KnownType.Int32:
                case KnownType.UInt32:
                case KnownType.Int64:
                case KnownType.UInt64:
                case KnownType.Char:
                case KnownType.Single:
                case KnownType.Double:
                    return true;

                case KnownType.Boolean:
                case KnownType.String:
                case KnownType.Object:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }

        public static KnownType? GetKnownType(this Type type)
        {
            if (type == typeof(Byte))
                return KnownType.Byte;

            if (type == typeof(SByte))
                return KnownType.SByte;

            if (type == typeof(Char))
                return KnownType.Char;

            if (type == typeof(Int16))
                return KnownType.Int16;

            if (type == typeof(UInt16))
                return KnownType.UInt16;

            if (type == typeof(Int32))
                return KnownType.Int32;

            if (type == typeof(UInt32))
                return KnownType.UInt32;

            if (type == typeof(Int64))
                return KnownType.Int64;

            if (type == typeof(UInt64))
                return KnownType.UInt64;

            if (type == typeof(Single))
                return KnownType.Single;

            if (type == typeof(Double))
                return KnownType.Double;

            if (type == typeof(Boolean))
                return KnownType.Boolean;

            if (type == typeof(String))
                return KnownType.String;

            if (type == typeof(Object))
                return KnownType.Object;

            return null;
        }
    }
}