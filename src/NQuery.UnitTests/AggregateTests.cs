using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Hosting;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.UnitTests
{
    [TestClass]
    public class AggregateTests
    {
        [TestMethod]
        public void Aggregate_DetectsAmbiguityBetweenAggregates()
        {
            var dataContext = DataContext.Default.AddAggregates(new AggregateSymbol("Agg"), new AggregateSymbol("AGG"));

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
            var dataContext = DataContext.Default
                                         .AddAggregates(new AggregateSymbol("AGG"))
                                         .AddFunctions(new FunctionSymbol<string, string>("AGG", x => x));

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
            var dataContext = DataContext.Default
                                         .AddAggregates(new AggregateSymbol("AGG"))
                                         .AddFunctions(new FunctionSymbol<string, string, string>("AGG", (x, y) => x));

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
            var dataContext = DataContext.Default;
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
            var countAggregated = DataContext.Default.Aggregates.Single(a => a.Name == "COUNT");
            var dataContext = DataContext.Default.RemoveAggregates(countAggregated);

            var syntaxTree = SyntaxTree.ParseQuery("SELECT COUNT(*)");
            var compilation = Compilation.Empty.WithDataContext(dataContext).WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UndeclaredAggregate, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Aggregate_DetectsAmbiguityBetweenCountAggregates()
        {
            var dataContext = DataContext.Default.AddAggregates(new AggregateSymbol("Count"));

            var syntaxTree = SyntaxTree.ParseQuery("SELECT COUNT(*)");
            var compilation = Compilation.Empty.WithDataContext(dataContext).WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.AmbiguousAggregate, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Aggregate_DetectsInvalidContextForAggregate()
        {
            var syntaxTree = SyntaxTree.ParseExpression("COUNT(4)");
            var compilation = Compilation.Empty.WithDataContext(DataContext.Default).WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.AggregateInvalidInCurrentContext, diagnostics[0].DiagnosticId);
        }
    }
}