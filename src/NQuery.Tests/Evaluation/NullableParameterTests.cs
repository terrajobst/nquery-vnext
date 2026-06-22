using System.Collections.Immutable;

using NQuery.Metadata;

namespace NQuery.Tests.Evaluation;

// Item 1 of the erasure work: the declared parameter type is erased to its non-nullable form and
// the engine lowers arguments before a call, so an invocation must coerce the lowered argument back
// to the member's actual (possibly Nullable<T>) parameter type. These exercise the two Expression.Call
// paths -- delegate functions and reflected methods -- that go through CoerceArguments.
public sealed class NullableParameterTests : EvaluationTest
{
    [Fact]
    public void DelegateFunction_WithNullableParameter_IsInvoked()
    {
        Func<int?, int> increment = x => x.GetValueOrDefault() + 1;
        var function = FunctionDefinition.Create(
            "IncN", typeof(int), new[] { ParameterDefinition.Create("x", typeof(int?)) }, increment);
        var catalog = Catalog.Default.AddFunctions(function);

        AssertProduces("SELECT IncN(41)", new[] { 42 }, catalog);
    }

    [Fact]
    public void ReflectionMethod_WithNullableParameter_IsInvoked()
    {
        var method = MethodDefinition.Create(typeof(Box).GetMethod(nameof(Box.AddOne))!);
        var catalog = Catalog.Default
                             .AddVariables(VariableDefinition.Create("b", typeof(Box), new Box()))
                             .AddMethodProvider(typeof(Box), new FixedMethodProvider(method));

        AssertProduces("SELECT @b.AddOne(41)", new[] { 42 }, catalog);
    }

    private sealed class Box
    {
        public int AddOne(int? value) => value.GetValueOrDefault() + 1;
    }

    private sealed class FixedMethodProvider : IMethodProvider
    {
        private readonly ImmutableArray<MethodDefinition> _methods;

        public FixedMethodProvider(params MethodDefinition[] methods)
        {
            _methods = methods.ToImmutableArray();
        }

        public IEnumerable<MethodDefinition> GetMethods(Type type)
        {
            return _methods;
        }
    }
}
