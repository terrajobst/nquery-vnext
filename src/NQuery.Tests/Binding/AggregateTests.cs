using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;
using NQuery.Metadata;
using NQuery.Metadata.Aggregation;

namespace NQuery.Tests.Binding;

public class AggregateTests
{
    private sealed class FakeAggregateDefinition : AggregateDefinition
    {
        public FakeAggregateDefinition(string name)
        {
            Name = name;
        }

        public override string Name { get; }

        internal override IAggregatable CreateAggregatable(Type argumentType)
        {
            return new FakeAggregatable();
        }

        private sealed class FakeAggregatable : IAggregatable
        {
            public IAggregator CreateAggregator()
            {
                throw new NotImplementedException();
            }

            public Type ReturnType { get { return typeof(object); } }
        }
    }

    private static AggregateDefinition CreateAggregate(string name)
    {
        return new FakeAggregateDefinition(name);
    }

    [Fact]
    public void Aggregate_DetectsAmbiguityBetweenAggregates()
    {
        var dataContext = DataContext.Default.AddAggregates(CreateAggregate("Agg"), CreateAggregate("AGG"));

        var syntaxTree = SyntaxTree.ParseQuery("SELECT AGG('test')");
        var compilation = Compilation.Empty.WithDataContext(dataContext).WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.AmbiguousAggregate, diagnostics[0].DiagnosticId);
    }

    [Fact]
    public void Aggregate_DetectsAmbiguityBetweenAggregateAndFunction()
    {
        var dataContext = DataContext.Default
                                     .AddAggregates(CreateAggregate("AGG"))
                                     .AddFunctions(FunctionDefinition.Create<string, string>("AGG", x => x));

        var syntaxTree = SyntaxTree.ParseQuery("SELECT AGG('test')");
        var compilation = Compilation.Empty.WithDataContext(dataContext).WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.AmbiguousReference, diagnostics[0].DiagnosticId);
    }

    [Fact]
    public void Aggregate_DetectsAmbiguityBetweenAggregateAndFunction_UnlessWrongArgumentCount()
    {
        var dataContext = DataContext.Default
                                     .AddAggregates(CreateAggregate("AGG"))
                                     .AddFunctions(FunctionDefinition.Create<string, string, string>("AGG", (x, _) => x));

        var aggregate = dataContext.Aggregates.Last();

        var syntaxTree = SyntaxTree.ParseQuery("SELECT AGG('test')");
        var compilation = Compilation.Empty.WithDataContext(dataContext).WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

        var invocation = syntaxTree.Root.DescendantNodes().OfType<FunctionInvocationExpressionSyntax>().Single();
        var symbol = semanticModel.GetSymbol(invocation);

        Assert.Empty(diagnostics);
        Assert.Same(aggregate, ((AggregateSymbol)symbol!).Definition);
    }

    [Fact]
    public void Aggregate_CountAllBindsToCount()
    {
        var dataContext = DataContext.Default;
        var countAggregated = dataContext.Aggregates.Single(a => a.Name == "COUNT");

        var syntaxTree = SyntaxTree.ParseQuery("SELECT COUNT(*)");
        var compilation = Compilation.Empty.WithDataContext(dataContext).WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

        var invocation = syntaxTree.Root.DescendantNodes().OfType<CountAllExpressionSyntax>().Single();
        var symbol = semanticModel.GetSymbol(invocation);

        Assert.Empty(diagnostics);
        Assert.Same(countAggregated, ((AggregateSymbol)symbol!).Definition);
    }

    [Fact]
    public void Aggregate_DetectsNonExistingCountAggregate()
    {
        var countAggregated = DataContext.Default.Aggregates.Single(a => a.Name == "COUNT");
        var dataContext = DataContext.Default.RemoveAggregates(countAggregated);

        var syntaxTree = SyntaxTree.ParseQuery("SELECT COUNT(*)");
        var compilation = Compilation.Empty.WithDataContext(dataContext).WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.UndeclaredAggregate, diagnostics[0].DiagnosticId);
    }

    [Fact]
    public void Aggregate_DetectsAmbiguityBetweenCountAggregates()
    {
        var dataContext = DataContext.Default.AddAggregates(CreateAggregate("Count"));

        var syntaxTree = SyntaxTree.ParseQuery("SELECT COUNT(*)");
        var compilation = Compilation.Empty.WithDataContext(dataContext).WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.AmbiguousAggregate, diagnostics[0].DiagnosticId);
    }

    [Fact]
    public void Aggregate_DetectsInvalidContextForAggregate()
    {
        var syntaxTree = SyntaxTree.ParseExpression("COUNT(4)");
        var compilation = Compilation.Empty.WithDataContext(DataContext.Default).WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.AggregateInvalidInCurrentContext, diagnostics[0].DiagnosticId);
    }
}
