using System.Collections.Immutable;

using NQuery.Authoring.Selection;

namespace NQuery.Authoring.Tests.Selection
{
    public abstract class SelectionSpanProviderTests
    {
        protected abstract ISelectionSpanProvider CreateProvider();

        protected void AssertIsMatch(string queryWithMarkers)
        {
            var query = queryWithMarkers.NormalizeCode()
                                        .ParseSpans(out var spans);

            var syntaxTree = SyntaxTree.ParseQuery(query);

            var provider = CreateProvider();
            var providers = ImmutableArray.Create(provider);

            var childParent = spans.Zip(spans.Skip(1), (c, p) => new { Child = c, Parent = p });

            foreach (var cp in childParent)
            {
                var child = cp.Child;
                var parent = cp.Parent;

                var actual = syntaxTree.ExtendSelection(child, providers);
                Assert.Equal(parent, actual);
            }
        }
    }
}