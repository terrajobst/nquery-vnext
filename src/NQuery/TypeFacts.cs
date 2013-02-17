using System;

using NQuery.Symbols;

namespace NQuery
{
    public static class TypeFacts
    {
        private static class MissingType { }
        private static class UnknownType { }
        private static class NullType { }

        public static readonly Type Missing = typeof(MissingType);
        public static readonly Type Unknown = typeof(UnknownType);
        public static readonly Type Null = typeof(NullType);

        public static bool IsMissing(this Type type)
        {
            return type == Missing;
        }

        public static bool IsUnknown(this Type type)
        {
            return type == Unknown;
        }

        public static bool IsError(this Type type)
        {
            return type.IsMissing() || type.IsUnknown();
        }

        public static bool IsNull(this Type type)
        {
            return type == Null;
        }

        internal static bool IsNonBoolean(this Type type)
        {
            return !type.IsError() && type != typeof(bool);
        }

        internal static bool IsIntrinsicNumericType(this KnownType value)
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

                case KnownType.Decimal:
                case KnownType.Boolean:
                case KnownType.String:
                case KnownType.Object:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }

        internal static bool IsSignedNumericType(this KnownType value)
        {
            switch (value)
            {
                case KnownType.SByte:
                case KnownType.Int16:
                case KnownType.Int32:
                case KnownType.Int64:
                    return true;

                default:
                    return false;
            }
        }

        internal static bool IsUnsignedNumericType(this KnownType value)
        {
            switch (value)
            {
                case KnownType.Byte:
                case KnownType.UInt16:
                case KnownType.UInt32:
                case KnownType.UInt64:
                    return true;

                default:
                    return false;
            }
        }

        internal static KnownType? GetKnownType(this Type type)
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

            if (type == typeof(decimal))
                return KnownType.Decimal;

            if (type == typeof(Boolean))
                return KnownType.Boolean;

            if (type == typeof(String))
                return KnownType.String;

            if (type == typeof(Object))
                return KnownType.Object;

            return null;
        }

        public static string ToDisplayName(this Type type)
        {
            if (type.IsUnknown())
                return "<?>";

            if (type.IsNull())
                return "<null>";

            if (type.IsMissing())
                return "<missing>";

            var knownType = type.GetKnownType();
            return knownType == null ? type.Name : knownType.Value.ToDisplayName();
        }

        private static string ToDisplayName(this KnownType type)
        {
            switch (type)
            {
                case KnownType.SByte:
                    return "SBYTE";
                case KnownType.Byte:
                    return "BYTE";
                case KnownType.Int16:
                    return "SHORT";
                case KnownType.UInt16:
                    return "USHORT";
                case KnownType.Int32:
                    return "INT";
                case KnownType.UInt32:
                    return "UINT";
                case KnownType.Int64:
                    return "LONG";
                case KnownType.UInt64:
                    return "ULONG";
                case KnownType.Char:
                    return "CHAR";
                case KnownType.Single:
                    return "FLOAT";
                case KnownType.Double:
                    return "DOUBLE";
                case KnownType.Decimal:
                    return "DECIMAL";
                case KnownType.Boolean:
                    return "BOOL";
                case KnownType.String:
                    return "STRING";
                case KnownType.Object:
                    return "OBJECT";
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        public static bool IsComparable(this Type type)
        {
            var comarable = typeof (IComparable);
            return comarable.IsAssignableFrom(type);
        }
    }
}