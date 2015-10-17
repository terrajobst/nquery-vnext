using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Symbols;
using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Fixes
{
    internal sealed class UnresolvedNameCodeFixProvider : CodeFixProvider
    {
        public override IEnumerable<DiagnosticId> GetFixableDiagnosticIds()
        {
            return new[]
            {
                DiagnosticId.UndeclaredTable,
                DiagnosticId.UndeclaredTableInstance,
                DiagnosticId.UndeclaredVariable,
                DiagnosticId.UndeclaredFunction,
                DiagnosticId.UndeclaredMethod,
                DiagnosticId.UndeclaredColumn,
                DiagnosticId.UndeclaredProperty,
                DiagnosticId.ColumnTableOrVariableNotDeclared,
            };
        }

        protected override IEnumerable<ICodeAction> GetFixes(SemanticModel semanticModel, int position, Diagnostic diagnostic)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindTokenOnLeft(position);
            if (token.Kind != SyntaxKind.IdentifierToken)
                return Enumerable.Empty<ICodeAction>();

            var candidates = GetCandidates(semanticModel, position, diagnostic, token);
            return candidates.Select(c => new FixIdentifierCodeAction(token, c));
        }

        private static IEnumerable<string> GetCandidates(SemanticModel semanticModel, int position, Diagnostic diagnostic, SyntaxToken token)
        {
            const int maxDistance = 3;

            var identifier = token.ValueText;
            return GetSymbols(semanticModel, position, token, diagnostic.DiagnosticId)
                .Where(s => Math.Abs(s.Name.Length - identifier.Length) <= maxDistance)
                .Select(s => Tuple.Create(s, ComputeLevenshteinDistance(s.Name, identifier)))
                .Where(t => t.Item2 <= maxDistance)
                .OrderBy(t => t.Item2)
                .ThenBy(t => t.Item1.Name)
                .Select(t => SyntaxFacts.GetValidIdentifier(t.Item1.Name))
                .Distinct()
                .Take(10);
        }

        private static IEnumerable<Symbol> GetSymbols(SemanticModel semanticModel, int position, SyntaxToken token, DiagnosticId diagnosticId)
        {
            switch (diagnosticId)
            {
                case DiagnosticId.UndeclaredTable:
                    return GetTables(semanticModel);
                case DiagnosticId.UndeclaredTableInstance:
                    return GetTableInstances(semanticModel, position);
                case DiagnosticId.UndeclaredVariable:
                    return GetVariables(semanticModel);
                case DiagnosticId.UndeclaredFunction:
                    return GetFunctions(semanticModel);
                case DiagnosticId.UndeclaredMethod:
                    return GetMethods(semanticModel, token);
                case DiagnosticId.UndeclaredColumn:
                    return GetColumns(semanticModel, token);
                case DiagnosticId.UndeclaredProperty:
                    return GetProperties(semanticModel, token);
                case DiagnosticId.ColumnTableOrVariableNotDeclared:
                    return GetColumnsTablesAndVariables(semanticModel, position);
                default:
                    return Enumerable.Empty<Symbol>();
            }
        }

        private static IEnumerable<Symbol> GetTables(SemanticModel semanticModel)
        {
            return semanticModel.Compilation.DataContext.Tables;
        }

        private static IEnumerable<Symbol> GetTableInstances(SemanticModel semanticModel, int position)
        {
            return semanticModel.LookupSymbols(position).OfType<TableInstanceSymbol>();
        }

        private static IEnumerable<Symbol> GetVariables(SemanticModel semanticModel)
        {
            return semanticModel.Compilation.DataContext.Variables;
        }

        private static IEnumerable<Symbol> GetFunctions(SemanticModel semanticModel)
        {
            return semanticModel.Compilation.DataContext.Functions;
        }

        private static IEnumerable<Symbol> GetMethods(SemanticModel semanticModel, SyntaxToken token)
        {
            var methodInvocation = token.Parent.AncestorsAndSelf().OfType<MethodInvocationExpressionSyntax>().First();
            var type = semanticModel.GetExpressionType(methodInvocation.Target);
            if (type.IsError())
                return Enumerable.Empty<Symbol>();

            return semanticModel.LookupMethods(type);
        }

        private static IEnumerable<Symbol> GetColumns(SemanticModel semanticModel, SyntaxToken token)
        {
            var methodInvocation = token.Parent.AncestorsAndSelf().OfType<PropertyAccessExpressionSyntax>().First();
            var table = semanticModel.GetSymbol(methodInvocation.Target) as TableInstanceSymbol;
            if (table == null)
                return Enumerable.Empty<Symbol>();

            return table.ColumnInstances;
        }

        private static IEnumerable<Symbol> GetProperties(SemanticModel semanticModel, SyntaxToken token)
        {
            var methodInvocation = token.Parent.AncestorsAndSelf().OfType<PropertyAccessExpressionSyntax>().First();
            var type = semanticModel.GetExpressionType(methodInvocation.Target);
            if (type.IsError())
                return Enumerable.Empty<Symbol>();

            return semanticModel.LookupProperties(type);
        }

        private static IEnumerable<Symbol> GetColumnsTablesAndVariables(SemanticModel semanticModel, int position)
        {
            return semanticModel.LookupSymbols(position)
                                .Where(s => s.Kind == SymbolKind.QueryColumnInstance ||
                                            s.Kind == SymbolKind.TableColumnInstance ||
                                            s.Kind == SymbolKind.TableInstance ||
                                            s.Kind == SymbolKind.Variable);
        }

        private static int ComputeLevenshteinDistance(string x, string y)
        {
            var n = x.Length;
            var m = y.Length;
            var d = new int[n + 1, m + 1];

            if (n == 0)
                return m;

            if (m == 0)
                return n;

            for (var i = 0; i <= n; i++)
                d[i, 0] = i;

            for (var j = 0; j <= m; j++)
                d[0, j] = j++;

            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j <= m; j++)
                {
                    var cost = (y[j - 1] == x[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        private sealed class FixIdentifierCodeAction : CodeAction
        {
            private readonly SyntaxToken _token;
            private readonly string _newName;

            public FixIdentifierCodeAction(SyntaxToken token, string newName)
                : base(token.Parent.SyntaxTree)
            {
                _token = token;
                _newName = newName;
            }

            public override string Description => $"Use {_newName}";

            protected override void GetChanges(TextChangeSet changeSet)
            {
                changeSet.ReplaceText(_token.Span, _newName);
            }
        }
    }
}