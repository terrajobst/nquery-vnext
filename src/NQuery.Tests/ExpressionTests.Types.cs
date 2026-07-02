namespace NQuery.Tests;

public partial class ExpressionTests
{
    public static IEnumerable<object[]> GetBuiltInNumericTypes()
    {
        return new[]
        {
            new object[] {typeof (int)},
            [typeof (uint)],
            [typeof (long)],
            [typeof (ulong)],
            [typeof (float)],
            [typeof (double)],
            [typeof (decimal)]
        };
    }

    public static IEnumerable<object[]> GetBuiltInSignedNumericTypes()
    {
        return new[]
        {
            new object[] {typeof (int)},
            [typeof (long)],
            [typeof (float)],
            [typeof (double)],
            [typeof (decimal)]
        };
    }

    public static IEnumerable<object[]> GetBuiltInIntegralTypes()
    {
        return new[]
        {
            new object[] {typeof (int)},
            [typeof (uint)],
            [typeof (long)],
            [typeof (ulong)]
        };
    }
}
