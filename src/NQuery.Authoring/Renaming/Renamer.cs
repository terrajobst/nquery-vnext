using System;

using NQuery.Authoring.SymbolSearch;
using NQuery.Text;

namespace NQuery.Authoring.Renaming
{
    public static class Renamer
    {
        public static bool CanBeRenamed(Symbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.DerivedTable:
                case SymbolKind.TableInstance:
                case SymbolKind.QueryColumnInstance:
                case SymbolKind.CommonTableExpression:
                    return true;

                default:
                    return false;
            }
        }

        public static Compilation RenameSymbol(this SemanticModel semanticModel, Symbol symbol, string newName)
        {
            if (semanticModel == null)
                throw new ArgumentNullException("semanticModel");

            if (symbol == null)
                throw new ArgumentNullException("symbol");

            if (newName == null)
                throw new ArgumentNullException("newName");

            if (!CanBeRenamed(symbol))
            {
                var message = string.Format("Symbol '{0}' cannot be renamed", symbol.Name);
                throw new InvalidOperationException(message);
            }

            var compilation = semanticModel.Compilation;
            var syntaxTree = compilation.SyntaxTree;

            var usages = semanticModel.FindUsages(symbol);
            var changeSet = new TextChangeSet();

            foreach (var usage in usages)
                changeSet.ReplaceText(usage.Span, newName);

            var newSyntaxTree = syntaxTree.WithChanges(changeSet);
            var newCompilation = compilation.WithSyntaxTree(newSyntaxTree);

            return newCompilation;
        }
    }
}