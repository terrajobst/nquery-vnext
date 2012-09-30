using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace NQuery.Language.VSEditor.BraceMatching
{
    [Export(typeof(IBraceMatchingService))]
    internal sealed class BraceMatchingService : IBraceMatchingService
    {
        private readonly IEnumerable<IBraceMatcher> _braceMatchers;

        [ImportingConstructor]
        public BraceMatchingService([ImportMany] IEnumerable<IBraceMatcher> braceMatchers)
        {
            _braceMatchers = braceMatchers;
        }

        public bool TryFindBrace(SyntaxTree syntaxTree, int position, out TextSpan left, out TextSpan right)
        {
            var token = syntaxTree.Root.FindTokenTouched(position);
            if (TryFindBrace(token, position, out left, out right))
                return true;

            var previousToken = token.GetPreviousToken();
            if (previousToken != null && previousToken.Value.Span.End == token.Span.Start)
            {
                if (TryFindBrace(previousToken.Value, position, out left, out right))
                    return true;                
            }

            var nextToken = token.GetPreviousToken();
            if (nextToken != null && nextToken.Value.Span.Start == token.Span.End)
            {
                if (TryFindBrace(nextToken.Value, position, out left, out right))
                    return true;
            }

            return false;
        }

        private bool TryFindBrace(SyntaxToken token, int position, out TextSpan left, out TextSpan right)
        {
            left = default(TextSpan);
            right = default(TextSpan);

            foreach (var braceMatcher in _braceMatchers)
            {
                if (braceMatcher.TryFindBrace(token, position, out left, out right))
                    return true;
            }
            return false;
        }
    }
}