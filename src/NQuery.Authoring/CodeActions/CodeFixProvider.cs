using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Authoring.CodeActions
{
    public abstract class CodeFixProvider : ICodeRefactoringProvider
    {
        public abstract ImmutableArray<DiagnosticId> FixableDiagnosticIds { get; }

        public IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position)
        {
            var syntaxDiagnostics = semanticModel.Compilation.SyntaxTree.GetDiagnostics();
            var semanticDiagnostics = semanticModel.GetDiagnostics();
            var diagnostics = syntaxDiagnostics.Concat(semanticDiagnostics);
            var applicableDiagnostics = diagnostics.Where(d => d.Span.ContainsOrTouches(position))
                                                   .Where(d => FixableDiagnosticIds.Contains(d.DiagnosticId));
            return GetFixes(semanticModel, position, applicableDiagnostics);
        }

        protected abstract IEnumerable<ICodeAction> GetFixes(SemanticModel semanticModel, int position, IEnumerable<Diagnostic> diagnostics);
    }
}