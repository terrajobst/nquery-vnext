using System.Collections.Immutable;
using System.Text;

using NQuery.Symbols;
using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Refactorings
{
    internal sealed class ExpandWildcardCodeRefactoringProvider : CodeRefactoringProvider<WildcardSelectColumnSyntax>
    {
        protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, WildcardSelectColumnSyntax node)
        {
            var columnInstances = semanticModel.GetColumnInstances(node).ToImmutableArray();
            if (!columnInstances.Any())
                return Enumerable.Empty<ICodeAction>();

            return new[] {new ExpandWildcardCodeAction(node, columnInstances)};
        }

        private sealed class ExpandWildcardCodeAction : CodeAction
        {
            private readonly WildcardSelectColumnSyntax _node;
            private readonly ImmutableArray<TableColumnInstanceSymbol> _columnInstances;

            public ExpandWildcardCodeAction(WildcardSelectColumnSyntax node, ImmutableArray<TableColumnInstanceSymbol> columnInstances)
                : base(node.SyntaxTree)
            {
                _node = node;
                _columnInstances = columnInstances;
            }

            public override string Description
            {
                get { return Resources.CodeActionExpandWildcard; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var location = _node.SyntaxTree.Text.GetTextLocation(_node.Span.Start);
                var indent = location.Column;
                var columnString = BuildColumns(indent, _columnInstances);
                changeSet.ReplaceText(_node.Span, columnString);
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
                        sb.Append(@",");
                        sb.AppendLine();
                        sb.Append(indentString);
                    }

                    var table = SyntaxFacts.GetValidIdentifier(symbol.TableInstance.Name);
                    var column = SyntaxFacts.GetValidIdentifier(symbol.Column.Name);
                    sb.Append(table);
                    sb.Append(@".");
                    sb.Append(column);
                }

                return sb.ToString();
            }
        }
    }
}