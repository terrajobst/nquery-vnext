using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Data;
using NQuery.Dynamic;

namespace NQuery.UnitTests
{
    [TestClass]
    public class DynamicTests
    {
        [TestMethod]
        public void Dynamic_ExecuteDynamicSequenceAllowsLateBoundAccess()
        {
            var dataTable = TestData.IdNameTable();
            var dataContext = DataContext.Default.AddTables(dataTable);
            var query = new Query(dataContext, "SELECT * FROM Table");

            var rows = query.ExecuteDynamicSequence().ToArray();

            Assert.AreEqual(1, rows[0].Id);
            Assert.AreEqual("One", rows[0].Name);

            Assert.AreEqual(2, rows[1].Id);
            Assert.AreEqual("Two", rows[1].Name);
        }
    }
}