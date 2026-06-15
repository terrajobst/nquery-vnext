using NQuery.Metadata;

namespace NQuery.Tests.Evaluation;

public sealed class EagerAndLazyTests
{
    private static InvocationResult EvaluateAndCountInvocations(string text)
    {
        var invocationResult = new InvocationResult();
        var invocationResultVariable = VariableDefinition.Create("ir", typeof(InvocationResult), invocationResult);
        var parameters = new[] { ParameterDefinition.Create("ir", typeof(InvocationResult)) };
        var nullInt32Function = FunctionDefinition.Create("NULL_INT32", typeof(int), parameters, (Func<InvocationResult, int?>)NullInt32Function);
        var nonNullInt32Function = FunctionDefinition.Create("NON_NULL_INT32", typeof(int), parameters, (Func<InvocationResult, int?>)NonNullInt32Function);
        var dataContext = DataContext.Default
                                     .AddVariables(invocationResultVariable)
                                     .AddFunctions(nullInt32Function, nonNullInt32Function);
        var expression = Expression<object>.Create(dataContext, text);
        invocationResult.Result = expression.Evaluate();
        return invocationResult;
    }

    [Fact]
    public void Evaluation_Conversion_Once()
    {
        var result = EvaluateAndCountInvocations("CAST(NON_NULL_INT32(ir) AS int64)");
        Assert.Equal(42L, result.Result);
        Assert.Equal(1, result.NonNullInt32FunctionCount);
    }

    [Fact]
    public void Evaluation_Unary_Once()
    {
        var result = EvaluateAndCountInvocations("~NON_NULL_INT32(ir)");
        Assert.Equal(~42, result.Result);
        Assert.Equal(1, result.NonNullInt32FunctionCount);
    }

    [Fact]
    public void Evaluation_Binary_EagerOnce()
    {
        var result = EvaluateAndCountInvocations("NULL_INT32(ir) + NON_NULL_INT32(ir)");
        Assert.Null(result.Result);
        Assert.Equal(1, result.NullInt32FunctionCount);
        Assert.Equal(1, result.NonNullInt32FunctionCount);
    }

    [Fact]
    public void Evaluation_FunctionInvocation_EagerOnce()
    {
        var result = EvaluateAndCountInvocations("SUBSTRING('abc', NULL_INT32(ir), NON_NULL_INT32(ir))");
        Assert.Null(result.Result);
        Assert.Equal(1, result.NullInt32FunctionCount);
        Assert.Equal(1, result.NonNullInt32FunctionCount);
    }

    [Fact]
    public void Evaluation_MethodInvocation_Instance_Once()
    {
        var result = EvaluateAndCountInvocations("NON_NULL_INT32(ir).Equals(42)");
        Assert.Equal(true, result.Result);
        Assert.Equal(1, result.NonNullInt32FunctionCount);
    }

    [Fact]
    public void Evaluation_MethodInvocation_Arguments_EagerOnce()
    {
        var result = EvaluateAndCountInvocations("''.Substring(NULL_INT32(ir), NON_NULL_INT32(ir))");
        Assert.Null(result.Result);
        Assert.Equal(1, result.NullInt32FunctionCount);
        Assert.Equal(1, result.NonNullInt32FunctionCount);
    }

    [Fact]
    public void Evaluation_PropertyAccess_Once()
    {
        var result = EvaluateAndCountInvocations("NON_NULL_INT32(ir).Equals(42)");
        Assert.Equal(true, result.Result);
        Assert.Equal(1, result.NonNullInt32FunctionCount);
    }

    [Fact]
    public void Evaluation_IsNull_Once()
    {
        var result = EvaluateAndCountInvocations("NON_NULL_INT32(ir) IS NOT NULL");
        Assert.Equal(true, result.Result);
        Assert.Equal(1, result.NonNullInt32FunctionCount);
    }

    [Fact]
    public void Evaluation_CaseWhen_NonNullFunction_LazyOnce()
    {
        const string text = @"
                CASE
                    WHEN NON_NULL_INT32(ir) = 42 THEN 42
                    ELSE NULL_INT32(ir)
                END";

        var result = EvaluateAndCountInvocations(text);
        Assert.Equal(42, result.Result);
        Assert.Equal(1, result.NonNullInt32FunctionCount);
        Assert.Equal(0, result.NullInt32FunctionCount);
    }

    [Fact]
    public void Evaluation_CaseWhen_NonNullNestedFunction_LazyOnce()
    {
        const string text = @"
                CASE
                    WHEN TO_INT32(NON_NULL_INT32(ir)) = 42 THEN 42
                    WHEN TO_INT32(NON_NULL_INT32(ir)) != 42 THEN 0
                    ELSE TO_INT32(NULL_INT32(ir))
                END";

        var result = EvaluateAndCountInvocations(text);
        Assert.Equal(42, result.Result);
        Assert.Equal(1, result.NonNullInt32FunctionCount);
        Assert.Equal(0, result.NullInt32FunctionCount);
    }

    [Fact]
    public void Evaluation_CaseWhen_NullFunction_LazyOnce()
    {
        const string text = @"
                CASE
                    WHEN NULL_INT32(ir) = 0 THEN 42
                    ELSE 0
                END";

        var result = EvaluateAndCountInvocations(text);
        Assert.Equal(0, result.Result);
        Assert.Equal(1, result.NullInt32FunctionCount);
    }

    private static int? NullInt32Function(InvocationResult ir)
    {
        ir.NullInt32FunctionCount++;
        return null;
    }

    private static int? NonNullInt32Function(InvocationResult ir)
    {
        ir.NonNullInt32FunctionCount++;
        return 42;
    }

    private sealed class InvocationResult
    {
        public object? Result { get; set; }
        public int NullInt32FunctionCount { get; set; }
        public int NonNullInt32FunctionCount { get; set; }
    }
}
