using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using NQuery.Binding;

namespace NQuery.Symbols
{
    internal static class BuiltInOperators
    {
        public static readonly MethodInfo StringConcatStringStringMethod = new Func<string, string, string>(string.Concat).Method;
        public static readonly MethodInfo StringConcatObjectObjectMethod = new Func<object, object, string>(string.Concat).Method;
        public static readonly MethodInfo SimilarToMethod = new Func<string, string, bool>(SimilarTo).Method;
        public static readonly MethodInfo LikeMethod = new Func<string, string, bool>(Like).Method;
        public static readonly MethodInfo SoundsLikeMethod = new Func<string, string, bool>(SoundsLike).Method;
        public static readonly MethodInfo PowerMethod = new Func<double, double, double>(Math.Pow).Method;

        public static readonly MethodInfo DecimalAddMethod = typeof(decimal).GetMethod("op_Addition", new[] { typeof(decimal), typeof(decimal) });
        public static readonly MethodInfo DecimalDivideMethod = typeof(decimal).GetMethod("op_Division", new[] { typeof(decimal), typeof(decimal) });
        public static readonly MethodInfo DecimalEqualsMethod = typeof(decimal).GetMethod("op_Equality", new[] { typeof(decimal), typeof(decimal) });
        public static readonly MethodInfo DecimalNotEqualsMethod = typeof(decimal).GetMethod("op_Inequality", new[] { typeof(decimal), typeof(decimal) });
        public static readonly MethodInfo DecimalGreaterMethod = typeof(decimal).GetMethod("op_GreaterThan", new[] { typeof(decimal), typeof(decimal) });
        public static readonly MethodInfo DecimalGreaterOrEqualMethod = typeof(decimal).GetMethod("op_GreaterThanOrEqual", new[] { typeof(decimal), typeof(decimal) });
        public static readonly MethodInfo DecimalLessMethod = typeof(decimal).GetMethod("op_LessThan", new[] { typeof(decimal), typeof(decimal) });
        public static readonly MethodInfo DecimalLessOrEqualMethod = typeof(decimal).GetMethod("op_LessThanOrEqual", new[] { typeof(decimal), typeof(decimal) });
        public static readonly MethodInfo DecimalModulusMethod = typeof(decimal).GetMethod("op_Modulus", new[] { typeof(decimal), typeof(decimal) });
        public static readonly MethodInfo DecimalMultiplyMethod = typeof(decimal).GetMethod("op_Multiply", new[] { typeof(decimal), typeof(decimal) });
        public static readonly MethodInfo DecimalSubtractMethod = typeof(decimal).GetMethod("op_Subtraction", new[] { typeof(decimal), typeof(decimal) });
        public static readonly MethodInfo DecimalUnaryNegationMethod = typeof(decimal).GetMethod("op_UnaryNegation", new[] { typeof(decimal) });
        public static readonly MethodInfo DecimalUnaryIdentityMethod = typeof(decimal).GetMethod("op_UnaryPlus", new[] { typeof(decimal) });

        private static bool SimilarTo(string str, string regex)
        {
            var regex1 = new Regex(regex, RegexOptions.Multiline);
            return regex1.IsMatch(str);
        }

        private static bool Like(string str, string expr)
        {
            var sb = new StringBuilder();

            sb.Append('^');

            foreach (var c in expr)
            {
                switch (c)
                {
                    case '%':
                        sb.Append(@".*");
                        break;

                    case '_':
                        sb.Append('.');
                        break;

                    default:
                        sb.Append(c);
                        break;
                }
            }

            sb.Append('$');

            var r = new Regex(sb.ToString(), RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return r.IsMatch(str);
        }

        private static bool SoundsLike(string left, string right)
        {
            if (left == null || right == null)
                return false;

            return Soundex.GetCode(left) == Soundex.GetCode(right);
        }
    }
}