using NQuery.Language.Symbols;

namespace NQuery.Language.VSEditor
{
    internal abstract class ExpressionQuickInfoModelProvider<T> : QuickInfoModelProvider<T>
        where T : ExpressionSyntax
    {
        protected override Symbol GetSymbol(SemanticModel semanticModel, int position, T node)
        {
            return semanticModel.GetSymbol(node);
        }
    }
}