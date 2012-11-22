using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

using NQuery.Binding;
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

        private static readonly bool[,] ImplicitNumericConversions = new[,]
        {
            /*                SByte     Byte     Short     UShort     Int     UInt     Long     ULong     Char     Float     Double      Decimal*/
            /* SByte   */  {  N,        N,       Y,        N,         Y,      N,       Y,       N,        N,       Y,        Y,           Y},
            /* Byte    */  {  N,        N,       Y,        Y,         Y,      Y,       Y,       Y,        N,       Y,        Y,           Y},
            /* Short   */  {  N,        N,       N,        N,         Y,      N,       Y,       N,        N,       Y,        Y,           Y},
            /* UShort  */  {  N,        N,       N,        N,         Y,      Y,       Y,       Y,        N,       Y,        Y,           Y},
            /* Int     */  {  N,        N,       N,        N,         N,      N,       Y,       N,        N,       Y,        Y,           Y},
            /* UInt    */  {  N,        N,       N,        N,         N,      N,       Y,       Y,        N,       Y,        Y,           Y},
            /* Long    */  {  N,        N,       N,        N,         N,      N,       N,       N,        N,       Y,        Y,           Y},
            /* ULong   */  {  N,        N,       N,        N,         N,      N,       N,       N,        N,       Y,        Y,           Y},
            /* Char    */  {  N,        N,       N,        Y,         Y,      Y,       Y,       Y,        N,       Y,        Y,           Y},
            /* Float   */  {  N,        N,       N,        N,         N,      N,       N,       N,        N,       N,        Y,           N},
            /* Double  */  {  N,        N,       N,        N,         N,      N,       N,       N,        N,       N,        N,           N},
            /* Decimal */  {  N,        N,       N,        N,         N,      N,       N,       N,        N,       N,        N,           N}
        };

        private readonly bool _exists;
        private readonly bool _isIdentity;
        private readonly bool _isImplicit;
        private readonly bool _isBoxingOrUnboxing;
        private readonly bool _isReference;
        private readonly ReadOnlyCollection<MethodInfo> _conversionMethods;

        private Conversion(bool exists, bool isIdentity, bool isImplicit, bool isBoxingOrUnboxing, bool isReference, IList<MethodInfo> conversionMethods)
        {
            _exists = exists;
            _isIdentity = isIdentity;
            _isImplicit = isImplicit;
            _isBoxingOrUnboxing = isBoxingOrUnboxing;
            _isReference = isReference;
            _conversionMethods = new ReadOnlyCollection<MethodInfo>(conversionMethods ?? new MethodInfo[0]);
        }

        private static readonly Conversion None = new Conversion(false, false, false, false, false, null);
        private static readonly Conversion Identity = new Conversion(true, true, true, false, false, null);
        private static readonly Conversion Implicit = new Conversion(true, false, true, false, false, null);
        private static readonly Conversion Explicit = new Conversion(true, false, false, false, false, null);
        private static readonly Conversion Boxing = new Conversion(true, false, true, true, false, null);
        private static readonly Conversion Unboxing = new Conversion(true, false, false, true, false, null);
        private static readonly Conversion UpCast = new Conversion(true, false, true, false, true, null);
        private static readonly Conversion DownCast = new Conversion(true, false, false, false, true, null);

        public bool Exists
        {
            get { return _exists; }
        }

        public bool IsIdentity
        {
            get { return _isIdentity; }
        }

        public bool IsImplicit
        {
            get { return _isImplicit; }
        }

        public bool IsExplicit
        {
            get { return Exists && !IsImplicit; }
        }

        public bool IsBoxing
        {
            get { return _isBoxingOrUnboxing &&  _isImplicit; }
        }

        public bool IsUnboxing
        {
            get { return _isBoxingOrUnboxing && IsExplicit; }
        }

        public bool IsReference
        {
            get { return _isReference; }
        }

        public ReadOnlyCollection<MethodInfo> ConversionMethods
        {
            get { return _conversionMethods; }
        }

        internal static Conversion Classify(Type sourceType, Type targetType)
        {
            if (sourceType == targetType)
                return Identity;

            var knownSourceType = sourceType.GetKnownType();
            var knownTargetType = targetType.GetKnownType();

            if (knownSourceType != null && knownTargetType != null)
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

            if (!sourceType.IsValueType && !targetType.IsValueType)
            {
                // If both a reference types, let's check whether target is a base type
                // of source. In that case it's an implict upcast.
                if (targetType.IsAssignableFrom(sourceType))
                    return UpCast;

                // The revers would be an explicit downcast.
                if (sourceType.IsAssignableFrom(targetType))
                    return DownCast;
            }

            var implicitConversions = GetConversionMethods(sourceType, targetType, true);
            if (implicitConversions.Length > 0)
                return ImplicitViaConversionMethod(implicitConversions);

            var explicitConversions = GetConversionMethods(sourceType, targetType, false);
            if (explicitConversions.Length > 0)
                return ExplicitViaConversionMethod(explicitConversions);

            return None;
        }

        private static Conversion ImplicitViaConversionMethod(MethodInfo[] implicitConversions)
        {
            return new Conversion(true, false, true, false, false, implicitConversions);
        }

        private static Conversion ExplicitViaConversionMethod(MethodInfo[] implicitConversions)
        {
            return new Conversion(true, false, false, false, false, implicitConversions);
        }

        private static bool HasImplicitNumericConversion(KnownType sourceType, KnownType targetType)
        {
            if (sourceType.IsNumericType() && targetType.IsNumericType())
            {
                var sourceIndex = (int) sourceType;
                var targetIndex = (int) targetType;
                return ImplicitNumericConversions[sourceIndex, targetIndex];
            }
            
            return false;
        }

        private static bool HasExplicitNumericConversion(KnownType sourceType, KnownType targetType)
        {
            if (sourceType.IsNumericType() && targetType.IsNumericType())
                return !HasImplicitNumericConversion(sourceType, targetType);

            return false;
        }

        private static MethodInfo[] GetConversionMethods(Type sourceType, Type targetType, bool isImplicit)
        {
            var methodName = isImplicit ? ImplicitMethodName : ExplicitMethodName;
            var sourceMethods = sourceType.GetMethods(ConversionMethodBindingFlags);
            var targetMethods = targetType.GetMethods(ConversionMethodBindingFlags);
            var methods = sourceMethods.Concat(targetMethods);

            return (from m in methods
                    where m.Name.Equals(methodName, StringComparison.Ordinal) &&
                          IsConversionMethods(m, sourceType, targetType)
                    select m).ToArray();
        }

        private static bool IsConversionMethods(MethodInfo methodInfo, Type sourceType, Type targetType)
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

            if (xKnown != null && yKnown != null)
            {
                var x = xKnown.Value;
                var y = yKnown.Value;
                if (x.IsNumericType() && y.IsNumericType())
                {
                    if (x.IsSignedNumericType() && y.IsUnsignedNumericType())
                        return -1;

                    if (x.IsUnsignedNumericType() && y.IsSignedNumericType())
                        return 1;
                }
            }

            return 0;
        }
    }
}