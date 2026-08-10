using System.Collections.Immutable;

using NQuery.Authoring.CodeActions.Fixes;
using NQuery.Authoring.CodeActions.Issues;
using NQuery.Authoring.CodeActions.Refactorings;

namespace NQuery.Authoring.CodeActions;

public static class CodeActionExtensions
{
    private static ImmutableArray<ICodeFixProvider> StandardCodeFixProviders { get; } =
    [
        new AddOrderByToSelectDistinctCodeFixProvider(),
        new AddParenthesesCodeFixProvider(),
        new AddToGroupByCodeFixProvider()
    ];

    private static ImmutableArray<ICodeIssueProvider> StandardCodeIssueProviders { get; } =
    [
        new ColumnsInExistsCodeIssueProvider(),
        new ComparisonWithNullCodeIssueProvider(),
        new OrderByExpressionsCodeIssueProvider(),
        new OrderByOrdinalCodeIssueProvider(),
        new UnusedCommonTableExpressionCodeIssueProvider(),
        new RecursiveCodeIssueProvider()
    ];

    private static ImmutableArray<ICodeRefactoringProvider> StandardCodeRefactoringProviders { get; } =
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

    extension(AuthoringServicesBuilder builder)
    {
        public AuthoringServicesBuilder AddCodeFixService()
        {
            ThrowIfNull(builder);

            return builder.AddService(s => new CodeFixService(s.GetProviders<ICodeFixProvider>()));
        }

        public AuthoringServicesBuilder AddCodeIssueService()
        {
            ThrowIfNull(builder);

            return builder.AddService(s => new CodeIssueService(s.GetProviders<ICodeIssueProvider>()));
        }

        public AuthoringServicesBuilder AddCodeRefactoringService()
        {
            ThrowIfNull(builder);

            return builder.AddService(s => new CodeRefactoringService(s.GetProviders<ICodeRefactoringProvider>()));
        }

        public AuthoringServicesBuilder AddCodeFixProvider(ICodeFixProvider provider)
        {
            ThrowIfNull(builder);

            return builder.AddProvider<ICodeFixProvider>(provider);
        }

        public AuthoringServicesBuilder AddCodeIssueProvider(ICodeIssueProvider provider)
        {
            ThrowIfNull(builder);

            return builder.AddProvider<ICodeIssueProvider>(provider);
        }

        public AuthoringServicesBuilder AddCodeRefactoringProvider(ICodeRefactoringProvider provider)
        {
            ThrowIfNull(builder);

            return builder.AddProvider<ICodeRefactoringProvider>(provider);
        }

        public AuthoringServicesBuilder AddStandardCodeFixProviders()
        {
            ThrowIfNull(builder);

            return builder.AddProviders(StandardCodeFixProviders);
        }

        public AuthoringServicesBuilder AddStandardCodeIssueProviders()
        {
            ThrowIfNull(builder);

            return builder.AddProviders(StandardCodeIssueProviders);
        }

        public AuthoringServicesBuilder AddStandardCodeRefactoringProviders()
        {
            ThrowIfNull(builder);

            return builder.AddProviders(StandardCodeRefactoringProviders);
        }
    }
}
