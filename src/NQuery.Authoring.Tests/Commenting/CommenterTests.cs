﻿using NQuery.Text;

namespace NQuery.Authoring.Tests.Commenting
{
    public abstract class CommenterTests
    {
        protected abstract SyntaxTree ToggleComment(SyntaxTree syntaxTree, TextSpan span);

        protected void AssertIsMatch(string queryWithMarkers, string expectedQuery)
        {
            var query = queryWithMarkers.NormalizeCode()
                                        .ParseSingleSpan(out var selection);

            var syntaxTree = SyntaxTree.ParseQuery(query);
            var actualTree = ToggleComment(syntaxTree, selection);

            var actualQuery = actualTree.Text.GetText();

            expectedQuery = expectedQuery.NormalizeCode();

            Assert.Equal(expectedQuery, actualQuery);
        }
    }
}