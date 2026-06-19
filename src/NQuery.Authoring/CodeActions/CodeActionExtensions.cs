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

    public static IEnumerable<ICodeAction> GetFixes(this SemanticModel semanticModel, int position)
    {
        return semanticModel.GetFixes(position, StandardFixProviders);
    }

    public static IEnumerable<ICodeAction> GetFixes(this SemanticModel semanticModel, int position, IEnumerable<ICodeFixProvider> providers)
    {
        return providers.SelectMany(p => p.GetFixes(semanticModel, position));
    }

    public static ImmutableArray<ICodeIssueProvider> StandardIssueProviders { get; } =
    [
        new ColumnsInExistsCodeIssueProvider(),
        new ComparisonWithNullCodeIssueProvider(),
        new OrderByExpressionsCodeIssueProvider(),
        new OrderByOrdinalCodeIssueProvider(),
        new UnusedCommonTableExpressionCodeIssueProvider(),
        new RecursiveCodeIssueProvider()
    ];

    public static IEnumerable<CodeIssue> GetIssues(this SemanticModel semanticModel)
    {
        return semanticModel.GetIssues(StandardIssueProviders);
    }

    public static IEnumerable<CodeIssue> GetIssues(this SemanticModel semanticModel, IEnumerable<ICodeIssueProvider> providers)
    {
        return providers.SelectMany(p => p.GetIssues(semanticModel));
    }

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

    public static IEnumerable<ICodeAction> GetRefactorings(this SemanticModel semanticModel, int position)
    {
        return semanticModel.GetRefactorings(position, StandardRefactoringProviders);
    }

    public static IEnumerable<ICodeAction> GetRefactorings(this SemanticModel semanticModel, int position, IEnumerable<ICodeRefactoringProvider> providers)
    {
        return providers.SelectMany(p => p.GetRefactorings(semanticModel, position));
    }
}
