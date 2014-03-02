using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.CodeActions
{
    [Export(typeof(ICodeRefactoringProvider))]
    internal sealed class ExpandWildcardCodeRefactoringProvider : CodeRefactoringProvider<WildcardSelectColumnSyntax>
    {
        protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, WildcardSelectColumnSyntax node)
        {
            var columnInstances = semanticModel.GetColumnInstances(node).ToArray();
            if (!columnInstances.Any())
                return Enumerable.Empty<ICodeAction>();

            return new[] {new ExpandWildcardCodeAction(node, columnInstances)};
        }

        private sealed class ExpandWildcardCodeAction : ICodeAction
        {
            private readonly WildcardSelectColumnSyntax _node;
            private readonly IReadOnlyCollection<TableColumnInstanceSymbol> _columnInstances;

            public ExpandWildcardCodeAction(WildcardSelectColumnSyntax node, IReadOnlyCollection<TableColumnInstanceSymbol> columnInstances)
            {
                _node = node;
                _columnInstances = columnInstances;
            }

            public string Description
            {
                get { return "Expand Wildcard"; }
            }

            public SyntaxTree GetEdit()
            {
                var location = _node.SyntaxTree.TextBuffer.GetTextLocation(_node.Span.Start);
                var indent = location.Column;
                var columnString = BuildColumns(indent, _columnInstances);
                return _node.SyntaxTree.ReplaceText(_node.Span, columnString);
            }

            private static string BuildColumns(int indent, IEnumerable<TableColumnInstanceSymbol> symbols)
            {
                var indentString = new string(' ', indent);

                var isFirst = true;
                var sb = new StringBuilder();

                foreach (var symbol in symbols)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sb.Append(",");
                        sb.AppendLine();
                        sb.Append(indentString);
                    }

                    var table = SyntaxFacts.GetValidIdentifier(symbol.TableInstance.Name);
                    var column = SyntaxFacts.GetValidIdentifier(symbol.Column.Name);
                    sb.Append(table);
                    sb.Append(".");
                    sb.Append(column);
                }

                return sb.ToString();
            }
        }
    }
}