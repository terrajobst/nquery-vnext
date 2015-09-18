using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace NQuery.Tests.Syntax
{
    partial class ParserTests
    {
        private sealed class AssertingEnumerator : IDisposable
        {
            private readonly IEnumerator<SyntaxNodeOrToken> _enumerator;
            private readonly HashSet<Diagnostic> _assertedDiagnostics = new HashSet<Diagnostic>();

            private bool _isStart = true;

            private AssertingEnumerator(IEnumerator<SyntaxNodeOrToken> enumerator)
            {
                _enumerator = enumerator;
            }

            public static AssertingEnumerator ForNode(SyntaxNode node)
            {
                var enumerator = node.DescendantNodesAndTokensAndSelf(true)
                                     .GetEnumerator();
                return new AssertingEnumerator(enumerator);
            }

            public static AssertingEnumerator ForQuery(string textWithOptionalMarker)
            {
                return ForText(textWithOptionalMarker, SyntaxTree.ParseQuery);
            }

            public static AssertingEnumerator ForExpression(string textWithOptionalMarker)
            {
                return ForText(textWithOptionalMarker, SyntaxTree.ParseExpression);
            }

            private static AssertingEnumerator ForText(string textWithOptionalMarker, Func<string, SyntaxTree> factory)
            {
                var annotatedText = AnnotatedText.Parse(textWithOptionalMarker);
                if (annotatedText.Spans.Length > 1 || annotatedText.Changes.Length > 0)
                    throw new InvalidOperationException("This method only supports text with zero or one span.");

                var syntaxTree = factory(annotatedText.Text);
                var span = annotatedText.Spans.Any()
                    ? annotatedText.Spans.Single()
                    : syntaxTree.Root.Root.Span;
                var enumerator = syntaxTree.Root.Root
                                           .DescendantNodesAndTokensAndSelf(true)
                                           .Where(n => span.Contains(n.Span))
                                           .GetEnumerator();
                return new AssertingEnumerator(enumerator);
            }

            private void CheckForMissingDiagnostics()
            {
                if (_isStart)
                {
                    _isStart = false;
                    return;
                }

                if (_enumerator.Current.IsNode)
                    return;

                var diagnostics = _enumerator.Current.AsToken().Diagnostics;
                var unexpectedDiangostics = diagnostics.Where(d => !_assertedDiagnostics.Contains(d));

                Assert.Empty(unexpectedDiangostics);

                _assertedDiagnostics.Clear();
            }

            public void Dispose()
            {
                CheckForMissingDiagnostics();

                Assert.False(_enumerator.MoveNext());

                _enumerator.Dispose();
            }

            public void AssertNode(SyntaxKind kind)
            {
                AssertNode(kind, false);
            }

            public void AssertNodeMissing(SyntaxKind kind)
            {
                AssertNode(kind, true);
            }

            private void AssertNode(SyntaxKind kind, bool isMissing)
            {
                CheckForMissingDiagnostics();

                Assert.True(_enumerator.MoveNext());
                Assert.Equal(kind, _enumerator.Current.Kind);
                Assert.True(_enumerator.Current.IsNode);
                Assert.Equal(isMissing, _enumerator.Current.IsMissing);
            }

            public void AssertToken(SyntaxKind kind, string text)
            {
                CheckForMissingDiagnostics();

                Assert.True(_enumerator.MoveNext());
                Assert.Equal(kind, _enumerator.Current.Kind);
                Assert.True(_enumerator.Current.IsToken);

                var token = _enumerator.Current.AsToken();
                var sourceText = token.Parent.SyntaxTree.Text;

                Assert.False(token.IsMissing);
                Assert.Equal(text, token.Text);
                Assert.Equal(text, sourceText.GetText(token.Span));
            }

            public void AssertTokenMissing(SyntaxKind kind)
            {
                CheckForMissingDiagnostics();

                Assert.True(_enumerator.MoveNext());
                Assert.Equal(kind, _enumerator.Current.Kind);
                Assert.True(_enumerator.Current.IsToken);

                var token = _enumerator.Current.AsToken();

                Assert.True(token.IsMissing);
                Assert.Equal(0, token.Span.Length);
            }

            public void AssertDiagnostic(DiagnosticId diagnosticId, string text)
            {
                Assert.True(_enumerator.Current.IsToken);

                var token = _enumerator.Current.AsToken();
                var diagnostic = Assert.Single(token.Diagnostics, d => d.DiagnosticId == diagnosticId);

                Assert.Equal(text, diagnostic.Message);

                _assertedDiagnostics.Add(diagnostic);
            }
        }
    }
}