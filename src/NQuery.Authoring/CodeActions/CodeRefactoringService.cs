using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace NQuery.Authoring.CodeActions
{
    [Export(typeof(ICodeRefactoringService))]
    internal sealed class CodeRefactoringService : ICodeRefactoringService
    {
        [ImportMany]
        public IEnumerable<ICodeRefactoringProvider> RefactoringProviders { get; set; }

        public IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position)
        {
            return RefactoringProviders.SelectMany(p => p.GetRefactorings(semanticModel, position));
        }
    }
}