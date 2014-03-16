using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Issues
{
    internal sealed class ColumnsInExistsCodeIssueProvider : CodeIssueProvider<ExistsSubselectSyntax>
    {
        protected override IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel, ExistsSubselectSyntax node)
        {
            var syntaxTree = node.SyntaxTree;
            var selectQuery = node.Query as SelectQuerySyntax;
            if (selectQuery == null)
                return Enumerable.Empty<CodeIssue>();

            if (selectQuery.SelectClause.IsMissing)
                return Enumerable.Empty<CodeIssue>();

            var selectClause = selectQuery.SelectClause;
            if (IsSingleSelectStar(selectClause.Columns))
                return Enumerable.Empty<CodeIssue>();

            var start = selectClause.Columns.First().Span.Start;
            var end = selectClause.Columns.Last().Span.End;
            var span = TextSpan.FromBounds(start, end);

            var action = new[] { new RemoveUnnecessaryColumnsFromExistsCodeAction(syntaxTree, span) };

            return selectClause.Columns.GetWithSeparators().Select(c => new CodeIssue(CodeIssueKind.Unnecessary, c.Span, action));
        }

        private static bool IsSingleSelectStar(ICollection<SelectColumnSyntax> selectColumns)
        {
            if (selectColumns.Count != 1)
                return false;

            var column = selectColumns.Single();
            return column is WildcardSelectColumnSyntax;
        }

        private sealed class RemoveUnnecessaryColumnsFromExistsCodeAction : ICodeAction
        {
            private readonly SyntaxTree _syntaxTree;
            private readonly TextSpan _columnListSpan;

            public RemoveUnnecessaryColumnsFromExistsCodeAction(SyntaxTree syntaxTree, TextSpan columnListSpan)
            {
                _syntaxTree = syntaxTree;
                _columnListSpan = columnListSpan;
            }

            public string Description
            {
                get { return "Remove unnecessary columns from EXISTS"; }
            }

            public SyntaxTree GetEdit()
            {
                return _syntaxTree.ReplaceText(_columnListSpan, "*");
            }
        }
    }
}