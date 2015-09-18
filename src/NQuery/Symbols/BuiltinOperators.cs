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

        public static readonly MethodInfo DecimalAddMethod = new Func<decimal, decimal, decimal>(decimal.Add).Method;
        public static readonly MethodInfo DecimalDivideMethod = new Func<decimal, decimal, decimal>(decimal.Divide).Method;
        public static readonly MethodInfo DecimalEqualsMethod = new Func<decimal, decimal, bool>(decimal.Equals).Method;
        public static readonly MethodInfo DecimalNotEqualsMethod = new Func<decimal, decimal, bool>((x, y) => x != y).Method;
        public static readonly MethodInfo DecimalGreaterMethod = new Func<decimal, decimal, bool>((x, y) => x > y).Method;
        public static readonly MethodInfo DecimalGreaterOrEqualMethod = new Func<decimal, decimal, bool>((x, y) => x >= y).Method;
        public static readonly MethodInfo DecimalLessMethod = new Func<decimal, decimal, bool>((x, y) => x < y).Method;
        public static readonly MethodInfo DecimalLessOrEqualMethod = new Func<decimal, decimal, bool>((x, y) => x <= y).Method;
        public static readonly MethodInfo DecimalModulusMethod = new Func<decimal, decimal, decimal>(decimal.Remainder).Method;
        public static readonly MethodInfo DecimalMultiplyMethod = new Func<decimal, decimal, decimal>(decimal.Multiply).Method;
        public static readonly MethodInfo DecimalSubtractMethod = new Func<decimal, decimal, decimal>(decimal.Subtract).Method;
        public static readonly MethodInfo DecimalUnaryNegationMethod = new Func<decimal, decimal>(decimal.Negate).Method;
        public static readonly MethodInfo DecimalUnaryIdentityMethod = new Func<decimal, decimal>(d => d).Method;

        private static bool SimilarTo(string str, string regex)
        {
            var regex1 = new Regex(regex, RegexOptions.Multiline);
            return regex1.IsMatch(str);
        }

        private static bool Like(string str, string expr)
        {
            str = str.ToUpper(CultureInfo.CurrentCulture);

            var sb = new StringBuilder();

            sb.Append('^');

            foreach (var c in expr)
            {
                switch (c)
                {
                    case '%':
                        sb.Append(".*");
                        break;

                    case '_':
                        sb.Append('.');
                        break;

                    default:
                        sb.Append(char.ToUpper(c, CultureInfo.CurrentCulture));
                        break;
                }
            }

            sb.Append('$');

            var r = new Regex(sb.ToString(), RegexOptions.Singleline);
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