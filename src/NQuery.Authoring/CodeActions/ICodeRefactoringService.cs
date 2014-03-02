using System;
using System.Collections.Generic;

namespace NQuery.Authoring.CodeActions
{
    public interface ICodeRefactoringService
    {
        IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position);
    }
}