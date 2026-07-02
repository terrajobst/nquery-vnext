using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis;

public static class TypeFacts
{
    private static class MissingType { }
    private static class UnknownType { }
    private static class NullType { }

    public static Type Missing { get; } = typeof(MissingType);
    public static Type Unknown { get; } = typeof(UnknownType);
    public static Type Null { get; } = typeof(NullType);

    extension(Type type)
    {
        public bool IsMissing()
        {
            return type == Missing;
        }

        public bool IsUnknown()
        {
            return type == Unknown;
        }

        public bool IsError()
        {
            return type.IsMissing() || type.IsUnknown();
        }

        public bool IsNull()
        {
            return type == Null;
        }

        internal Type ToOutputType()
        {
            return type.IsNull() ? typeof(object) : type;
        }

        internal bool IsNonBoolean()
        {
            return !type.IsError() && type != typeof(bool);
        }

        internal KnownType? GetKnownType()
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

        public string ToDisplayName()
        {
            if (type.IsUnknown())
                return Resources.TypeUnknown;

            if (type.IsNull())
                return Resources.TypeNull;

            if (type.IsMissing())
                return Resources.TypeMissing;

            var knownType = type.GetKnownType();
            return knownType is null ? type.Name : knownType.Value.ToDisplayName();
        }

        public bool IsComparable()
        {
            var comparable = typeof(IComparable);
            return comparable.IsAssignableFrom(type);
        }

        public bool CanBeNull()
        {
            var isReferenceType = type.IsClass;
            var isNullableOfT = type.IsNullableOfT();
            return isReferenceType || isNullableOfT;
        }

        public bool IsNullableOfT()
        {
            return Nullable.GetUnderlyingType(type) is not null;
        }

        public Type GetNonNullableType()
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        public Type GetNullableType()
        {
            return type.CanBeNull()
                       ? type
                       : typeof(Nullable<>).MakeGenericType(type);
        }
    }

    extension(KnownType value)
    {
        internal bool IsIntrinsicNumericType()
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

        internal bool IsSignedNumericType()
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

        internal bool IsUnsignedNumericType()
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
    }

    extension(KnownType type)
    {
        private string ToDisplayName()
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
    }
}
