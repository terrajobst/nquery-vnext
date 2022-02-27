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
            if (selectQuery is null)
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

        private static bool IsSingleSelectStar(IReadOnlyCollection<SelectColumnSyntax> selectColumns)
        {
            if (selectColumns.Count != 1)
                return false;

            var column = selectColumns.Single();
            return column is WildcardSelectColumnSyntax;
        }

        private sealed class RemoveUnnecessaryColumnsFromExistsCodeAction : CodeAction
        {
            private readonly TextSpan _columnListSpan;

            public RemoveUnnecessaryColumnsFromExistsCodeAction(SyntaxTree syntaxTree, TextSpan columnListSpan)
                : base(syntaxTree)
            {
                _columnListSpan = columnListSpan;
            }

            public override string Description
            {
                get { return Resources.CodeActionRemoveUnnecessaryColumnsFromExists; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                changeSet.ReplaceText(_columnListSpan, @"*");
            }
        }
    }
}