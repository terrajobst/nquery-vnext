using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Language
{
    public abstract class SyntaxNode
    {
        private TextSpan? _span;
        private TextSpan? _fullSpan;

        public abstract IEnumerable<SyntaxNodeOrToken> GetChildren();

        private TextSpan ComputeSpan()
        {
            if (!GetChildren().Any())
                return new TextSpan(0, 0);
            
            var start = GetChildren().First().Span.Start;
            var end = GetChildren().Last().Span.End;
            return TextSpan.FromBounds(start, end);
        }

        private TextSpan ComputeFullSpan()
        {
            if (!GetChildren().Any())
                return new TextSpan(0, 0);
            
            var start = GetChildren().First().FullSpan.Start;
            var end = GetChildren().Last().FullSpan.End;
            return TextSpan.FromBounds(start, end);
        }

        public abstract SyntaxKind Kind { get; }

        public TextSpan Span
        {
            get
            {
                if (_span == null)
                    _span = ComputeSpan();

                return _span.Value;
            }
        }

        public TextSpan FullSpan
        {
            get
            {
                if (_fullSpan == null)
                    _fullSpan = ComputeFullSpan();

                return _fullSpan.Value;
            }
        }
    }
}