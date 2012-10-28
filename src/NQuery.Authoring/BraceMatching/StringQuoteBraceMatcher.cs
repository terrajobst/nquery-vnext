using System;
using System.ComponentModel.Composition;

namespace NQuery.Authoring.BraceMatching
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