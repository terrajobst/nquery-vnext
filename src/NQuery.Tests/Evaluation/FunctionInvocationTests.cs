using NQuery.Symbols;

namespace NQuery.Tests.Evaluation
{
    public sealed class FunctionInvocationTests : EvaluationTest
    {
        private readonly DataContext _dataContext;

        public FunctionInvocationTests()
        {
            _dataContext = DataContext.Default.AddFunctions(
                new FunctionSymbol<int>(nameof(StaticFunction0), StaticFunction0),
                new FunctionSymbol<int, int>(nameof(StaticFunction1), StaticFunction1),
                new FunctionSymbol<int, int, int>(nameof(StaticFunction2), StaticFunction2),
                new FunctionSymbol<int, int, int, int>(nameof(StaticFunction3), StaticFunction3),
                new FunctionSymbol<int>(nameof(InstanceFunction0), InstanceFunction0),
                new FunctionSymbol<int, int>(nameof(InstanceFunction1), InstanceFunction1),
                new FunctionSymbol<int, int, int>(nameof(InstanceFunction2), InstanceFunction2),
                new FunctionSymbol<int, int, int, int>(nameof(InstanceFunction3), InstanceFunction3)
            );
        }

        [Theory]
        [InlineData(nameof(StaticFunction0) + "()")]
        [InlineData(nameof(StaticFunction1) + "(42)")]
        [InlineData(nameof(StaticFunction2) + "(40, 2)")]
        [InlineData(nameof(StaticFunction3) + "(20, 20, 2)")]
        public void Evaluation_FunctionInvocationExpression_StaticFunction(string functionInvocation)
        {
            var text = "SELECT " + functionInvocation;

            var expected = new[] { 42 };

            AssertProduces(text, expected, _dataContext);
        }

        [Theory]
        [InlineData(nameof(InstanceFunction0) + "()")]
        [InlineData(nameof(InstanceFunction1) + "(42)")]
        [InlineData(nameof(InstanceFunction2) + "(40, 2)")]
        [InlineData(nameof(InstanceFunction3) + "(20, 20, 2)")]
        public void Evaluation_FunctionInvocationExpression_InstanceFunction(string functionInvocation)
        {
            var text = "SELECT " + functionInvocation;

            var expected = new[] { GetHashCode() + 42 };

            AssertProduces(text, expected, _dataContext);
        }

        private static int StaticFunction0()
        {
            return 42;
        }

        private static int StaticFunction1(int arg)
        {
            return arg;
        }

        private static int StaticFunction2(int arg1, int arg2)
        {
            return arg1 + arg2;
        }

        private static int StaticFunction3(int arg1, int arg2, int arg3)
        {
            return arg1 + arg2 + arg3;
        }

        private int InstanceFunction0()
        {
            return GetHashCode() + 42;
        }

        private int InstanceFunction1(int arg)
        {
            return GetHashCode() + arg;
        }

        private int InstanceFunction2(int arg1, int arg2)
        {
            return GetHashCode() + arg1 + arg2;
        }

        private int InstanceFunction3(int arg1, int arg2, int arg3)
        {
            return GetHashCode() + arg1 + arg2 + arg3;
        }
    }
}
