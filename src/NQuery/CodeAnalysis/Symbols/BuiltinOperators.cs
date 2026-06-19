using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using NQuery.CodeAnalysis.Binding;

namespace NQuery.CodeAnalysis.Symbols;

internal static class BuiltInOperators
{
    public static MethodInfo StringConcatStringStringMethod { get; } = new Func<string, string, string>(string.Concat).Method;
    public static MethodInfo StringConcatObjectObjectMethod { get; } = new Func<object, object, string>(string.Concat).Method;
    public static MethodInfo SimilarToMethod { get; } = new Func<string, string, bool>(SimilarTo).Method;
    public static MethodInfo LikeMethod { get; } = new Func<string, string, bool>(Like).Method;
    public static MethodInfo SoundsLikeMethod { get; } = new Func<string, string, bool>(SoundsLike).Method;
    public static MethodInfo PowerMethod { get; } = new Func<double, double, double>(Math.Pow).Method;

    public static MethodInfo DecimalAddMethod { get; } = typeof(decimal).GetMethod("op_Addition", new[] { typeof(decimal), typeof(decimal) })!;
    public static MethodInfo DecimalDivideMethod { get; } = typeof(decimal).GetMethod("op_Division", new[] { typeof(decimal), typeof(decimal) })!;
    public static MethodInfo DecimalEqualsMethod { get; } = typeof(decimal).GetMethod("op_Equality", new[] { typeof(decimal), typeof(decimal) })!;
    public static MethodInfo DecimalNotEqualsMethod { get; } = typeof(decimal).GetMethod("op_Inequality", new[] { typeof(decimal), typeof(decimal) })!;
    public static MethodInfo DecimalGreaterMethod { get; } = typeof(decimal).GetMethod("op_GreaterThan", new[] { typeof(decimal), typeof(decimal) })!;
    public static MethodInfo DecimalGreaterOrEqualMethod { get; } = typeof(decimal).GetMethod("op_GreaterThanOrEqual", new[] { typeof(decimal), typeof(decimal) })!;
    public static MethodInfo DecimalLessMethod { get; } = typeof(decimal).GetMethod("op_LessThan", new[] { typeof(decimal), typeof(decimal) })!;
    public static MethodInfo DecimalLessOrEqualMethod { get; } = typeof(decimal).GetMethod("op_LessThanOrEqual", new[] { typeof(decimal), typeof(decimal) })!;
    public static MethodInfo DecimalModulusMethod { get; } = typeof(decimal).GetMethod("op_Modulus", new[] { typeof(decimal), typeof(decimal) })!;
    public static MethodInfo DecimalMultiplyMethod { get; } = typeof(decimal).GetMethod("op_Multiply", new[] { typeof(decimal), typeof(decimal) })!;
    public static MethodInfo DecimalSubtractMethod { get; } = typeof(decimal).GetMethod("op_Subtraction", new[] { typeof(decimal), typeof(decimal) })!;
    public static MethodInfo DecimalUnaryNegationMethod { get; } = typeof(decimal).GetMethod("op_UnaryNegation", new[] { typeof(decimal) })!;
    public static MethodInfo DecimalUnaryIdentityMethod { get; } = typeof(decimal).GetMethod("op_UnaryPlus", new[] { typeof(decimal) })!;

    private static bool SimilarTo(string str, string regex)
    {
        var regex1 = new Regex(regex, RegexOptions.Multiline);
        return regex1.IsMatch(str);
    }

    private static bool Like(string str, string expr)
    {
        expr = Regex.Escape(expr);

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
        if (left is null || right is null)
            return false;

        return Soundex.GetCode(left) == Soundex.GetCode(right);
    }
}
