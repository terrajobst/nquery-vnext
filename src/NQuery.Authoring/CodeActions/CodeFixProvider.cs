namespace NQuery.Authoring.CodeActions
{
    public abstract class CodeFixProvider : ICodeFixProvider
    {
        public abstract IEnumerable<DiagnosticId> GetFixableDiagnosticIds();

        public IEnumerable<ICodeAction> GetFixes(SemanticModel semanticModel, int position)
        {
            var syntaxDiagnostics = semanticModel.SyntaxTree.GetDiagnostics();
            var semanticDiagnostics = semanticModel.GetDiagnostics();
            var diagnostics = syntaxDiagnostics.Concat(semanticDiagnostics);
            var applicableDiagnostics = diagnostics.Where(d => d.Span.ContainsOrTouches(position))
                                                   .Where(d => GetFixableDiagnosticIds().Contains(d.DiagnosticId));

            return applicableDiagnostics.SelectMany(d => GetFixes(semanticModel, position, d));
        }

        protected abstract IEnumerable<ICodeAction> GetFixes(SemanticModel semanticModel, int position, Diagnostic diagnostic);
    }
}