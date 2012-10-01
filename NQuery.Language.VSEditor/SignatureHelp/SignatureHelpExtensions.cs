using System.Linq;

namespace NQuery.Language.VSEditor.SignatureHelp
{
    internal static class SignatureHelpExtensions
    {
        public static int GetParameterIndex(this ArgumentListSyntax argumentList, int position)
        {
            var separators = argumentList.Arguments.GetSeparators();
            return separators.TakeWhile(s => !s.IsMissing && s.Span.End <= position).Count();
        }
    }
}