using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests
{
    [TestClass]
    public class ExpressionTests
    {
        [TestMethod]
        public void Expression_AllowsConstructingInvalidExpressions()
        {
            var dataContext = DataContext.Empty;
            var fooBar = "FOO BAR";

            var query = new Expression<object>(dataContext, fooBar);

            Assert.AreEqual(dataContext, query.DataContext);
            Assert.AreEqual(fooBar, query.Text);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Expression_CtorThrows_IfDataContextIsNull()
        {
            new Expression<object>(null, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Expression_CtorThrows_IfTextIsNull()
        {
            new Expression<object>(DataContext.Empty, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Expression_CtorThrows_IfTIsNotAssignableFromTargetType()
        {
            new Expression<bool>(DataContext.Empty, "", false, typeof(int));
        }

        [TestMethod]
        public void Expression_ResolveThrows_IfExpressionCannotBeParsed()
        {
            var expression = new Expression<bool>(DataContext.Empty, "foo;");

            try
            {
                expression.Resolve();
            }
            catch (CompilationException ex)
            {
                Assert.IsTrue(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.IllegalInputCharacter));
                Assert.IsTrue(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.ColumnTableOrVariableNotDeclared));
            }
        }

        [TestMethod]
        public void Expression_EvaluateThrows_IfExpressionCannotBeParsed()
        {
            var expression = new Expression<bool>(DataContext.Empty, "bar;");

            try
            {
                expression.Evaluate();
            }
            catch (CompilationException ex)
            {
                Assert.IsTrue(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.IllegalInputCharacter));
                Assert.IsTrue(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.ColumnTableOrVariableNotDeclared));
            }
        }

        [TestMethod]
        public void Expression_ResolveReturnsValue()
        {
            var expression = new Expression<double>(DataContext.Empty, "1 + 1.0");
            var result = expression.Resolve();
            Assert.AreEqual(typeof(double), result);
        }

        [TestMethod]
        public void Expression_EvaluateReturnsValue()
        {
            var expression = new Expression<double>(DataContext.Empty, "1 + 1.0");
            var result = expression.Evaluate();
            Assert.AreEqual(2.0, result);
        }

        [TestMethod]
        public void Expression_Evaluate_HonorsNullValue()
        {
            const int nullValue = 42;
            var expression = new Expression<int>(DataContext.Empty, "NULL", nullValue);
            var result = expression.Evaluate();
            Assert.AreEqual(nullValue, result);
        }
    }
}