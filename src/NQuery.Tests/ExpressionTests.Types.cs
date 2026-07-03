namespace NQuery.Tests;

public partial class ExpressionTests
{
    public static TheoryData<Type> GetBuiltInNumericTypes() =>
        [typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)];

    public static TheoryData<Type> GetBuiltInSignedNumericTypes() =>
        [typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal)];

    public static TheoryData<Type> GetBuiltInIntegralTypes() =>
        [typeof(int), typeof(uint), typeof(long), typeof(ulong)];
}
