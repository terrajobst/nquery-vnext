using System;
using System.ComponentModel.Composition;

namespace NQuery.Language.Services.BraceMatching
{
    [Export(typeof(IBraceMatcher))]
    internal sealed class StringQuoteBraceMatcher : SingleTokenBraceMatcher
    {
        public StringQuoteBraceMatcher()
            : base(SyntaxKind.StringLiteralToken)
        {
        }
    }
}