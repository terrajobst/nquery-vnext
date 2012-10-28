using System;
using System.ComponentModel.Composition;

namespace NQuery.Authoring.BraceMatching
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