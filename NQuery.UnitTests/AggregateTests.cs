using Microsoft.VisualStudio.TestTools.UnitTesting;
using NQuery.Language.Runtime;
using NQuery.Language.Symbols;
using System.Linq;

namespace NQuery.Language.UnitTests
{
    [TestClass]
    public class AggregateTests
    {
        [TestMethod]
        public void Aggregate_DetectsAmbiguityBetweenAggregates()
        {
            var dataContext = new DataContextBuilder
            {
                Aggregates =
                    {
                        new AggregateSymbol("Agg"),
                        new AggregateSymbol("AGG")
                    }
            }.GetResult();

            var syntaxTree = SyntaxTree.ParseQuery("SELECT AGG('test')");
            var compilation = Compilation.Empty.WithDataContext(dataContext).WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.AmbiguousAggregate, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Aggregate_DetectsAmbiguityBetweenAggregateAndFunction()
        {
            var dataContext = new DataContextBuilder
            {
                Aggregates = {new AggregateSymbol("AGG")},
                Functions = {new FunctionSymbol<string, string>("AGG", x => x)}
            }.GetResult();

            var syntaxTree = SyntaxTree.ParseQuery("SELECT AGG('test')");
            var compilation = Compilation.Empty.WithDataContext(dataContext).WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.AmbiguousReference, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Aggregate_DetectsAmbiguityBetweenAggregateAndFunction_UnlessWrongArgumentCount()
        {
            var dataContext = new DataContextBuilder
            {
                Aggregates = {new AggregateSymbol("AGG")},
                Functions = {new FunctionSymbol<string, string, string>("AGG", (x, y) => x)}
            }.GetResult();

            var aggregate = dataContext.Aggregates.Last();

            var syntaxTree = SyntaxTree.ParseQuery("SELECT AGG('test')");
            var compilation = Compilation.Empty.WithDataContext(dataContext).WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            var invocation = syntaxTree.Root.DescendantNodes().OfType<FunctionInvocationExpressionSyntax>().Single();
            var symbol = semanticModel.GetSymbol(invocation);

            Assert.AreEqual(0, diagnostics.Length);
            Assert.AreEqual(aggregate, symbol);
        }

        [TestMethod]
        public void Aggregate_CountAllBindsToCount()
        {
            var dataContext = new DataContextBuilder().GetResult();
            var countAggregated = dataContext.Aggregates.Single(a => a.Name == "COUNT");

            var syntaxTree = SyntaxTree.ParseQuery("SELECT COUNT(*)");
            var compilation = Compilation.Empty.WithDataContext(dataContext).WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            var invocation = syntaxTree.Root.DescendantNodes().OfType<CountAllExpressionSyntax>().Single();
            var symbol = semanticModel.GetSymbol(invocation);

            Assert.AreEqual(0, diagnostics.Length);
            Assert.AreEqual(countAggregated, symbol);
        }

        [TestMethod]
        public void Aggregate_DetectsNonExistingCountAggregate()
        {
            var dataContextBuilder = new DataContextBuilder();
            var countAggregated = dataContextBuilder.Aggregates.Single(a => a.Name == "COUNT");
            dataContextBuilder.Aggregates.Remove(countAggregated);

            var dataContext = dataContextBuilder.GetResult();

            var syntaxTree = SyntaxTree.ParseQuery("SELECT COUNT(*)");
            var compilation = Compilation.Empty.WithDataContext(dataContext).WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UndeclaredAggregate, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Aggregate_Aggregate_DetectsAmbiguityBetweenCountAggregates()
        {
            var dataContextBuilder = new DataContextBuilder();
            dataContextBuilder.Aggregates.Add(new AggregateSymbol("Count"));

            var dataContext = dataContextBuilder.GetResult();

            var syntaxTree = SyntaxTree.ParseQuery("SELECT COUNT(*)");
            var compilation = Compilation.Empty.WithDataContext(dataContext).WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.AmbiguousAggregate, diagnostics[0].DiagnosticId);
        }
    }
}