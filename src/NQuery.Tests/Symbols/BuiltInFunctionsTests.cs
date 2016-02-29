using System;
using Xunit;

namespace NQuery.Tests.Symbols
{
    public class BuiltInFunctionsTests
    {
        private static void AssertEvaluatesTo(string text, object expectedValue)
        {
            var actualValue = Compute(text);

            Assert.Equal(expectedValue, actualValue);
        }

        private static void AssertEvaluatesToDouble(string text, double expectedValue)
        {
            var untypedActualValue = Compute(text);
            var actualValue = Convert.ToDouble(untypedActualValue);
            var expectedRounded = Math.Round(actualValue, 6);
            var actualRounded = Math.Round(expectedValue, 6);

            Assert.Equal(expectedRounded, actualRounded);
        }

        private static void AssertEvaluatesTo(string text, DateTime expectedValue)
        {
            var untypedActualValue = Compute(text);
            var actualValue = Convert.ToDateTime(untypedActualValue);
            var expectedRounded = TruncateSeconds(expectedValue);
            var actualRounded = TruncateSeconds(actualValue);

            Assert.Equal(expectedRounded, actualRounded);
        }

        private static DateTime TruncateSeconds(DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
        }

        private static object Compute(string text)
        {
            var dataContext = DataContext.Default;
            var expression = Expression<object>.Create(dataContext, text);
            return expression.Evaluate();
        }

        [Fact]
        public void BuiltInFunctions_Abs()
        {
            AssertEvaluatesToDouble("ABS(-2)", 2);
            AssertEvaluatesToDouble("ABS(0)", 0);
            AssertEvaluatesToDouble("ABS(+3)", 3);
        }

        [Fact]
        public void BuiltInFunctions_Acos()
        {
            AssertEvaluatesToDouble("ACOS(0)", Math.PI / 2);
            AssertEvaluatesToDouble("ACOS(1)", 0);
        }

        [Fact]
        public void BuiltInFunctions_Asin()
        {
            AssertEvaluatesToDouble("ASIN(0)", 0.0);
            AssertEvaluatesToDouble("ASIN(1)", Math.PI / 2);
        }

        [Fact]
        public void BuiltInFunctions_Atan()
        {
            AssertEvaluatesToDouble("ATAN(0)", 0.0);
            AssertEvaluatesToDouble("ATAN(1)", Math.PI / 4);
        }

        [Fact]
        public void BuiltInFunctions_Atan2()
        {
            AssertEvaluatesToDouble("ATAN2(0, 0)", 0.0);
            AssertEvaluatesToDouble("ATAN2(1, 1)", Math.PI / 4);
        }

        [Fact]
        public void BuiltInFunctions_Ceiling()
        {
            AssertEvaluatesToDouble("CEILING(-3.1)", -3.0);
            AssertEvaluatesToDouble("CEILING(-3.9)", -3.0);
            AssertEvaluatesToDouble("CEILING(2.1)", 3.0);
            AssertEvaluatesToDouble("CEILING(2.9)", 3.0);
        }

        [Fact]
        public void BuiltInFunctions_Charindex()
        {
            AssertEvaluatesTo("CHARINDEX('el', 'hello')", 2);
            AssertEvaluatesTo("CHARINDEX('test', 'hello')", 0);
        }

        [Fact]
        public void BuiltInFunctions_Cos()
        {
            AssertEvaluatesToDouble("COS(0)", 1.0);
            AssertEvaluatesToDouble("COS(1)", 0.54030230586814);
        }

        [Fact]
        public void BuiltInFunctions_Cosh()
        {
            AssertEvaluatesToDouble("COSH(0)", 1);
            AssertEvaluatesToDouble("COSH(1)", 1.54308063481524);
        }

        [Fact]
        public void BuiltInFunctions_Day()
        {
            AssertEvaluatesTo("DAY(TO_DATETIME('2010-1-1'))", 1);
            AssertEvaluatesTo("DAY(TO_DATETIME('2011-3-2'))", 2);
            AssertEvaluatesTo("DAY(TO_DATETIME('2012-12-31'))", 31);
        }

        [Fact]
        public void BuiltInFunctions_Exp()
        {
            AssertEvaluatesToDouble("EXP(0)", 1);
            AssertEvaluatesToDouble("EXP(1)", Math.E);
            AssertEvaluatesToDouble("EXP(2)", Math.E * Math.E);
        }

        [Fact]
        public void BuiltInFunctions_Floor()
        {
            AssertEvaluatesToDouble("FLOOR(-3.1)", -4.0);
            AssertEvaluatesToDouble("FLOOR(-3.9)", -4.0);
            AssertEvaluatesToDouble("FLOOR(2.1)", 2.0);
            AssertEvaluatesToDouble("FLOOR(2.9)", 2.0);
        }

        [Fact]
        public void BuiltInFunctions_Format()
        {
            AssertEvaluatesTo("FORMAT(.123, 'P2')", "12.30 %");
        }

        [Fact]
        public void BuiltInFunctions_GetDate()
        {
            AssertEvaluatesTo("GETDATE()", DateTime.Now);
        }

        [Fact]
        public void BuiltInFunctions_GetUtcDate()
        {
            AssertEvaluatesTo("GETUTCDATE()", DateTime.UtcNow);
        }

        [Fact]
        public void BuiltInFunctions_Left()
        {
            AssertEvaluatesTo("LEFT('test', 0)", "");
            AssertEvaluatesTo("LEFT('test', 2)", "te");
            AssertEvaluatesTo("LEFT('test', 4)", "test");
            AssertEvaluatesTo("LEFT('test', 10)", "test");
        }

        [Fact]
        public void BuiltInFunctions_Len()
        {
            AssertEvaluatesTo("LEN('')", 0);
            AssertEvaluatesTo("LEN('hello')", 5);
        }

        [Fact]
        public void BuiltInFunctions_Log()
        {
            AssertEvaluatesToDouble("LOG(0)", double.NegativeInfinity);
            AssertEvaluatesToDouble("LOG(1)", 0.0);
            AssertEvaluatesToDouble("LOG(3)", 1.09861228866811);
        }

        [Fact]
        public void BuiltInFunctions_Log10()
        {
            AssertEvaluatesToDouble("LOG10(0)", double.NegativeInfinity);
            AssertEvaluatesToDouble("LOG10(1)", 0.0);
            AssertEvaluatesToDouble("LOG10(10)", 1);
        }

        [Fact]
        public void BuiltInFunctions_Lower()
        {
            AssertEvaluatesTo("LOWER('Hello')", "hello");
        }

        [Fact]
        public void BuiltInFunctions_LPad()
        {
            AssertEvaluatesTo("LPAD('test', 10)", "      test");
        }

        [Fact]
        public void BuiltInFunctions_LTrim()
        {
            AssertEvaluatesTo("LTRIM('   test   ')", "test   ");
        }

        [Fact]
        public void BuiltInFunctions_Month()
        {
            AssertEvaluatesTo("MONTH(TO_DATETIME('2010-1-1'))", 1);
            AssertEvaluatesTo("MONTH(TO_DATETIME('2011-3-2'))", 3);
            AssertEvaluatesTo("MONTH(TO_DATETIME('2012-12-31'))", 12);
        }

        [Fact]
        public void BuiltInFunctions_Pow()
        {
            AssertEvaluatesToDouble("POW(2, 0)", 1);
            AssertEvaluatesToDouble("POW(2, 1)", 2);
            AssertEvaluatesToDouble("POW(2, 2)", 4);
        }

        [Fact]
        public void BuiltInFunctions_RegexEscape()
        {
            AssertEvaluatesTo("REGEX_ESCAPE('')", "");
            AssertEvaluatesTo("REGEX_ESCAPE('test')", "test");
            AssertEvaluatesTo("REGEX_ESCAPE('.*')", @"\.\*");
        }

        [Fact]
        public void BuiltInFunctions_RegexMatch()
        {
            AssertEvaluatesTo("REGEX_MATCH('', 'v\\d+\\.\\d+')", false);
            AssertEvaluatesTo("REGEX_MATCH('v12.2', '^v\\d+\\.\\d+$')", true);
            AssertEvaluatesTo("REGEX_MATCH('v12.2', '^\\d+\\.\\d+$')", false);
        }

        [Fact]
        public void BuiltInFunctions_RegexReplace()
        {
            AssertEvaluatesTo("REGEX_REPLACE('', '([a-z]+) = ([a-z]+)', '$2 = $1')", "");
            AssertEvaluatesTo("REGEX_REPLACE('var = value', '([a-z]+) = ([a-z]+)', '$2 = $1')", "value = var");
            AssertEvaluatesTo("REGEX_REPLACE('var != value', '([a-z]+) = ([a-z]+)', '$2 = $1')", "var != value");
        }

        [Fact]
        public void BuiltInFunctions_RegexUnescape()
        {
            AssertEvaluatesTo("REGEX_UNESCAPE('')", "");
            AssertEvaluatesTo("REGEX_UNESCAPE('test')", "test");
            AssertEvaluatesTo("REGEX_UNESCAPE('\\.\\*')", ".*");
        }

        [Fact]
        public void BuiltInFunctions_Replace()
        {
            AssertEvaluatesTo("REPLACE('', ',', '_')", "");
            AssertEvaluatesTo("REPLACE('hello, world', ', ', '_')", "hello_world");
        }

        [Fact]
        public void BuiltInFunctions_Replicate()
        {
            AssertEvaluatesTo("REPLICATE('', 0)", "");
            AssertEvaluatesTo("REPLICATE('', 1)", "");
            AssertEvaluatesTo("REPLICATE('', 2)", "");

            AssertEvaluatesTo("REPLICATE('a', 0)", "");
            AssertEvaluatesTo("REPLICATE('a', 1)", "a");
            AssertEvaluatesTo("REPLICATE('a', 2)", "aa");

            AssertEvaluatesTo("REPLICATE('hello', 0)", "");
            AssertEvaluatesTo("REPLICATE('hello', 1)", "hello");
            AssertEvaluatesTo("REPLICATE('hello', 2)", "hellohello");
        }

        [Fact]
        public void BuiltInFunctions_Reverse()
        {
            AssertEvaluatesTo("REVERSE('')", "");
            AssertEvaluatesTo("REVERSE('a')", "a");
            AssertEvaluatesTo("REVERSE('hello')", "olleh");
        }

        [Fact]
        public void BuiltInFunctions_Right()
        {
            AssertEvaluatesTo("RIGHT('test', 0)", "");
            AssertEvaluatesTo("RIGHT('test', 2)", "st");
            AssertEvaluatesTo("RIGHT('test', 4)", "test");
            AssertEvaluatesTo("RIGHT('test', 10)", "test");
        }

        [Fact]
        public void BuiltInFunctions_Round()
        {
            AssertEvaluatesToDouble("ROUND(1.1)", 1.0);
            AssertEvaluatesToDouble("ROUND(1.2)", 1.0);
            AssertEvaluatesToDouble("ROUND(1.5)", 2.0);
            AssertEvaluatesToDouble("ROUND(1.6)", 2.0);

            AssertEvaluatesToDouble("ROUND(TO_DECIMAL(1.001), 2)", 1.00);
            AssertEvaluatesToDouble("ROUND(TO_DECIMAL(1.002), 2)", 1.00);
            AssertEvaluatesToDouble("ROUND(TO_DECIMAL(1.005), 2)", 1.01);
            AssertEvaluatesToDouble("ROUND(TO_DECIMAL(1.006), 2)", 1.01);
        }

        [Fact]
        public void BuiltInFunctions_RPad()
        {
            AssertEvaluatesTo("RPAD('test', 10)", "test      ");
        }

        [Fact]
        public void BuiltInFunctions_RTrim()
        {
            AssertEvaluatesTo("RTRIM('   test   ')", "   test");
        }

        [Fact]
        public void BuiltInFunctions_Sign()
        {
            AssertEvaluatesTo("SIGN(-3)", -1);
            AssertEvaluatesTo("SIGN(0)", 0);
            AssertEvaluatesTo("SIGN(+2)", +1);
        }

        [Fact]
        public void BuiltInFunctions_Sin()
        {
            AssertEvaluatesToDouble("SIN(0)", 0.0);
            AssertEvaluatesToDouble("SIN(1)", 0.841470984807897);
        }

        [Fact]
        public void BuiltInFunctions_Sinh()
        {
            AssertEvaluatesToDouble("SINH(0)", 0);
            AssertEvaluatesToDouble("SINH(1)", 1.1752011936438);
        }

        [Fact]
        public void BuiltInFunctions_Soundex()
        {
            AssertEvaluatesTo("SOUNDEX('Robert')", "R163");
        }

        [Fact]
        public void BuiltInFunctions_Space()
        {
            AssertEvaluatesTo("SPACE(0)", "");
            AssertEvaluatesTo("SPACE(1)", " ");
            AssertEvaluatesTo("SPACE(4)", "    ");
        }

        [Fact]
        public void BuiltInFunctions_Sqrt()
        {
            AssertEvaluatesToDouble("SQRT(0)", 0.0);
            AssertEvaluatesToDouble("SQRT(2)", Math.Sqrt(2.0));
            AssertEvaluatesToDouble("SQRT(4)", 2.0);
            AssertEvaluatesToDouble("SQRT(9)", 3.0);
        }

        [Fact]
        public void BuiltInFunctions_Substring()
        {
            AssertEvaluatesTo("SUBSTRING('hello', 0)", "");
            AssertEvaluatesTo("SUBSTRING('hello', 1)", "hello");
            AssertEvaluatesTo("SUBSTRING('hello', 3)", "llo");

            AssertEvaluatesTo("SUBSTRING('hello', 0, 0)", "");
            AssertEvaluatesTo("SUBSTRING('hello', 0, 1)", "");
            AssertEvaluatesTo("SUBSTRING('hello', 1, 5)", "hello");
            AssertEvaluatesTo("SUBSTRING('hello', 1, 10)", "hello");
            AssertEvaluatesTo("SUBSTRING('hello', 1, 3)", "hel");
            AssertEvaluatesTo("SUBSTRING('hello', 3, 2)", "ll");
            AssertEvaluatesTo("SUBSTRING('hello', 3, 3)", "llo");
            AssertEvaluatesTo("SUBSTRING('hello', 3, 4)", "llo");
        }

        [Fact]
        public void BuiltInFunctions_Tan()
        {
            AssertEvaluatesToDouble("TAN(0)", 0.0);
            AssertEvaluatesToDouble("TAN(1)", 1.5574077246549);
        }

        [Fact]
        public void BuiltInFunctions_Tanh()
        {
            AssertEvaluatesToDouble("TANH(0)", 0);
            AssertEvaluatesToDouble("TANH(1)", 0.761594155955765);
        }

        [Fact]
        public void BuiltInFunctions_ToBoolean()
        {
            AssertEvaluatesTo("TO_BOOLEAN(0)", false);
            AssertEvaluatesTo("TO_BOOLEAN(1)", true);

            AssertEvaluatesTo("TO_BOOLEAN('True')", true);
            AssertEvaluatesTo("TO_BOOLEAN('False')", false);

            AssertEvaluatesTo("TO_BOOLEAN('true')", true);
            AssertEvaluatesTo("TO_BOOLEAN('false')", false);
        }

        [Fact]
        public void BuiltInFunctions_ToByte()
        {
            AssertEvaluatesTo("TO_BYTE(0)", (byte)0);
            AssertEvaluatesTo("TO_BYTE(255)", (byte)255);
        }

        [Fact]
        public void BuiltInFunctions_ToChar()
        {
            AssertEvaluatesTo("TO_CHAR(32)", ' ');
            AssertEvaluatesTo("TO_CHAR(' ')", ' ');
        }

        [Fact]
        public void BuiltInFunctions_ToDateTime()
        {
            AssertEvaluatesTo("TO_DATETIME('2015-12-31')", new DateTime(2015, 12, 31));
        }

        [Fact]
        public void BuiltInFunctions_ToDecimal()
        {
            AssertEvaluatesTo("TO_DECIMAL(-123.456)", -123.456m);
            AssertEvaluatesTo("TO_DECIMAL(0)", 0m);
            AssertEvaluatesTo("TO_DECIMAL(123.456)", 123.456m);

            AssertEvaluatesTo("TO_DECIMAL('-123.456')", -123.456m);
            AssertEvaluatesTo("TO_DECIMAL('0')", 0m);
            AssertEvaluatesTo("TO_DECIMAL('123.456')", 123.456m);
        }

        [Fact]
        public void BuiltInFunctions_ToDouble()
        {
            AssertEvaluatesTo("TO_DOUBLE(0)", 0d);
            AssertEvaluatesTo("TO_DOUBLE(123e4)", 123e4d);
            AssertEvaluatesTo("TO_DOUBLE(123e-4)", 123e-4d);

            AssertEvaluatesTo("TO_DOUBLE('0')", 0d);
            AssertEvaluatesTo("TO_DOUBLE('123e4')", 123e4d);
            AssertEvaluatesTo("TO_DOUBLE('123e-4')", 123e-4d);
        }

        [Fact]
        public void BuiltInFunctions_ToInt16()
        {
            AssertEvaluatesTo("TO_INT16(-32000)", (short)-32000);
            AssertEvaluatesTo("TO_INT16(0)", (short)0);
            AssertEvaluatesTo("TO_INT16(32000)", (short)32000);

            AssertEvaluatesTo("TO_INT16('-32000')", (short)-32000);
            AssertEvaluatesTo("TO_INT16('0')", (short)0);
            AssertEvaluatesTo("TO_INT16('32000')", (short)32000);
        }

        [Fact]
        public void BuiltInFunctions_ToInt32()
        {
            AssertEvaluatesTo("TO_INT32(-123456789)", -123456789);
            AssertEvaluatesTo("TO_INT32(0)", 0);
            AssertEvaluatesTo("TO_INT32(123456789)", 123456789);

            AssertEvaluatesTo("TO_INT32('-123456789')", -123456789);
            AssertEvaluatesTo("TO_INT32('0')", 0);
            AssertEvaluatesTo("TO_INT32('123456789')", 123456789);
        }

        [Fact]
        public void BuiltInFunctions_ToInt64()
        {
            AssertEvaluatesTo("TO_INT64(-12345678912)", -12345678912);
            AssertEvaluatesTo("TO_INT64(0)", (long)0);
            AssertEvaluatesTo("TO_INT64(12345678912)", 12345678912);

            AssertEvaluatesTo("TO_INT64('-12345678912')", -12345678912);
            AssertEvaluatesTo("TO_INT64('0')", (long)0);
            AssertEvaluatesTo("TO_INT64('12345678912')", 12345678912);
        }

        [Fact]
        public void BuiltInFunctions_ToSbyte()
        {
            AssertEvaluatesTo("TO_SBYTE(-127)", (sbyte)-127);
            AssertEvaluatesTo("TO_SBYTE(0)", (sbyte)0);
            AssertEvaluatesTo("TO_SBYTE(127)", (sbyte)127);

            AssertEvaluatesTo("TO_SBYTE('-127')", (sbyte)-127);
            AssertEvaluatesTo("TO_SBYTE('0')", (sbyte)0);
            AssertEvaluatesTo("TO_SBYTE('127')", (sbyte)127);
        }

        [Fact]
        public void BuiltInFunctions_ToSingle()
        {
            AssertEvaluatesTo("TO_SINGLE(0)", 0f);
            AssertEvaluatesTo("TO_SINGLE(123e4)", 123e4f);
            AssertEvaluatesTo("TO_SINGLE(123e-4)", 123e-4f);

            AssertEvaluatesTo("TO_SINGLE('0')", 0f);
            AssertEvaluatesTo("TO_SINGLE('123e4')", 123e4f);
            AssertEvaluatesTo("TO_SINGLE('123e-4')", 123e-4f);
        }

        [Fact]
        public void BuiltInFunctions_ToString()
        {
            AssertEvaluatesTo("TO_STRING(0)", "0");
            AssertEvaluatesTo("TO_STRING(1)", "1");

            AssertEvaluatesTo("TO_STRING(FALSE)", "False");
            AssertEvaluatesTo("TO_STRING(TRUE)", "True");
        }

        [Fact]
        public void BuiltInFunctions_ToUInt16()
        {
            AssertEvaluatesTo("TO_UINT16(0)", (ushort)0);
            AssertEvaluatesTo("TO_UINT16(65000)", (ushort)65000);

            AssertEvaluatesTo("TO_UINT16('0')", (ushort)0);
            AssertEvaluatesTo("TO_UINT16('65000')", (ushort)65000);
        }

        [Fact]
        public void BuiltInFunctions_ToUInt32()
        {
            AssertEvaluatesTo("TO_UINT32(0)", (uint)0);
            AssertEvaluatesTo("TO_UINT32(4123456789)", 4123456789);

            AssertEvaluatesTo("TO_UINT32('0')", (uint)0);
            AssertEvaluatesTo("TO_UINT32('4123456789')", 4123456789);
        }

        [Fact]
        public void BuiltInFunctions_ToUInt64()
        {
            AssertEvaluatesTo("TO_UINT64(0)", (ulong) 0);
            AssertEvaluatesTo("TO_UINT64(123456789)", (ulong)123456789);

            AssertEvaluatesTo("TO_UINT64('0')", (ulong) 0);
            AssertEvaluatesTo("TO_UINT64('9999999999123456789')", 9999999999123456789);
        }

        [Fact]
        public void BuiltInFunctions_Trim()
        {
            AssertEvaluatesTo("TRIM('   test   ')", "test");
        }

        [Fact]
        public void BuiltInFunctions_Upper()
        {
            AssertEvaluatesTo("UPPER('Hello')", "HELLO");
        }

        [Fact]
        public void BuiltInFunctions_Year()
        {
            AssertEvaluatesTo("YEAR(TO_DATETIME('2010-1-1'))", 2010);
            AssertEvaluatesTo("YEAR(TO_DATETIME('2011-3-2'))", 2011);
            AssertEvaluatesTo("YEAR(TO_DATETIME('2012-12-31'))", 2012);
        }
    }
}