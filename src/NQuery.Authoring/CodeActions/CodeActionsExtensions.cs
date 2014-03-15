using System.Collections.Generic;
using System.Linq;

using NQuery.Authoring.CodeActions.Issues;
using NQuery.Authoring.CodeActions.Refactorings;

namespace NQuery.Authoring.CodeActions
{
    public static class CodeActionsExtensions
    {
        public static IEnumerable<ICodeIssueProvider> GetStandardIssueProviders()
        {
            return new ICodeIssueProvider[]
                   {
                       new ColumnsInExistsCodeIssueProvider(),
                       new ComparisonWithNullCodeIssueProvider(),
                       new OrderByExpressionsCodeIssueProvider() 
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
                       new AddMissingKeywordCodeRefactoringProvider(),
                       new ExpandWildcardCodeRefactoringProvider()
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