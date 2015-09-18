using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using NQuery.Binding;

namespace NQuery.Symbols
{
    internal static class BuiltInFunctions
    {
        public static IEnumerable<FunctionSymbol> GetFunctions()
        {
            return new FunctionSymbol[]
            {
                #region Conversion
                new FunctionSymbol<object, bool>("TO_BOOLEAN", "value", ToBoolean),
                new FunctionSymbol<object, byte>("TO_BYTE", "value", ToByte),
                new FunctionSymbol<object, char>("TO_CHAR", "value", ToChar),
                new FunctionSymbol<object, DateTime>("TO_DATETIME", "value", ToDateTime),
                new FunctionSymbol<object, decimal>("TO_DECIMAL", "value", ToDecimal),
                new FunctionSymbol<object, double>("TO_DOUBLE", "value", ToDouble),
                new FunctionSymbol<object, short>("TO_INT16", "value", ToInt16),
                new FunctionSymbol<object, int>("TO_INT32", "value", ToInt32),
                new FunctionSymbol<object, long>("TO_INT64", "value", ToInt64),
                new FunctionSymbol<object, sbyte>("TO_SBYTE", "value", ToSByte),
                new FunctionSymbol<object, float>("TO_SINGLE", "value", ToSingle),
                new FunctionSymbol<object, string>("TO_STRING", "value", ToString),
                new FunctionSymbol<object, ushort>("TO_UINT16", "value", ToUInt16),
                new FunctionSymbol<object, uint>("TO_UINT32", "value", ToUInt32),
                new FunctionSymbol<object, ulong>("TO_UINT64", "value", ToUInt64),
                #endregion

                #region Math
                new FunctionSymbol<decimal, decimal>("ABS", "value", Math.Abs),
                new FunctionSymbol<double, double>("ABS", "value", Math.Abs),
                new FunctionSymbol<float, float>("ABS", "value", Math.Abs),
                new FunctionSymbol<long, long>("ABS", "value", Math.Abs),
                new FunctionSymbol<int, int>("ABS", "value", Math.Abs),
                new FunctionSymbol<short, short>("ABS", "value", Math.Abs),
                new FunctionSymbol<sbyte, sbyte>("ABS", "value", Math.Abs),
                new FunctionSymbol<double, double>("ACOS", "value", Math.Acos),
                new FunctionSymbol<double, double>("ASIN", "value", Math.Asin),
                new FunctionSymbol<double, double>("ATAN", "value", Math.Atan),
                new FunctionSymbol<double, double, double>("ATAN2", "y", "x", Math.Atan2),
                new FunctionSymbol<double, double>("CEILING", "value", Math.Ceiling),
                new FunctionSymbol<decimal, decimal>("CEILING", "value", Math.Ceiling),
                new FunctionSymbol<double, double>("COS", "value", Math.Cos),
                new FunctionSymbol<double, double>("COSH", "value", Math.Cosh),
                new FunctionSymbol<double, double>("EXP", "value", Math.Exp),
                new FunctionSymbol<double, double>("FLOOR", "value", Math.Floor),
                new FunctionSymbol<double, double>("ROUND", "value", v => Math.Round(v, MidpointRounding.AwayFromZero)),
                new FunctionSymbol<double, int, double>("ROUND", "value", "digits", (v, d) => Math.Round(v, d, MidpointRounding.AwayFromZero)),
                new FunctionSymbol<decimal, decimal>("ROUND", "value", v => Math.Round(v, MidpointRounding.AwayFromZero)),
                new FunctionSymbol<decimal, int, decimal>("ROUND", "value", "digits", (v, d) => Math.Round(v, d, MidpointRounding.AwayFromZero)),
                new FunctionSymbol<double, double>("LOG", "value", Math.Log),
                new FunctionSymbol<double, double, double>("LOG", "value", "newBase", Math.Log),
                new FunctionSymbol<double, double>("LOG10", "value", Math.Log10),
                new FunctionSymbol<double, double>("SIN", "value", Math.Sin),
                new FunctionSymbol<double, double>("SINH", "value", Math.Sinh),
                new FunctionSymbol<double, double>("SQRT", "value", Math.Sqrt),
                new FunctionSymbol<double, double>("TAN", "value", Math.Tan),
                new FunctionSymbol<double, double>("TANH", "value", Math.Tanh),
                new FunctionSymbol<double, double, double>("POW", "basis", "exponent", Math.Pow),
                new FunctionSymbol<decimal, int>("SIGN", "basis", Math.Sign),
                new FunctionSymbol<double, int>("SIGN", "basis", Math.Sign),
                new FunctionSymbol<float, int>("SIGN", "basis", Math.Sign),
                new FunctionSymbol<long, int>("SIGN", "basis", Math.Sign),
                new FunctionSymbol<int, int>("SIGN", "basis", Math.Sign),
                new FunctionSymbol<short, int>("SIGN", "basis", Math.Sign),
                new FunctionSymbol<sbyte, int>("SIGN", "basis", Math.Sign),
                #endregion

                #region String
                new FunctionSymbol<string, string>("SOUNDEX", "text", GetSoundexCode),
                new FunctionSymbol<string, int>("LEN", "text", StringLength),
                new FunctionSymbol<string, string, int>("CHARINDEX", "chars", "text", CharIndex),
                new FunctionSymbol<string, int, int, string>("SUBSTRING", "text", "start", "length", Substring),
                new FunctionSymbol<string, int, string>("SUBSTRING", "text", "start", Substring),
                new FunctionSymbol<string, string>("UPPER", "text", Upper),
                new FunctionSymbol<string, string>("LOWER", "text", Lower),
                new FunctionSymbol<string, string>("TRIM", "text", Trim),
                new FunctionSymbol<string, string>("LTRIM", "text", LTrim),
                new FunctionSymbol<string, string>("RTRIM", "text", RTrim),
                new FunctionSymbol<string, string, string, string>("REPLACE", "text", "oldValue", "newValue", Replace),
                new FunctionSymbol<string, string, string, string>("REGEX_REPLACE", "text", "pattern", "replacementPattern", RegexReplace),
                new FunctionSymbol<string, string, bool>("REGEX_MATCH", "text", "pattern", RegexMatch),
                new FunctionSymbol<string, string>("REGEX_ESCAPE", "text", RegexEscape),
                new FunctionSymbol<string, string>("REGEX_UNESCAPE", "text", RegexUnescape),
                new FunctionSymbol<object, string, string>("FORMAT", "value", "format", Format),
                new FunctionSymbol<string, int, string>("REPLICATE", "text", "count", Replicate),
                new FunctionSymbol<string, string>("REVERSE", "text", Reverse),
                new FunctionSymbol<string, int, string>("LEFT", "text", "numberOfChars", Left),
                new FunctionSymbol<string, int, string>("RIGHT", "text", "numberOfChars", Right),
                new FunctionSymbol<int, string>("SPACE", "numberOfSpaces", Space),
                new FunctionSymbol<string, int, string>("LPAD", "text", "totalWidth", LPad),
                new FunctionSymbol<string, int, string>("RPAD", "text", "totalWidth", RPad),
                #endregion

                #region Date
                new FunctionSymbol<DateTime>("GETDATE", GetDate),
                new FunctionSymbol<DateTime>("GETUTCDATE", GetUtcDate),
                new FunctionSymbol<DateTime, int>("DAY", "dateTime", GetDay),
                new FunctionSymbol<DateTime, int>("MONTH", "dateTime", GetMonth),
                new FunctionSymbol<DateTime, int>("YEAR", "dateTime", GetYear)
                #endregion
            };
        }

        #region Conversion

        private static bool ToBoolean(object value)
        {
            if (value == null)
                return false;

            return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        private static byte ToByte(object value)
        {
            if (value == null)
                return 0;

            return Convert.ToByte(value, CultureInfo.InvariantCulture);
        }

        private static char ToChar(object value)
        {
            if (value == null)
                return (char)0;

            return Convert.ToChar(value, CultureInfo.InvariantCulture);
        }

        private static DateTime ToDateTime(object value)
        {
            if (value == null)
                return DateTime.MinValue;

            return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }

        private static decimal ToDecimal(object value)
        {
            if (value == null)
                return 0;

            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        private static double ToDouble(object value)
        {
            if (value == null)
                return 0;

            return Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        private static short ToInt16(object value)
        {
            if (value == null)
                return 0;

            return Convert.ToInt16(value, CultureInfo.InvariantCulture);
        }

        private static int ToInt32(object value)
        {
            if (value == null)
                return 0;

            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        private static long ToInt64(object value)
        {
            if (value == null)
                return 0;

            return Convert.ToInt64(value, CultureInfo.InvariantCulture);
        }

        private static sbyte ToSByte(object value)
        {
            if (value == null)
                return 0;

            return Convert.ToSByte(value, CultureInfo.InvariantCulture);
        }

        private static float ToSingle(object value)
        {
            if (value == null)
                return 0;

            return Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        private static string ToString(object value)
        {
            if (value == null)
                return null;

            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static ushort ToUInt16(object value)
        {
            if (value == null)
                return 0;

            return Convert.ToUInt16(value, CultureInfo.InvariantCulture);
        }

        private static uint ToUInt32(object value)
        {
            if (value == null)
                return 0;

            return Convert.ToUInt32(value, CultureInfo.InvariantCulture);
        }

        private static ulong ToUInt64(object value)
        {
            if (value == null)
                return 0;

            return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
        }

        #endregion

        #region String

        private static string GetSoundexCode(string text)
        {
            if (text == null)
                return null;

            return Soundex.GetCode(text);
        }

        private static int StringLength(string text)
        {
            if (text == null)
                return 0;

            return text.Length;
        }

        private static int CharIndex(string chars, string text)
        {
            if (chars == null || text == null)
                return 0;

            if (chars.Length == 0 || text.Length == 0)
                return 0;

            return text.IndexOf(chars, StringComparison.CurrentCulture) + 1;
        }

        private static string Substring(string text, int start, int length)
        {
            if (text == null)
                return null;

            if (start == 0 || text.Length == 0)
                return string.Empty;

            if (start > text.Length)
                return string.Empty;

            if (start + length - 1 > text.Length)
                length = text.Length - start + 1;

            return text.Substring(start - 1, length);
        }

        private static string Substring(string text, int start)
        {
            if (text == null)
                return null;

            return Substring(text, start, text.Length);
        }

        private static string Upper(string text)
        {
            if (text == null)
                return null;

            return text.ToUpper(CultureInfo.CurrentCulture);
        }

        private static string Lower(string text)
        {
            if (text == null)
                return null;

            return text.ToLower(CultureInfo.CurrentCulture);
        }

        private static string Trim(string text)
        {
            if (text == null)
                return null;

            return text.Trim();
        }

        private static string LTrim(string text)
        {
            if (text == null)
                return null;

            return text.TrimStart(' ', '\t');
        }

        private static string RTrim(string text)
        {
            if (text == null)
                return null;

            return text.TrimEnd(' ', '\t');
        }

        private static string Replace(string text, string oldValue, string newValue)
        {
            if (text == null || oldValue == null || newValue == null)
                return null;

            return text.Replace(oldValue, newValue);
        }

        private static string RegexReplace(string text, string pattern, string replacementPattern)
        {
            if (text == null || pattern == null || replacementPattern == null)
                return null;

            return Regex.Replace(pattern, text, replacementPattern);
        }

        private static bool RegexMatch(string text, string pattern)
        {
            if (text == null || pattern == null)
                return false;

            return Regex.IsMatch(pattern, text);
        }

        private static string RegexEscape(string text)
        {
            if (text == null)
                return null;

            return Regex.Escape(text);
        }

        private static string RegexUnescape(string text)
        {
            if (text == null)
                return null;

            return Regex.Unescape(text);
        }

        private static string Format(object value, string format)
        {
            var embeddedFormatString = string.Format(CultureInfo.InvariantCulture, "{{0:{0}}}", format);
            return string.Format(CultureInfo.CurrentCulture, embeddedFormatString, value);
        }

        private static string Replicate(string text, int count)
        {
            if (text == null)
                return null;

            var sb = new StringBuilder(text.Length * count);
            for (var i = 0; i < count; i++)
                sb.Append(text);
            return sb.ToString();
        }

        private static string Reverse(string text)
        {
            if (text == null)
                return null;

            var sb = new StringBuilder(text.Length);
            for (var i = text.Length - 1; i >= 0; i--)
                sb.Append(text[i]);
            return sb.ToString();
        }

        private static string Left(string text, int numberOfChars)
        {
            if (text == null)
                return null;

            if (numberOfChars > text.Length)
                numberOfChars = text.Length;

            return text.Substring(0, numberOfChars);
        }

        private static string Right(string text, int numberOfChars)
        {
            if (text == null)
                return null;

            if (numberOfChars > text.Length)
                numberOfChars = text.Length;

            return text.Substring(text.Length - numberOfChars, numberOfChars);
        }

        private static string Space(int numberOfSpaces)
        {
            if (numberOfSpaces <= 0)
                return string.Empty;

            return Replicate(" ", numberOfSpaces);
        }

        private static string LPad(string text, int totalWidth)
        {
            if (text == null)
                return null;

            return text.PadLeft(totalWidth);
        }

        private static string RPad(string text, int totalWidth)
        {
            if (text == null)
                return null;

            return text.PadRight(totalWidth);
        }

        #endregion

        #region Date

        private static DateTime GetDate()
        {
            return DateTime.Now;
        }

        private static DateTime GetUtcDate()
        {
            return DateTime.Now.ToUniversalTime();
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

        #endregion
    }
}

