using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Language
{
    public abstract class SyntaxNode
    {
        public abstract IEnumerable<SyntaxNodeOrToken> GetChildren();

        public abstract SyntaxKind Kind { get; }

        public TextSpan Span
        {
            get
            {
                if (!GetChildren().Any())
                    return new TextSpan(0, 0);

                var start = GetChildren().First().Span.Start;
                var end = GetChildren().Last().Span.End;
                return TextSpan.FromBounds(start, end);
            }
        }

        public TextSpan FullSpan
        {
            get
            {
                if (!GetChildren().Any())
                    return new TextSpan(0, 0);

                var start = GetChildren().First().FullSpan.Start;
                var end = GetChildren().Last().FullSpan.End;
                return TextSpan.FromBounds(start, end);
            }
        }
    }
}