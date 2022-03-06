using NQuery.Authoring.CodeActions.Fixes;
using NQuery.Authoring.CodeActions.Issues;
using NQuery.Authoring.CodeActions.Refactorings;

namespace NQuery.Authoring.CodeActions
{
    public static class CodeActionExtensions
    {
        public static IEnumerable<ICodeFixProvider> GetStandardFixProviders()
        {
            return new ICodeFixProvider[]
                   {
                       new AddOrderByToSelectDistinctCodeFixProvider(),
                       new AddParenthesesCodeFixProvider(),
                       new AddToGroupByCodeFixProvider()
                   };
        }

        public static IEnumerable<ICodeAction> GetFixes(this SemanticModel semanticModel, int position)
        {
            var providers = GetStandardFixProviders();
            return semanticModel.GetFixes(position, providers);
        }

        public static IEnumerable<ICodeAction> GetFixes(this SemanticModel semanticModel, int position, IEnumerable<ICodeFixProvider> providers)
        {
            return providers.SelectMany(p => p.GetFixes(semanticModel, position));
        }

        public static IEnumerable<ICodeIssueProvider> GetStandardIssueProviders()
        {
            return new ICodeIssueProvider[]
                   {
                       new ColumnsInExistsCodeIssueProvider(),
                       new ComparisonWithNullCodeIssueProvider(),
                       new OrderByExpressionsCodeIssueProvider(),
                       new OrderByOrdinalCodeIssueProvider(),
                       new UnusedCommonTableExpressionCodeIssueProvider(),
                       new RecursiveCodeIssueProvider(),
                       new UnusedQueryColumnCodeIssueProvider()
                   };
        }

        public static IEnumerable<CodeIssue> GetIssues(this SemanticModel semanticModel)
        {
            var providers = GetStandardIssueProviders();
            return semanticModel.GetIssues(providers);
        }

        public static IEnumerable<CodeIssue> GetIssues(this SemanticModel semanticModel, IEnumerable<ICodeIssueProvider> providers)
        {
            return providers.SelectMany(p => p.GetIssues(semanticModel));
        }

        public static IEnumerable<ICodeRefactoringProvider> GetStandardRefactoringProviders()
        {
            return new ICodeRefactoringProvider[]
                   {
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
                   };
        }

        public static IEnumerable<ICodeAction> GetRefactorings(this SemanticModel semanticModel, int position)
        {
            var providers = GetStandardRefactoringProviders();
            return semanticModel.GetRefactorings(position, providers);
        }

        public static IEnumerable<ICodeAction> GetRefactorings(this SemanticModel semanticModel, int position, IEnumerable<ICodeRefactoringProvider> providers)
        {
            return providers.SelectMany(p => p.GetRefactorings(semanticModel, position));
        }
    }
}