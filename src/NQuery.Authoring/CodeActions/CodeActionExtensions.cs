using System.Collections.Immutable;

using NQuery.Authoring.CodeActions.Fixes;
using NQuery.Authoring.CodeActions.Issues;
using NQuery.Authoring.CodeActions.Refactorings;
using NQuery.CodeAnalysis;

namespace NQuery.Authoring.CodeActions;

public static class CodeActionExtensions
{
    public static ImmutableArray<ICodeFixProvider> StandardFixProviders { get; } =
    [
        new AddOrderByToSelectDistinctCodeFixProvider(),
        new AddParenthesesCodeFixProvider(),
        new AddToGroupByCodeFixProvider()
    ];

    public static ImmutableArray<ICodeIssueProvider> StandardIssueProviders { get; } =
    [
        new ColumnsInExistsCodeIssueProvider(),
        new ComparisonWithNullCodeIssueProvider(),
        new OrderByExpressionsCodeIssueProvider(),
        new OrderByOrdinalCodeIssueProvider(),
        new UnusedCommonTableExpressionCodeIssueProvider(),
        new RecursiveCodeIssueProvider()
    ];

    public static ImmutableArray<ICodeRefactoringProvider> StandardRefactoringProviders { get; } =
    [
        new FlipBinaryOperatorSidesCodeRefactoringProvider(),
        new SortOrderCodeRefactoringProvider(),
        new AddAsAliasCodeRefactoringProvider(),
        new AddAsDerivedTableCodeRefactoringProvider(),
        new AddMissingKeywordCodeRefactoringProvider(),
        new ExpandWildcardCodeRefactoringProvider(),
        new QualifyColumnCodeRefactoringProvider(),
        new BetweenCodeRefactoringProvider(),
        new RemoveRedundantBracketsCodeRefactoringProvider(),
        new RemoveRedundantParenthesisCodeRefactoringProvider()
    ];

    extension(SemanticModel semanticModel)
    {
        public IEnumerable<ICodeAction> GetFixes(int position)
        {
            return semanticModel.GetFixes(position, StandardFixProviders);
        }

        public IEnumerable<ICodeAction> GetFixes(int position, IEnumerable<ICodeFixProvider> providers)
        {
            return providers.SelectMany(p => p.GetFixes(semanticModel, position));
        }

        public IEnumerable<CodeIssue> GetIssues()
        {
            return semanticModel.GetIssues(StandardIssueProviders);
        }

        public IEnumerable<CodeIssue> GetIssues(IEnumerable<ICodeIssueProvider> providers)
        {
            return providers.SelectMany(p => p.GetIssues(semanticModel));
        }

        public IEnumerable<ICodeAction> GetRefactorings(int position)
        {
            return semanticModel.GetRefactorings(position, StandardRefactoringProviders);
        }

        public IEnumerable<ICodeAction> GetRefactorings(int position, IEnumerable<ICodeRefactoringProvider> providers)
        {
            return providers.SelectMany(p => p.GetRefactorings(semanticModel, position));
        }
    }
}
