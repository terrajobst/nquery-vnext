using System;
using System.Collections.Generic;
using System.Linq;

namespace NQueryViewer.Syntax
{
    public abstract class SyntaxNode
    {
        // TODO: Should be abstract
        public virtual IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            return Enumerable.Empty<SyntaxNodeOrToken>();
        }

        // TODO: Should be abstract
        public virtual SyntaxKind Kind
        {
            get{ return SyntaxKind.BadToken;}
        }

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