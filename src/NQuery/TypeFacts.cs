using System;
using System.Linq;

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

        internal static Type ToOutputType(this Type type)
        {
            return type.IsNull() ? typeof (object) : type;
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
                    throw ExceptionBuilder.UnexpectedValue(value);
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
            if (type == typeof(byte))
                return KnownType.Byte;

            if (type == typeof(sbyte))
                return KnownType.SByte;

            if (type == typeof(char))
                return KnownType.Char;

            if (type == typeof(short))
                return KnownType.Int16;

            if (type == typeof(ushort))
                return KnownType.UInt16;

            if (type == typeof(int))
                return KnownType.Int32;

            if (type == typeof(uint))
                return KnownType.UInt32;

            if (type == typeof(long))
                return KnownType.Int64;

            if (type == typeof(ulong))
                return KnownType.UInt64;

            if (type == typeof(float))
                return KnownType.Single;

            if (type == typeof(double))
                return KnownType.Double;

            if (type == typeof(decimal))
                return KnownType.Decimal;

            if (type == typeof(bool))
                return KnownType.Boolean;

            if (type == typeof(string))
                return KnownType.String;

            if (type == typeof(object))
                return KnownType.Object;

            return null;
        }

        public static string ToDisplayName(this Type type)
        {
            if (type.IsUnknown())
                return Resources.TypeUnknown;

            if (type.IsNull())
                return Resources.TypeNull;

            if (type.IsMissing())
                return Resources.TypeMissing;

            var knownType = type.GetKnownType();
            return knownType == null ? type.Name : knownType.Value.ToDisplayName();
        }

        private static string ToDisplayName(this KnownType type)
        {
            switch (type)
            {
                case KnownType.SByte:
                    return @"SBYTE";
                case KnownType.Byte:
                    return @"BYTE";
                case KnownType.Int16:
                    return @"SHORT";
                case KnownType.UInt16:
                    return @"USHORT";
                case KnownType.Int32:
                    return @"INT";
                case KnownType.UInt32:
                    return @"UINT";
                case KnownType.Int64:
                    return @"LONG";
                case KnownType.UInt64:
                    return @"ULONG";
                case KnownType.Char:
                    return @"CHAR";
                case KnownType.Single:
                    return @"FLOAT";
                case KnownType.Double:
                    return @"DOUBLE";
                case KnownType.Decimal:
                    return @"DECIMAL";
                case KnownType.Boolean:
                    return @"BOOL";
                case KnownType.String:
                    return @"STRING";
                case KnownType.Object:
                    return @"OBJECT";
                default:
                    throw ExceptionBuilder.UnexpectedValue(type);
            }
        }

        public static bool IsComparable(this Type type)
        {
            var comparable = typeof (IComparable);
            return comparable.IsAssignableFrom(type);
        }

        public static bool CanBeNull(this Type type)
        {
            var isReferenceType = type.IsClass;
            var isNullableOfT = type.IsNullableOfT();
            return isReferenceType || isNullableOfT;
        }

        public static bool IsNullableOfT(this Type type)
        {
            return type.IsValueType &&
                   type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof (Nullable<>);
        }

        public static Type GetNonNullableType(this Type type)
        {
            return type.IsNullableOfT()
                       ? type.GetGenericArguments().Single()
                       : type;
        }

        public static Type GetNullableType(this Type type)
        {
            return type.CanBeNull()
                       ? type
                       : typeof (Nullable<>).MakeGenericType(type);
        }
    }
}