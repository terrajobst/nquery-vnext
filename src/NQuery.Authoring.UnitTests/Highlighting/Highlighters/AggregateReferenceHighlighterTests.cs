﻿using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class AggregateReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new SymbolReferenceHighlighter();
        }

        [TestMethod]
        public void AggregateReferenceHighlighter_Matches()
        {
            var query = @"
                SELECT  {COUNT}(*),
                        {COUNT}(e.ReportsTo)
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }
    }
}