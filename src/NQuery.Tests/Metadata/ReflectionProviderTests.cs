using NQuery.Metadata;

namespace NQuery.Tests.Metadata;

public sealed class ReflectionProviderTests
{
    private sealed class Overloads
    {
        public int Foo(int value) => value;
        public int Foo(int? value) => value ?? -1;
    }

    // After Nullable<T> erasure, Foo(int) and Foo(int?) share the signature Foo(Int32), so the
    // provider must collapse them to one. The survivor is chosen deterministically: the non-nullable
    // overload wins regardless of reflection order (see ReflectionProvider.ExistingMethodIsMoreSpecific).
    [Fact]
    public void GetMethods_CollapsesNullabilityOverloads_PreferringNonNullable()
    {
        var provider = new ReflectionProvider();

        var foos = provider.GetMethods(typeof(Overloads)).Where(m => m.Name == nameof(Overloads.Foo)).ToList();

        var method = Assert.Single(foos);
        Assert.Equal(typeof(int), Assert.Single(method.Parameters).Type);

        // The surviving definition is backed by the non-nullable CLR overload.
        var reflection = Assert.IsType<ReflectionMethodDefinition>(method);
        Assert.Equal(typeof(int), reflection.MethodInfo.GetParameters().Single().ParameterType);
    }
}
