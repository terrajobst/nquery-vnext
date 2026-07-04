using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using NQuery.CodeAnalysis.Binding;

namespace NQuery.Metadata;

internal static class BuiltInFunctions
{
    public static ImmutableArray<FunctionDefinition> Functions { get; } =
    [
        FunctionDefinition.Create<object, bool>(@"TO_BOOLEAN", value => ToBoolean(value)),
        FunctionDefinition.Create<object, string, bool>(@"TO_BOOLEAN", (value, culture) => ToBoolean(value, culture)),
        FunctionDefinition.Create<object, byte>(@"TO_BYTE", value => ToByte(value)),
        FunctionDefinition.Create<object, string, byte>(@"TO_BYTE", (value, culture) => ToByte(value, culture)),
        FunctionDefinition.Create<object, char>(@"TO_CHAR", value => ToChar(value)),
        FunctionDefinition.Create<object, string, char>(@"TO_CHAR", (value, culture) => ToChar(value, culture)),
        FunctionDefinition.Create<object, DateTime>(@"TO_DATETIME", value => ToDateTime(value)),
        FunctionDefinition.Create<object, string, DateTime>(@"TO_DATETIME", (value, culture) => ToDateTime(value, culture)),
        FunctionDefinition.Create<object, decimal>(@"TO_DECIMAL", value => ToDecimal(value)),
        FunctionDefinition.Create<object, string, decimal>(@"TO_DECIMAL", (value, culture) => ToDecimal(value, culture)),
        FunctionDefinition.Create<object, double>(@"TO_DOUBLE", value => ToDouble(value)),
        FunctionDefinition.Create<object, string, double>(@"TO_DOUBLE", (value, culture) => ToDouble(value, culture)),
        FunctionDefinition.Create<object, short>(@"TO_INT16", value => ToInt16(value)),
        FunctionDefinition.Create<object, string, short>(@"TO_INT16", (value, culture) => ToInt16(value, culture)),
        FunctionDefinition.Create<object, int>(@"TO_INT32", value => ToInt32(value)),
        FunctionDefinition.Create<object, string, int>(@"TO_INT32", (value, culture) => ToInt32(value, culture)),
        FunctionDefinition.Create<object, long>(@"TO_INT64", value => ToInt64(value)),
        FunctionDefinition.Create<object, string, long>(@"TO_INT64", (value, culture) => ToInt64(value, culture)),
        FunctionDefinition.Create<object, sbyte>(@"TO_SBYTE", value => ToSByte(value)),
        FunctionDefinition.Create<object, string, sbyte>(@"TO_SBYTE", (value, culture) => ToSByte(value, culture)),
        FunctionDefinition.Create<object, float>(@"TO_SINGLE", value => ToSingle(value)),
        FunctionDefinition.Create<object, string, float>(@"TO_SINGLE", (value, culture) => ToSingle(value, culture)),
        FunctionDefinition.Create<object, string?>(@"TO_STRING", value => ToString(value)),
        FunctionDefinition.Create<object, string, string?>(@"TO_STRING", (value, culture) => ToString(value, culture)),
        FunctionDefinition.Create<object, ushort>(@"TO_UINT16", value => ToUInt16(value)),
        FunctionDefinition.Create<object, string, ushort>(@"TO_UINT16", (value, culture) => ToUInt16(value, culture)),
        FunctionDefinition.Create<object, uint>(@"TO_UINT32", value => ToUInt32(value)),
        FunctionDefinition.Create<object, string, uint>(@"TO_UINT32", (value, culture) => ToUInt32(value, culture)),
        FunctionDefinition.Create<object, ulong>(@"TO_UINT64", value => ToUInt64(value)),
        FunctionDefinition.Create<object, string, ulong>(@"TO_UINT64", (value, culture) => ToUInt64(value, culture)),

        FunctionDefinition.Create<decimal, decimal>(@"ABS", value => Math.Abs(value)),
        FunctionDefinition.Create<double, double>(@"ABS", value => Math.Abs(value)),
        FunctionDefinition.Create<float, float>(@"ABS", value => Math.Abs(value)),
        FunctionDefinition.Create<long, long>(@"ABS", value => Math.Abs(value)),
        FunctionDefinition.Create<int, int>(@"ABS", value => Math.Abs(value)),
        FunctionDefinition.Create<short, short>(@"ABS", value => Math.Abs(value)),
        FunctionDefinition.Create<sbyte, sbyte>(@"ABS", value => Math.Abs(value)),
        FunctionDefinition.Create<double, double>(@"ACOS", value => Math.Acos(value)),
        FunctionDefinition.Create<double, double>(@"ASIN", value => Math.Asin(value)),
        FunctionDefinition.Create<double, double>(@"ATAN", value => Math.Atan(value)),
        FunctionDefinition.Create<double, double, double>(@"ATAN2", (y, x) => Math.Atan2(y, x)),
        FunctionDefinition.Create<double, double>(@"CEILING", value => Math.Ceiling(value)),
        FunctionDefinition.Create<decimal, decimal>(@"CEILING", value => Math.Ceiling(value)),
        FunctionDefinition.Create<double, double>(@"COS", value => Math.Cos(value)),
        FunctionDefinition.Create<double, double>(@"COSH", value => Math.Cosh(value)),
        FunctionDefinition.Create<double, double>(@"EXP", value => Math.Exp(value)),
        FunctionDefinition.Create<double, double>(@"FLOOR", value => Math.Floor(value)),
        FunctionDefinition.Create<double, double>(@"ROUND", value => Round(value)),
        FunctionDefinition.Create<double, int, double>(@"ROUND", (value, digits) => Round(value, digits)),
        FunctionDefinition.Create<decimal, decimal>(@"ROUND", value => Round(value)),
        FunctionDefinition.Create<decimal, int, decimal>(@"ROUND", (value, digits) => Round(value, digits)),
        FunctionDefinition.Create<double, double>(@"LOG", value => Math.Log(value)),
        FunctionDefinition.Create<double, double, double>(@"LOG", (value, newBase) => Math.Log(value, newBase)),
        FunctionDefinition.Create<double, double>(@"LOG10", value => Math.Log10(value)),
        FunctionDefinition.Create<double, double>(@"SIN", value => Math.Sin(value)),
        FunctionDefinition.Create<double, double>(@"SINH", value => Math.Sinh(value)),
        FunctionDefinition.Create<double, double>(@"SQRT", value => Math.Sqrt(value)),
        FunctionDefinition.Create<double, double>(@"TAN", value => Math.Tan(value)),
        FunctionDefinition.Create<double, double>(@"TANH", value => Math.Tanh(value)),
        FunctionDefinition.Create<double, double, double>(@"POW", (basis, exponent) => Math.Pow(basis, exponent)),
        FunctionDefinition.Create<decimal, int>(@"SIGN", basis => Math.Sign(basis)),
        FunctionDefinition.Create<double, int>(@"SIGN", basis => Math.Sign(basis)),
        FunctionDefinition.Create<float, int>(@"SIGN", basis => Math.Sign(basis)),
        FunctionDefinition.Create<long, int>(@"SIGN", basis => Math.Sign(basis)),
        FunctionDefinition.Create<int, int>(@"SIGN", basis => Math.Sign(basis)),
        FunctionDefinition.Create<short, int>(@"SIGN", basis => Math.Sign(basis)),
        FunctionDefinition.Create<sbyte, int>(@"SIGN", basis => Math.Sign(basis)),

        FunctionDefinition.Create<string, string?>(@"SOUNDEX", text => GetSoundexCode(text)),
        FunctionDefinition.Create<string, int>(@"LEN", text => StringLength(text)),
        FunctionDefinition.Create<string, string, int>(@"CHARINDEX", (chars, text) => CharIndex(chars, text)),
        FunctionDefinition.Create<string, int, int, string?>(@"SUBSTRING", (text, start, length) => Substring(text, start, length)),
        FunctionDefinition.Create<string, int, string?>(@"SUBSTRING", (text, start) => Substring(text, start)),
        FunctionDefinition.Create<string, string?>(@"UPPER", text => Upper(text)),
        FunctionDefinition.Create<string, string, string?>(@"UPPER", (text, culture) => Upper(text, culture)),
        FunctionDefinition.Create<string, string?>(@"LOWER", text => Lower(text)),
        FunctionDefinition.Create<string, string, string?>(@"LOWER", (text, culture) => Lower(text, culture)),
        FunctionDefinition.Create<string, string?>(@"TRIM", text => Trim(text)),
        FunctionDefinition.Create<string, string?>(@"LTRIM", text => LTrim(text)),
        FunctionDefinition.Create<string, string?>(@"RTRIM", text => RTrim(text)),
        FunctionDefinition.Create<string, string, string, string?>(@"REPLACE", (text, oldValue, newValue) => Replace(text, oldValue, newValue)),
        FunctionDefinition.Create<string, string, string, string?>(@"REGEX_REPLACE", (text, pattern, replacementPattern) => RegexReplace(text, pattern, replacementPattern)),
        FunctionDefinition.Create<string, string, bool>(@"REGEX_MATCH", (text, pattern) => RegexMatch(text, pattern)),
        FunctionDefinition.Create<string, string?>(@"REGEX_ESCAPE", text => RegexEscape(text)),
        FunctionDefinition.Create<string, string?>(@"REGEX_UNESCAPE", text => RegexUnescape(text)),
        FunctionDefinition.Create<object, string, string?>(@"FORMAT", (value, format) => Format(value, format)),
        FunctionDefinition.Create<object, string, string, string?>(@"FORMAT", (value, format, culture) => Format(value, format, culture)),
        FunctionDefinition.Create<string, int, string?>(@"REPLICATE", (text, count) => Replicate(text, count)),
        FunctionDefinition.Create<string, string?>(@"REVERSE", text => Reverse(text)),
        FunctionDefinition.Create<string, int, string?>(@"LEFT", (text, numberOfChars) => Left(text, numberOfChars)),
        FunctionDefinition.Create<string, int, string?>(@"RIGHT", (text, numberOfChars) => Right(text, numberOfChars)),
        FunctionDefinition.Create<int, string?>(@"SPACE", numberOfSpaces => Space(numberOfSpaces)),
        FunctionDefinition.Create<string, int, string?>(@"LPAD", (text, totalWidth) => LPad(text, totalWidth)),
        FunctionDefinition.Create<string, int, string?>(@"RPAD", (text, totalWidth) => RPad(text, totalWidth)),

        FunctionDefinition.Create<DateTime>(@"GETDATE", () => GetDate()),
        FunctionDefinition.Create<DateTime>(@"GETUTCDATE", () => GetUtcDate()),
        FunctionDefinition.Create<DateTime, int>(@"DAY", dateTime => GetDay(dateTime)),
        FunctionDefinition.Create<DateTime, int>(@"MONTH", dateTime => GetMonth(dateTime)),
        FunctionDefinition.Create<DateTime, int>(@"YEAR", dateTime => GetYear(dateTime))
    ];

    private static bool ToBoolean(object value)
    {
        if (value is null)
            return false;

        return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
    }

    private static bool ToBoolean(object value, string culture)
    {
        if (value is null)
            return false;

        return Convert.ToBoolean(value, GetCultureInfo(culture));
    }

    private static byte ToByte(object value)
    {
        if (value is null)
            return 0;

        return Convert.ToByte(value, CultureInfo.InvariantCulture);
    }

    private static byte ToByte(object value, string culture)
    {
        if (value is null)
            return 0;

        return Convert.ToByte(value, GetCultureInfo(culture));
    }

    private static char ToChar(object value)
    {
        if (value is null)
            return (char)0;

        return Convert.ToChar(value, CultureInfo.InvariantCulture);
    }

    private static char ToChar(object value, string culture)
    {
        if (value is null)
            return (char)0;

        return Convert.ToChar(value, GetCultureInfo(culture));
    }

    private static DateTime ToDateTime(object value)
    {
        if (value is null)
            return DateTime.MinValue;

        return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
    }

    private static DateTime ToDateTime(object value, string culture)
    {
        if (value is null)
            return DateTime.MinValue;

        return Convert.ToDateTime(value, GetCultureInfo(culture));
    }

    private static decimal ToDecimal(object value)
    {
        if (value is null)
            return 0;

        return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
    }

    private static decimal ToDecimal(object value, string culture)
    {
        if (value is null)
            return 0;

        return Convert.ToDecimal(value, GetCultureInfo(culture));
    }

    private static double ToDouble(object value)
    {
        if (value is null)
            return 0;

        return Convert.ToDouble(value, CultureInfo.InvariantCulture);
    }

    private static double ToDouble(object value, string culture)
    {
        if (value is null)
            return 0;

        return Convert.ToDouble(value, GetCultureInfo(culture));
    }

    private static short ToInt16(object value)
    {
        if (value is null)
            return 0;

        return Convert.ToInt16(value, CultureInfo.InvariantCulture);
    }

    private static short ToInt16(object value, string culture)
    {
        if (value is null)
            return 0;

        return Convert.ToInt16(value, GetCultureInfo(culture));
    }

    private static int ToInt32(object value)
    {
        if (value is null)
            return 0;

        return Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    private static int ToInt32(object value, string culture)
    {
        if (value is null)
            return 0;

        return Convert.ToInt32(value, GetCultureInfo(culture));
    }

    private static long ToInt64(object value)
    {
        if (value is null)
            return 0;

        return Convert.ToInt64(value, CultureInfo.InvariantCulture);
    }

    private static long ToInt64(object value, string culture)
    {
        if (value is null)
            return 0;

        return Convert.ToInt64(value, GetCultureInfo(culture));
    }

    private static sbyte ToSByte(object value)
    {
        if (value is null)
            return 0;

        return Convert.ToSByte(value, CultureInfo.InvariantCulture);
    }

    private static sbyte ToSByte(object value, string culture)
    {
        if (value is null)
            return 0;

        return Convert.ToSByte(value, GetCultureInfo(culture));
    }

    private static float ToSingle(object value)
    {
        if (value is null)
            return 0;

        return Convert.ToSingle(value, CultureInfo.InvariantCulture);
    }

    private static float ToSingle(object value, string culture)
    {
        if (value is null)
            return 0;

        return Convert.ToSingle(value, GetCultureInfo(culture));
    }

    private static string? ToString(object value)
    {
        if (value is null)
            return null;

        return Convert.ToString(value, CultureInfo.InvariantCulture);
    }

    private static string? ToString(object value, string culture)
    {
        if (value is null)
            return null;

        return Convert.ToString(value, GetCultureInfo(culture));
    }

    private static ushort ToUInt16(object value)
    {
        if (value is null)
            return 0;

        return Convert.ToUInt16(value, CultureInfo.InvariantCulture);
    }

    private static ushort ToUInt16(object value, string culture)
    {
        if (value is null)
            return 0;

        return Convert.ToUInt16(value, GetCultureInfo(culture));
    }

    private static uint ToUInt32(object value)
    {
        if (value is null)
            return 0;

        return Convert.ToUInt32(value, CultureInfo.InvariantCulture);
    }

    private static uint ToUInt32(object value, string culture)
    {
        if (value is null)
            return 0;

        return Convert.ToUInt32(value, GetCultureInfo(culture));
    }

    private static ulong ToUInt64(object value)
    {
        if (value is null)
            return 0;

        return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
    }

    private static ulong ToUInt64(object value, string culture)
    {
        if (value is null)
            return 0;

        return Convert.ToUInt64(value, GetCultureInfo(culture));
    }

    private static double Round(double v)
    {
        return Math.Round(v, MidpointRounding.AwayFromZero);
    }

    private static double Round(double v, int decimals)
    {
        return Math.Round(v, decimals, MidpointRounding.AwayFromZero);
    }

    private static decimal Round(decimal v)
    {
        return Math.Round(v, MidpointRounding.AwayFromZero);
    }

    private static decimal Round(decimal v, int decimals)
    {
        return Math.Round(v, decimals, MidpointRounding.AwayFromZero);
    }

    private static string? GetSoundexCode(string text)
    {
        if (text is null)
            return null;

        return Soundex.GetCode(text);
    }

    private static int StringLength(string text)
    {
        if (text is null)
            return 0;

        return text.Length;
    }

    private static int CharIndex(string chars, string text)
    {
        if (chars is null || text is null)
            return 0;

        if (chars.Length == 0 || text.Length == 0)
            return 0;

        return text.IndexOf(chars, StringComparison.Ordinal) + 1;
    }

    private static string? Substring(string text, int start, int length)
    {
        if (text is null)
            return null;

        if (start == 0 || text.Length == 0)
            return string.Empty;

        if (start > text.Length)
            return string.Empty;

        if (start + length - 1 > text.Length)
            length = text.Length - start + 1;

        return text.Substring(start - 1, length);
    }

    private static string? Substring(string text, int start)
    {
        if (text is null)
            return null;

        return Substring(text, start, text.Length);
    }

    private static string? Upper(string text)
    {
        return text?.ToUpper(CultureInfo.InvariantCulture);
    }

    private static string? Upper(string text, string culture)
    {
        return text?.ToUpper(GetCultureInfo(culture));
    }

    private static string? Lower(string text)
    {
        return text?.ToLower(CultureInfo.InvariantCulture);
    }

    private static string? Lower(string text, string culture)
    {
        return text?.ToLower(GetCultureInfo(culture));
    }

    private static string? Trim(string text)
    {
        return text?.Trim();
    }

    private static string? LTrim(string text)
    {
        return text?.TrimStart(' ', '\t');
    }

    private static string? RTrim(string text)
    {
        return text?.TrimEnd(' ', '\t');
    }

    private static string? Replace(string text, string oldValue, string newValue)
    {
        if (text is null || oldValue is null || newValue is null)
            return null;

        return text.Replace(oldValue, newValue);
    }

    private static string? RegexReplace(string text, string pattern, string replacementPattern)
    {
        if (text is null || pattern is null || replacementPattern is null)
            return null;

        return Regex.Replace(text, pattern, replacementPattern);
    }

    private static bool RegexMatch(string text, string pattern)
    {
        if (text is null || pattern is null)
            return false;

        return Regex.IsMatch(text, pattern);
    }

    private static string? RegexEscape(string text)
    {
        if (text is null)
            return null;

        return Regex.Escape(text);
    }

    private static string? RegexUnescape(string text)
    {
        if (text is null)
            return null;

        return Regex.Unescape(text);
    }

    private static string? Format(object value, string format)
    {
        return Format(value, format, CultureInfo.InvariantCulture);
    }

    private static string? Format(object value, string format, string culture)
    {
        return Format(value, format, GetCultureInfo(culture));
    }

    private static string? Format(object value, string format, IFormatProvider formatProvider)
    {
        var embeddedFormatString = string.Format(CultureInfo.InvariantCulture, @"{{0:{0}}}", format);
        return string.Format(formatProvider, embeddedFormatString, value);
    }

    private static string? Replicate(string text, int count)
    {
        if (text is null)
            return null;

        var sb = new StringBuilder(text.Length * count);
        for (var i = 0; i < count; i++)
            sb.Append(text);
        return sb.ToString();
    }

    private static string? Reverse(string text)
    {
        if (text is null)
            return null;

        var sb = new StringBuilder(text.Length);
        for (var i = text.Length - 1; i >= 0; i--)
            sb.Append(text[i]);
        return sb.ToString();
    }

    private static string? Left(string text, int numberOfChars)
    {
        if (text is null)
            return null;

        if (numberOfChars > text.Length)
            numberOfChars = text.Length;

        return text[..numberOfChars];
    }

    private static string? Right(string text, int numberOfChars)
    {
        if (text is null)
            return null;

        if (numberOfChars > text.Length)
            numberOfChars = text.Length;

        return text[^numberOfChars..];
    }

    private static string? Space(int numberOfSpaces)
    {
        if (numberOfSpaces <= 0)
            return string.Empty;

        return Replicate(@" ", numberOfSpaces);
    }

    private static string? LPad(string text, int totalWidth)
    {
        return text?.PadLeft(totalWidth);
    }

    private static string? RPad(string text, int totalWidth)
    {
        return text?.PadRight(totalWidth);
    }

    private static DateTime GetDate()
    {
        return DateTime.Now;
    }

    private static DateTime GetUtcDate()
    {
        return DateTime.UtcNow;
    }

    private static int GetDay(DateTime dateTime)
    {
        return dateTime.Day;
    }

    private static int GetMonth(DateTime dateTime)
    {
        return dateTime.Month;
    }

    private static int GetYear(DateTime dateTime)
    {
        return dateTime.Year;
    }

    private static CultureInfo GetCultureInfo(string culture)
    {
        ThrowIfNull(culture);

        return CultureInfo.GetCultureInfo(culture);
    }
}
