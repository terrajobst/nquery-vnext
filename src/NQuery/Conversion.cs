using System.Collections.Immutable;
using System.Reflection;

using NQuery.Symbols;

namespace NQuery
{
    public sealed class Conversion
    {
        private const string ImplicitMethodName = "op_Implicit";
        private const string ExplicitMethodName = "op_Explicit";
        private const BindingFlags ConversionMethodBindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

        private const bool N = false;
        private const bool Y = true;

        private static readonly bool[,] ImplicitNumericConversions =
        {
            /*                SByte     Byte     Short     UShort     Int     UInt     Long     ULong     Char     Float     Double*/
            /* SByte   */  {  N,        N,       Y,        N,         Y,      N,       Y,       N,        N,       Y,        Y},
            /* Byte    */  {  N,        N,       Y,        Y,         Y,      Y,       Y,       Y,        N,       Y,        Y},
            /* Short   */  {  N,        N,       N,        N,         Y,      N,       Y,       N,        N,       Y,        Y},
            /* UShort  */  {  N,        N,       N,        N,         Y,      Y,       Y,       Y,        N,       Y,        Y},
            /* Int     */  {  N,        N,       N,        N,         N,      N,       Y,       N,        N,       Y,        Y},
            /* UInt    */  {  N,        N,       N,        N,         N,      N,       Y,       Y,        N,       Y,        Y},
            /* Long    */  {  N,        N,       N,        N,         N,      N,       N,       N,        N,       Y,        Y},
            /* ULong   */  {  N,        N,       N,        N,         N,      N,       N,       N,        N,       Y,        Y},
            /* Char    */  {  N,        N,       N,        Y,         Y,      Y,       Y,       Y,        N,       Y,        Y},
            /* Float   */  {  N,        N,       N,        N,         N,      N,       N,       N,        N,       N,        Y},
            /* Double  */  {  N,        N,       N,        N,         N,      N,       N,       N,        N,       N,        N}
        };

        private readonly bool _isBoxingOrUnboxing;

        private Conversion(bool exists, bool isIdentity, bool isImplicit, bool isBoxingOrUnboxing, bool isReference, IEnumerable<MethodInfo> conversionMethods)
        {
            Exists = exists;
            IsIdentity = isIdentity;
            IsImplicit = isImplicit;
            _isBoxingOrUnboxing = isBoxingOrUnboxing;
            IsReference = isReference;
            ConversionMethods = conversionMethods?.ToImmutableArray() ?? ImmutableArray<MethodInfo>.Empty;
        }

        private static readonly Conversion None = new(false, false, false, false, false, null);
        private static readonly Conversion Null = new(true, false, true, false, false, null);
        private static readonly Conversion Identity = new(true, true, true, false, false, null);
        private static readonly Conversion Implicit = new(true, false, true, false, false, null);
        private static readonly Conversion Explicit = new(true, false, false, false, false, null);
        private static readonly Conversion Boxing = new(true, false, true, true, false, null);
        private static readonly Conversion Unboxing = new(true, false, false, true, false, null);
        private static readonly Conversion UpCast = new(true, false, true, false, true, null);
        private static readonly Conversion DownCast = new(true, false, false, false, true, null);

        public bool Exists { get; }

        public bool IsIdentity { get; }

        public bool IsImplicit { get; }

        public bool IsExplicit => Exists && !IsImplicit;

        public bool IsBoxing => _isBoxingOrUnboxing && IsImplicit;

        public bool IsUnboxing => _isBoxingOrUnboxing && IsExplicit;

        public bool IsReference { get; }

        public ImmutableArray<MethodInfo> ConversionMethods { get; }

        internal static Conversion Classify(Type sourceType, Type targetType)
        {
            if (sourceType == targetType)
                return Identity;

            var knownSourceType = sourceType.GetKnownType();
            var knownTargetType = targetType.GetKnownType();

            if (knownSourceType is not null && knownTargetType is not null)
            {
                if (HasImplicitNumericConversion(knownSourceType.Value, knownTargetType.Value))
                    return Implicit;

                if (HasExplicitNumericConversion(knownSourceType.Value, knownTargetType.Value))
                    return Explicit;
            }

            if (sourceType.IsValueType && knownTargetType == KnownType.Object)
            {
                // Converting a value type to object is always possible (AKA 'boxing').
                return Boxing;
            }

            if (knownSourceType == KnownType.Object && targetType.IsValueType)
            {
                // Converting object to a value type is always possible (AKA 'unboxing').
                return Unboxing;
            }

            if (sourceType.IsNull() || targetType.IsNull())
            {
                // If either side is the null type, we have an implicit conversion to or from NULL.
                return Null;
            }

            if (!sourceType.IsValueType && !targetType.IsValueType)
            {
                // If both are reference types, let's check whether target is a base type
                // of source. In that case it's an implicit upcast.
                if (targetType.IsAssignableFrom(sourceType))
                    return UpCast;

                // The reverse would be an explicit downcast.
                if (sourceType.IsAssignableFrom(targetType))
                    return DownCast;
            }

            // TODO: The implementation of user defined conversions (UDC) is incomplete.
            //
            //       For instance, the following is considered valid in C#:
            //
            //              class SomeType
            //              {
            //                  public static implicit operator sbyte (SomeType argument)
            //                  {
            //                      return 42;
            //                  }
            //              }
            //
            //              class Program
            //              {
            //                  static void Main()
            //                  {
            //                      float x = new SomeType();
            //                      var y = - new SomeType(); // Uses unary minus on int
            //                  }
            //              }
            //
            //       In other words, the conversions SomeType -> float and SomeType -> int
            //       are considered valid because SomeType has a UDC from SomeType -> sbyte.
            //
            //       Also, C# considers UDC that are on base types.
            //
            //       Implementing this isn't that easy because we also need to apply the
            //       usual betterness rules in order to find the best UDC among a set of
            //       candidates. For instance, if we'd add an implicit conversion to
            //       SomeType -> int, that's the one it would use instead of the one
            //       that returns an sbyte.

            var implicitConversions = GetConversionMethods(sourceType, targetType, true);
            if (implicitConversions.Length > 0)
                return ImplicitViaConversionMethod(implicitConversions);

            var explicitConversions = GetConversionMethods(sourceType, targetType, false);
            if (explicitConversions.Length > 0)
                return ExplicitViaConversionMethod(explicitConversions);

            return None;
        }

        private static Conversion ImplicitViaConversionMethod(IEnumerable<MethodInfo> implicitConversions)
        {
            return new Conversion(true, false, true, false, false, implicitConversions);
        }

        private static Conversion ExplicitViaConversionMethod(IEnumerable<MethodInfo> implicitConversions)
        {
            return new Conversion(true, false, false, false, false, implicitConversions);
        }

        private static bool HasImplicitNumericConversion(KnownType sourceType, KnownType targetType)
        {
            if (sourceType.IsIntrinsicNumericType() && targetType.IsIntrinsicNumericType())
            {
                var sourceIndex = (int)sourceType;
                var targetIndex = (int)targetType;
                return ImplicitNumericConversions[sourceIndex, targetIndex];
            }

            return false;
        }

        private static bool HasExplicitNumericConversion(KnownType sourceType, KnownType targetType)
        {
            if (sourceType.IsIntrinsicNumericType() && targetType.IsIntrinsicNumericType())
                return !HasImplicitNumericConversion(sourceType, targetType);

            return false;
        }

        private static ImmutableArray<MethodInfo> GetConversionMethods(Type sourceType, Type targetType, bool isImplicit)
        {
            var methodName = isImplicit ? ImplicitMethodName : ExplicitMethodName;
            var sourceMethods = sourceType.GetMethods(ConversionMethodBindingFlags);
            var targetMethods = targetType.GetMethods(ConversionMethodBindingFlags);
            var methods = sourceMethods.Concat(targetMethods);

            return (from m in methods
                    where m.Name.Equals(methodName, StringComparison.Ordinal) &&
                          HasConversionSignature(m, sourceType, targetType)
                    select m).ToImmutableArray();
        }

        private static bool HasConversionSignature(MethodInfo methodInfo, Type sourceType, Type targetType)
        {
            if (methodInfo.ReturnType != targetType)
                return false;

            var parameterInfos = methodInfo.GetParameters();
            if (parameterInfos.Length != 1)
                return false;

            return parameterInfos[0].ParameterType == sourceType;
        }

        internal static int Compare(Type xType, Conversion xConversion, Type yType, Conversion yConversion)
        {
            if (xConversion.IsIdentity && !yConversion.IsIdentity ||
                xConversion.IsImplicit && yConversion.IsExplicit)
                return -1;

            if (!xConversion.IsIdentity && yConversion.IsIdentity ||
                xConversion.IsExplicit && yConversion.IsImplicit)
                return 1;

            var xTypeToYType = Classify(xType, yType);
            var yTypeToXType = Classify(yType, xType);

            if (xTypeToYType.IsImplicit && yTypeToXType.IsExplicit)
                return -1;

            if (xTypeToYType.IsExplicit && yTypeToXType.IsImplicit)
                return 1;

            var xKnown = xType.GetKnownType();
            var yKnown = yType.GetKnownType();

            if (xKnown is not null && yKnown is not null)
            {
                var x = xKnown.Value;
                var y = yKnown.Value;

                if (x.IsSignedNumericType() && y.IsUnsignedNumericType())
                    return -1;

                if (x.IsUnsignedNumericType() && y.IsSignedNumericType())
                    return 1;
            }

            return 0;
        }
    }
}