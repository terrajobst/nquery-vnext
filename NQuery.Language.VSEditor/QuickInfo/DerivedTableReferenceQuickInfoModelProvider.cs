using System.ComponentModel.Composition;
using NQuery.Language.Symbols;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class DerivedTableReferenceQuickInfoModelProvider : QuickInfoModelProvider<DerivedTableReferenceSyntax>
    {
        protected override bool IsMatch(SemanticModel semanticModel, int position, DerivedTableReferenceSyntax node)
        {
            return node.Name.Span.Contains(position);
        }

        protected override Symbol GetSymbol(SemanticModel semanticModel, int position, DerivedTableReferenceSyntax node)
        {
            return semanticModel.GetDeclaredSymbol(node);
        }
    }
}