using System;
using System.Data;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Data;

namespace NQuery.UnitTests
{
    [TestClass]
    public class QueryTests
    {
        [TestMethod]
        public void Query_AllowsConstructingInvalidQueries()
        {
            var dataContext = DataContext.Empty;
            var fooBar = "FOO BAR";

            var query = new Query(dataContext, fooBar);

            Assert.AreEqual(dataContext, query.DataContext);
            Assert.AreEqual(fooBar, query.Text);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Query_CtorThrows_IfDataContextIsNull()
        {
            new Query(null, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Query_CtorThrows_IfTextIsNull()
        {
            new Query(DataContext.Empty, null);
        }

        [TestMethod]
        public void Query_ExecuteSchemaReaderThrows_IfQueryCannotBeParsed()
        {
            var query = new Query(DataContext.Empty, "SELECT SchemaReader;");
            try
            {
                query.ExecuteSchemaReader();
                Assert.Fail("Should have thrown an exception.");
            }
            catch (CompilationException ex)
            {
                Assert.IsTrue(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.IllegalInputCharacter));
                Assert.IsTrue(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.ColumnTableOrVariableNotDeclared));
            }
        }

        [TestMethod]
        public void Query_ExecuteReaderThrows_IfQueryCannotBeParsed()
        {
            var query = new Query(DataContext.Empty, "SELECT Reader;");
            try
            {
                query.ExecuteReader();
                Assert.Fail("Should have thrown an exception.");
            }
            catch (CompilationException ex)
            {
                Assert.IsTrue(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.IllegalInputCharacter));
                Assert.IsTrue(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.ColumnTableOrVariableNotDeclared));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void Query_ExecuteScalarOfTThrows_IfTypeDoesNotMatch()
        {
            var dataTable = TestData.IdNameTable();
            var dataContext = DataContext.Default.AddTables(dataTable);

            var query = new Query(dataContext, "SELECT * FROM Table");
            query.ExecuteScalar<string>();
        }

        [TestMethod]
        public void Query_ExecuteScalarReturnsFirstValue()
        {
            var dataTable = TestData.IdNameTable();
            var dataContext = DataContext.Default.AddTables(dataTable);

            var query = new Query(dataContext, "SELECT * FROM Table");
            var result = query.ExecuteScalar();
            var expected = dataTable.Rows[0][0];

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Query_ExecuteScalarReturnsNullIfEmpty()
        {
            var dataTable = TestData.IdNameTable();
            var dataContext = DataContext.Default.AddTables(dataTable);

            var query = new Query(dataContext, "SELECT * FROM Table WHERE FALSE");
            var result = query.ExecuteScalar();

            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void Query_ExecuteReaderReturnsAllRows()
        {
            var dataTable = new DataTable("Table");
            dataTable.Columns.Add("Value", typeof (int));
            dataTable.Rows.Add(1);
            dataTable.Rows.Add(42);

            var dataContext = DataContext.Default.AddTables(dataTable);

            var query = new Query(dataContext, "SELECT * FROM Table");
            using (var reader = query.ExecuteReader())
            {
                Assert.AreEqual(1, reader.ColumnCount);
                Assert.AreEqual("Value", reader.GetColumnName(0));
                Assert.AreEqual(typeof(int), reader.GetColumnType(0));

                Assert.IsTrue(reader.Read());
                var first = (int) reader[0];
                Assert.AreEqual(1, first);

                Assert.IsTrue(reader.Read());
                var second = (int)reader[0];
                Assert.AreEqual(42, second);

                Assert.IsFalse(reader.Read());
            }
        }

        [TestMethod]
        public void Query_ExecuteSchemaReaderReturnsNoRows()
        {
            var dataTable = new DataTable("Table");
            dataTable.Columns.Add("Value", typeof (int));
            dataTable.Rows.Add(1);
            dataTable.Rows.Add(42);

            var dataContext = DataContext.Default.AddTables(dataTable);

            var query = new Query(dataContext, "SELECT * FROM Table");
            using (var reader = query.ExecuteSchemaReader())
            {
                Assert.AreEqual(1, reader.ColumnCount);
                Assert.AreEqual("Value", reader.GetColumnName(0));
                Assert.AreEqual(typeof(int), reader.GetColumnType(0));
                Assert.IsFalse(reader.Read());
            }
        }
    }
}