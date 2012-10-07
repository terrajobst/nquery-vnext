using NQuery.Language.Symbols;

namespace NQuery.Language.VSEditor
{
    public sealed class QuickInfoModel
    {
        private readonly SyntaxNodeOrToken _nodeOrToken;
        private readonly Symbol _symbol;

        public QuickInfoModel(SyntaxNodeOrToken nodeOrToken, Symbol symbol)
        {
            _nodeOrToken = nodeOrToken;
            _symbol = symbol;
        }

        public SyntaxNodeOrToken NodeOrToken
        {
            get { return _nodeOrToken; }
        }

        public Symbol Symbol
        {
            get { return _symbol; }
        }
    }
}