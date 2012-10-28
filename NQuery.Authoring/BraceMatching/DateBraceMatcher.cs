using System;
using System.ComponentModel.Composition;

namespace NQuery.Language.Services.BraceMatching
{
    [Export(typeof(IBraceMatcher))]
    internal sealed class DateBraceMatcher : SingleTokenBraceMatcher
    {
        public DateBraceMatcher()
            : base(SyntaxKind.DateLiteralToken)
        {
        }
    }
}