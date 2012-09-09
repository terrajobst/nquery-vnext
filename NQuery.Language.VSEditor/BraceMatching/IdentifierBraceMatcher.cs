using System.ComponentModel.Composition;

namespace NQuery.Language.VSEditor.BraceMatching
{
    [Export(typeof(IBraceMatcher))]
    internal sealed class IdentifierBraceMatcher : SingleTokenBraceMatcher
    {
        public IdentifierBraceMatcher()
            : base(SyntaxKind.IdentifierToken)
        {
        }

        public override bool TryFindBrace(SyntaxToken token, int position, out TextSpan left, out TextSpan right)
        {
            var isQuoted = token.Text.StartsWith("\"");
            var isBracketed = token.Text.StartsWith("[");
            var hasBraces = isQuoted || isBracketed;

            if (!hasBraces)
            {
                left = default(TextSpan);
                right = default(TextSpan);
                return false;
            }

            return base.TryFindBrace(token, position, out left, out right);
        }
    }
}