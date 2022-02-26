using NQuery.Symbols;

using Xunit;

namespace NQuery.Tests
{
    public partial class ExpressionTests
    {
        [Fact]
        public void Expression_Queries_SingleRowSubselect()
        {
            var dataContext = NorthwindDataContext.Instance;
            var text = "(SELECT LastName FROM Employees WHERE FirstName = 'Margaret')";
            var expression = Expression<string>.Create(dataContext, text);
            var result = expression.Evaluate();

            Assert.Equal("Peacock", result);
        }

        [Fact]
        public void Expression_Queries_Exists()
        {
            var dataContext = NorthwindDataContext.Instance;
            var text = "EXISTS (SELECT * FROM Employees WHERE FirstName = 'Margaret')";
            var expression = Expression<bool>.Create(dataContext, text);
            var result = expression.Evaluate();

            Assert.True(result);
        }

        [Fact]
        public void Expression_Queries_Exists_NoFilter()
        {
            var dataContext = NorthwindDataContext.Instance;
            var text = "EXISTS (SELECT * FROM Employees)";
            var expression = Expression<bool>.Create(dataContext, text);
            var result = expression.Evaluate();

            Assert.True(result);
        }

        [Fact]
        public void Expression_Queries_All()
        {
            var dataContext = NorthwindDataContext.Instance;
            var text = "10 >= ALL (SELECT EmployeeId FROM Employees)";
            var expression = Expression<bool>.Create(dataContext, text);
            var result = expression.Evaluate();

            Assert.True(result);
        }

        [Fact]
        public void Expression_Queries_Any()
        {
            var name = new VariableSymbol("name", typeof(string), "Margaret");
            var dataContext = NorthwindDataContext.Instance.AddVariables(name);
            var text = "'London' = ANY (SELECT City FROM Employees)";
            var expression = Expression<bool>.Create(dataContext, text);
            var result = expression.Evaluate();

            Assert.True(result);
        }
    }
}