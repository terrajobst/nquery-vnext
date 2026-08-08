using System.Collections.Immutable;

using NQuery.Metadata;

namespace NQuery.Tests.Evaluation;

// Covers the reflection-based FunctionDefinition.Create and the delegate-based MethodDefinition.Create
// added to make the two definition types offer a uniform set of factory methods (expression / delegate
// / reflection). The pre-existing directions are exercised by ExpressionMemberTests and
// NullableParameterTests.
public sealed class UniformCreateTests : EvaluationTest
{
    [Fact]
    public void ReflectionFunction_FromStaticMethod_IsEvaluated()
    {
        var method = typeof(UniformCreateTests).GetMethod(nameof(Triple))!;
        var function = FunctionDefinition.Create(method.Name, method);
        var catalog = Catalog.Default.AddFunctions(function);

        AssertProduces("SELECT Triple(7)", [21], catalog);
    }

    [Fact]
    public void ReflectionFunction_WithExplicitName_IsEvaluated()
    {
        var method = typeof(UniformCreateTests).GetMethod(nameof(Triple))!;
        var function = FunctionDefinition.Create("TIMES_THREE", method);
        var catalog = Catalog.Default.AddFunctions(function);

        AssertProduces("SELECT TIMES_THREE(7)", [21], catalog);
    }

    [Fact]
    public void ReflectionFunction_FromInstanceMethod_WithoutInstance_Throws()
    {
        var method = typeof(UniformCreateTests).GetMethod(nameof(InstanceTriple))!;

        Assert.Throws<ArgumentException>(() => FunctionDefinition.Create(method.Name, method));
    }

    [Fact]
    public void ReflectionFunction_FromInstanceMethod_WithInstance_IsEvaluated()
    {
        // The bound instance is closed over, so the delta is supplied by the receiver, not an argument.
        var adder = new Adder(10);
        var method = typeof(Adder).GetMethod(nameof(Adder.Add))!;
        var function = FunctionDefinition.Create("ADD_TEN", method, adder);
        var catalog = Catalog.Default.AddFunctions(function);

        AssertProduces("SELECT ADD_TEN(5)", [15], catalog);
    }

    [Fact]
    public void ReflectionFunction_WithInstance_OnStaticMethod_Throws()
    {
        var method = typeof(UniformCreateTests).GetMethod(nameof(Triple))!;

        Assert.Throws<ArgumentException>(() => FunctionDefinition.Create("Triple", method, new Adder(1)));
    }

    [Fact]
    public void ReflectionFunction_WithInstance_OfWrongType_Throws()
    {
        var method = typeof(Adder).GetMethod(nameof(Adder.Add))!;

        Assert.Throws<ArgumentException>(() => FunctionDefinition.Create("Add", method, "not an adder"));
    }

    [Fact]
    public void DelegateMethod_IsEvaluated()
    {
        // The delegate's first parameter is the instance; the rest are the method's arguments.
        Func<string, int, string> head = (s, n) => s.Substring(0, n);
        var method = MethodDefinition.Create(
            "Head", typeof(string), new[] { ParameterDefinition.Create("n", typeof(int)) }, head);
        var catalog = Catalog.Default
                             .AddVariables(VariableDefinition.Create("s", typeof(string), "abcdef"))
                             .AddMethodProvider(typeof(string), new FixedMethodProvider(method));

        AssertProduces("SELECT @s.Head(3)", ["abc"], catalog);
    }

    [Fact]
    public void InstanceType_IsCaptured_ForEachMethodBacking()
    {
        var expression = MethodDefinition.Create<string, int, string>("Head", (s, n) => s.Substring(0, n));
        Assert.Equal(typeof(string), expression.InstanceType);

        Func<string, int, string> head = (s, n) => s.Substring(0, n);
        var @delegate = MethodDefinition.Create(
            "Head", typeof(string), new[] { ParameterDefinition.Create("n", typeof(int)) }, head);
        Assert.Equal(typeof(string), @delegate.InstanceType);

        var substring = typeof(string).GetMethod(nameof(string.Substring), new[] { typeof(int) })!;
        var reflection = MethodDefinition.Create(substring.Name, substring);
        Assert.Equal(typeof(string), reflection.InstanceType);
    }

    [Fact]
    public void InstanceType_IsCaptured_ForEachPropertyBacking()
    {
        var expression = PropertyDefinition.Create<string, int>("Len", s => s.Length);
        Assert.Equal(typeof(string), expression.InstanceType);

        var @delegate = PropertyDefinition.Create("Len", typeof(int), (Func<string, int>)(s => s.Length));
        Assert.Equal(typeof(string), @delegate.InstanceType);

        var length = typeof(string).GetProperty(nameof(string.Length))!;
        var reflection = PropertyDefinition.Create(length.Name, length);
        Assert.Equal(typeof(string), reflection.InstanceType);
    }

    public static int Triple(int x) => x * 3;

    public int InstanceTriple(int x) => x * 3;

    private sealed class Adder
    {
        private readonly int _delta;

        public Adder(int delta) => _delta = delta;

        public int Add(int x) => x + _delta;
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
