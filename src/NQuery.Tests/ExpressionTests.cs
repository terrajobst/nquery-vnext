using System;
using System.Linq;

using Xunit;

namespace NQuery.Tests
{
    public partial class ExpressionTests
    {
        [Fact]
        public void Expression_AllowsConstructingInvalidExpressions()
        {
            var dataContext = DataContext.Empty;
            var fooBar = "FOO BAR";

            var query = Expression<object>.Create(dataContext, fooBar);

            Assert.Equal(dataContext, query.DataContext);
            Assert.Equal(fooBar, query.Text);
        }

        [Fact]
        public void Expression_CtorThrows_IfDataContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Expression<object>.Create(null, string.Empty);
            });
        }

        [Fact]
        public void Expression_CtorThrows_IfTextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Expression<object>.Create(DataContext.Empty, null);
            });
        }

        [Fact]
        public void Expression_CtorThrows_IfTIsNotAssignableFromTargetType()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Expression<bool>.Create(DataContext.Empty, "", false, typeof (int));
            });
        }

        [Fact]
        public void Expression_ResolveThrows_IfExpressionCannotBeParsed()
        {
            var expression = Expression<bool>.Create(DataContext.Empty, "foo;");

            try
            {
                expression.Resolve();
            }
            catch (CompilationException ex)
            {
                Assert.True(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.IllegalInputCharacter));
                Assert.True(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.ColumnTableOrVariableNotDeclared));
            }
        }

        [Fact]
        public void Expression_EvaluateThrows_IfExpressionCannotBeParsed()
        {
            var expression = Expression<bool>.Create(DataContext.Empty, "bar;");

            try
            {
                expression.Evaluate();
            }
            catch (CompilationException ex)
            {
                Assert.True(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.IllegalInputCharacter));
                Assert.True(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.ColumnTableOrVariableNotDeclared));
            }
        }

        [Fact]
        public void Expression_ResolveReturnsValue()
        {
            var expression = Expression<double>.Create(DataContext.Empty, "1 + 1.0");
            var result = expression.Resolve();
            Assert.Equal(typeof(double), result);
        }

        [Fact]
        public void Expression_EvaluateReturnsValue()
        {
            var expression = Expression<double>.Create(DataContext.Empty, "1 + 1.0");
            var result = expression.Evaluate();
            Assert.Equal(2.0, result);
        }

        [Fact]
        public void Expression_Evaluate_HonorsNullValue()
        {
            const int nullValue = 42;
            var expression = Expression<int>.Create(DataContext.Empty, "NULL", nullValue);
            var result = expression.Evaluate();
            Assert.Equal(nullValue, result);
        }
    }
}