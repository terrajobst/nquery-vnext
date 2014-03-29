using System;
using System.Collections.Immutable;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Data.Samples;

namespace NQuery.Dynamic.UnitTests
{
    [TestClass]
    public class DynamicTests
    {
        [TestMethod]
        public void Dynamic_ExecuteDynamicSequenceAllowsLateBoundAccess()
        {
            var text = @"
                SELECT  c.CategoryID,
                        c.CategoryName
                FROM    Categories c
                WHERE   c.CategoryID < 3
                ORDER   BY 1
            ";

            var dataContext = DataContextFactory.CreateNorthwind();
            var query = new Query(dataContext, text);

            var rows = query.ExecuteDynamicSequence().ToImmutableArray();

            Assert.AreEqual(1, rows[0].CategoryID);
            Assert.AreEqual("Beverages", rows[0].CategoryName);

            Assert.AreEqual(2, rows[1].CategoryID);
            Assert.AreEqual("Condiments", rows[1].CategoryName);
        }
    }
}