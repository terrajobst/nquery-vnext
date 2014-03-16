using System;
using System.Linq;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.UnitTests.CodeActions
{
    public abstract class RefactoringTests
    {
        protected ICodeAction[] GetActions(string query)
        {
            int position;
            var compilation = CompilationFactory.CreateQuery(query, out position);
            var semanticModel = compilation.GetSemanticModel();

            var provider = CreateProvider();
            var providers = new[] {provider};
            return semanticModel.GetRefactorings(position, providers).ToArray();
        }

        protected abstract ICodeRefactoringProvider CreateProvider();
    }
}