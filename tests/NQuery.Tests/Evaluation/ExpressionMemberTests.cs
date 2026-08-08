using System.Collections.Immutable;

using NQuery.Metadata;

namespace NQuery.Tests.Evaluation;

public sealed class ExpressionMemberTests : EvaluationTest
{
    [Fact]
    public void ExpressionFunction_IsEvaluated()
    {
        var function = FunctionDefinition.Create<int, int>("SQUARE", x => x * x);
        var catalog = Catalog.Default.AddFunctions(function);

        AssertProduces("SELECT SQUARE(5)", [25], catalog);
    }

    [Fact]
    public void ExpressionProperty_IsEvaluated()
    {
        // The single lambda parameter is the instance.
        var property = PropertyDefinition.Create<string, int>("DoubleLength", s => s.Length * 2);
        var catalog = Catalog.Default
                                     .AddVariables(VariableDefinition.Create("s", typeof(string), "abc"))
                                     .AddPropertyProvider(typeof(string), new FixedPropertyProvider(property));

        AssertProduces("SELECT @s.DoubleLength", [6], catalog);
    }

    [Fact]
    public void DelegateProperty_WithExplicitType_IsEvaluated()
    {
        var property = PropertyDefinition.Create("DoubleLength", typeof(int), (Func<string, int>)(s => s.Length * 2));
        var catalog = Catalog.Default
                                     .AddVariables(VariableDefinition.Create("s", typeof(string), "abc"))
                                     .AddPropertyProvider(typeof(string), new FixedPropertyProvider(property));

        AssertProduces("SELECT @s.DoubleLength", [6], catalog);
    }

    [Fact]
    public void ExpressionMethod_IsEvaluated()
    {
        // The first lambda parameter is the instance; the rest are the method's arguments.
        var method = MethodDefinition.Create<string, int, string>("Head", (s, n) => s.Substring(0, n));
        var catalog = Catalog.Default
                                     .AddVariables(VariableDefinition.Create("s", typeof(string), "abcdef"))
                                     .AddMethodProvider(typeof(string), new FixedMethodProvider(method));

        AssertProduces("SELECT @s.Head(3)", ["abc"], catalog);
    }

    private sealed class FixedPropertyProvider : IPropertyProvider
    {
        private readonly ImmutableArray<PropertyDefinition> _properties;

        public FixedPropertyProvider(params PropertyDefinition[] properties)
        {
            _properties = [.. properties];
        }

        public IEnumerable<PropertyDefinition> GetProperties(Type type)
        {
            return _properties;
        }
    }

    private sealed class FixedMethodProvider : IMethodProvider
    {
        private readonly ImmutableArray<MethodDefinition> _methods;

        public FixedMethodProvider(params MethodDefinition[] methods)
        {
            _methods = [.. methods];
        }

        public IEnumerable<MethodDefinition> GetMethods(Type type)
        {
            return _methods;
        }
    }
}
