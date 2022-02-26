using NQuery.Symbols;

using Xunit;

namespace NQuery.Tests
{
    public partial class ExpressionTests
    {
        private static object EvaluateBinary(string op, Type leftType, Type rightType, object left, object right)
        {
            var leftVariable = new VariableSymbol("left", leftType, left);
            var rightVariable = new VariableSymbol("right", rightType, right);
            var dataContext = DataContext.Default.AddVariables(leftVariable, rightVariable);
            var text = leftVariable.Name + " " + op + " " + rightVariable.Name;
            var expression = Expression<object>.Create(dataContext, text);
            return expression.Evaluate();
        }

        public static IEnumerable<object[]> GetOverloadableBinaryOperators()
        {
            return new[]
            {
                new object[] {"*"},
                new object[] {"/"},
                new object[] {"%"},
                new object[] {"+"},
                new object[] {"-"},
                new object[] {"="},
                new object[] {"!="},
                new object[] {"<"},
                new object[] {"<="},
                new object[] {">"},
                new object[] {">="},
                new object[] {"^"},
                new object[] {"&"},
                new object[] {"|"}
            };
        }

        public static IEnumerable<object[]> GetOverloadableShiftingOperators()
        {
            return new[]
            {
                new object[] {"<<"},
                new object[] {">>"}
            };
        }

        // All overloadable operators with custom structs and classes

        [Theory]
        [MemberData(nameof(GetOverloadableBinaryOperators))]
        public void Expression_Binary_Overloadable_Struct(string op)
        {
            var left = new MockedOperatorStruct("left");
            var right = new MockedOperatorStruct("right");
            var type = left.GetType();
            var result = EvaluateBinary(op, type, type, left, right);
            Assert.Equal(new MockedOperatorStruct(left + " " + op + " " + right), result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableBinaryOperators))]
        public void Expression_Binary_Overloadable_Struct_Null_Left(string op)
        {
            var right = new MockedOperatorStruct("right");
            var type = right.GetType();
            var result = EvaluateBinary(op, type, type, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableBinaryOperators))]
        public void Expression_Binary_Overloadable_Struct_Null_Right(string op)
        {
            var left = new MockedOperatorStruct("left");
            var type = left.GetType();
            var result = EvaluateBinary(op, type, type, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableBinaryOperators))]
        public void Expression_Binary_Overloadable_Struct_Null_Both(string op)
        {
            var type = typeof(MockedOperatorStruct);
            var result = EvaluateBinary(op, type, type, null, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableBinaryOperators))]
        public void Expression_Binary_Overloadable_Class(string op)
        {
            var left = new MockedOperatorClass("left");
            var right = new MockedOperatorClass("right");
            var type = left.GetType();
            var result = EvaluateBinary(op, type, type, left, right);
            Assert.Equal(new MockedOperatorClass(left + " " + op + " " + right), result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableBinaryOperators))]
        public void Expression_Binary_Overloadable_Class_Null_Left(string op)
        {
            var right = new MockedOperatorClass("right");
            var type = right.GetType();
            var result = EvaluateBinary(op, type, type, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableBinaryOperators))]
        public void Expression_Binary_Overloadable_Class_Null_Right(string op)
        {
            var left = new MockedOperatorClass("left");
            var type = left.GetType();
            var result = EvaluateBinary(op, type, type, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableBinaryOperators))]
        public void Expression_Binary_Overloadable_Class_Null_Both(string op)
        {
            var type = typeof(MockedOperatorClass);
            var result = EvaluateBinary(op, type, type, null, null);
            Assert.Null(result);
        }

        // Shifting operators with custom structs and classes

        [Theory]
        [MemberData(nameof(GetOverloadableShiftingOperators))]
        public void Expression_Binary_Overloadable_Shifting_Struct(string op)
        {
            var left = new MockedOperatorStruct("left");
            var leftType = left.GetType();
            var right = 2;
            var rightType = right.GetType();
            var result = EvaluateBinary(op, leftType, rightType, left, right);
            Assert.Equal(new MockedOperatorStruct(left + " " + op + " " + right), result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableShiftingOperators))]
        public void Expression_Binary_Overloadable_Shifting_Struct_Null_Left(string op)
        {
            var leftType = typeof(MockedOperatorStruct);
            var right = 3;
            var rightType = right.GetType();
            var result = EvaluateBinary(op, leftType, rightType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableShiftingOperators))]
        public void Expression_Binary_Overloadable_Shifting_Struct_Null_Right(string op)
        {
            var left = new MockedOperatorStruct("left");
            var leftType = left.GetType();
            var rightType = typeof (int);
            var result = EvaluateBinary(op, leftType, rightType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableShiftingOperators))]
        public void Expression_Binary_Overloadable_Shifting_Struct_Null_Both(string op)
        {
            var leftType = typeof(MockedOperatorStruct);
            var rightType = typeof (int);
            var result = EvaluateBinary(op, leftType, rightType, null, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableShiftingOperators))]
        public void Expression_Binary_Overloadable_Shifting_Class(string op)
        {
            var left = new MockedOperatorClass("left");
            var leftType = left.GetType();
            var right = 4;
            var rightType = right.GetType();
            var result = EvaluateBinary(op, leftType, rightType, left, right);
            Assert.Equal(new MockedOperatorClass(left + " " + op + " " + right), result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableShiftingOperators))]
        public void Expression_Binary_Overloadable_Shifting_Class_Null_Left(string op)
        {
            var leftType = typeof(MockedOperatorClass);
            var right = 3;
            var rightType = right.GetType();
            var result = EvaluateBinary(op, leftType, rightType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableShiftingOperators))]
        public void Expression_Binary_Overloadable_Shifting_Class_Null_Right(string op)
        {
            var left = new MockedOperatorClass("left");
            var leftType = left.GetType();
            var rightType = typeof(int);
            var result = EvaluateBinary(op, leftType, rightType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableShiftingOperators))]
        public void Expression_Binary_Overloadable_Shifting_Class_Null_Both(string op)
        {
            var leftType = typeof(MockedOperatorClass);
            var rightType = typeof(int);
            var result = EvaluateBinary(op, leftType, rightType, null, null);
            Assert.Null(result);
        }

        // Built-in Multiply

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Multiply(Type argumentType)
        {
            var left = Convert.ChangeType(3, argumentType);
            var right = Convert.ChangeType(5, argumentType);
            var expectedResult = Convert.ChangeType(3 * 5, argumentType);

            Assert.IsType(argumentType, left);
            Assert.IsType(argumentType, right);
            Assert.IsType(argumentType, expectedResult);

            var result = EvaluateBinary("*", argumentType, argumentType, left, right);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Multiply_Null_Left(Type argumentType)
        {
            var right = Convert.ChangeType(5, argumentType);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary("*", argumentType, argumentType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Multiply_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(3, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary("*", argumentType, argumentType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Multiply_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary("*", argumentType, argumentType, null, null);
            Assert.Null(result);
        }

        // Built-in Divide

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Divide(Type argumentType)
        {
            var left = Convert.ChangeType(21, argumentType);
            var right = Convert.ChangeType(7, argumentType);
            var expectedResult = Convert.ChangeType(21 / 7, argumentType);

            Assert.IsType(argumentType, left);
            Assert.IsType(argumentType, right);
            Assert.IsType(argumentType, expectedResult);

            var result = EvaluateBinary("/", argumentType, argumentType, left, right);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Divide_Null_Left(Type argumentType)
        {
            var right = Convert.ChangeType(7, argumentType);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary("/", argumentType, argumentType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Divide_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(21, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary("/", argumentType, argumentType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Divide_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary("/", argumentType, argumentType, null, null);
            Assert.Null(result);
        }

        // Built-in Modulus

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Modulus(Type argumentType)
        {
            var left = Convert.ChangeType(25, argumentType);
            var right = Convert.ChangeType(8, argumentType);
            var expectedResult = Convert.ChangeType(25 % 8, argumentType);

            Assert.IsType(argumentType, left);
            Assert.IsType(argumentType, right);
            Assert.IsType(argumentType, expectedResult);

            var result = EvaluateBinary("%", argumentType, argumentType, left, right);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Modulus_Null_Left(Type argumentType)
        {
            var right = Convert.ChangeType(8, argumentType);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary("%", argumentType, argumentType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Modulus_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(25, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary("%", argumentType, argumentType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Modulus_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary("%", argumentType, argumentType, null, null);
            Assert.Null(result);
        }

        // Built-in Add

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Add(Type argumentType)
        {
            var left = Convert.ChangeType(25, argumentType);
            var right = Convert.ChangeType(8, argumentType);
            var expectedResult = Convert.ChangeType(25 + 8, argumentType);

            Assert.IsType(argumentType, left);
            Assert.IsType(argumentType, right);
            Assert.IsType(argumentType, expectedResult);

            var result = EvaluateBinary("+", argumentType, argumentType, left, right);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Add_Null_Left(Type argumentType)
        {
            var right = Convert.ChangeType(8, argumentType);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary("+", argumentType, argumentType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Add_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(25, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary("+", argumentType, argumentType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Add_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary("+", argumentType, argumentType, null, null);
            Assert.Null(result);
        }

        // Built-in Concat

        [Theory]
        [InlineData(typeof(string), typeof(string), "First", "Second")]
        [InlineData(typeof(string), typeof(object), "First", 2)]
        [InlineData(typeof(object), typeof(string), 1, "Second")]
        public void Expression_Binary_BuiltIn_Concat(Type leftType, Type rightType, object left, object right)
        {
            var result = EvaluateBinary("+", leftType, rightType, left, right);
            var expectedResult = string.Concat(left, right);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(typeof(string), typeof(string), "Second")]
        [InlineData(typeof(string), typeof(object), 2)]
        [InlineData(typeof(object), typeof(string), "Second")]
        public void Expression_Binary_BuiltIn_Concat_Null_Left(Type leftType, Type rightType, object right)
        {
            var result = EvaluateBinary("+", leftType, rightType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [InlineData(typeof(string), typeof(string), "First")]
        [InlineData(typeof(string), typeof(object), "First")]
        [InlineData(typeof(object), typeof(string), 1)]
        public void Expression_Binary_BuiltIn_Concat_Null_Right(Type leftType, Type rightType, object left)
        {
            var result = EvaluateBinary("+", leftType, rightType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [InlineData(typeof(string), typeof(string))]
        [InlineData(typeof(string), typeof(object))]
        [InlineData(typeof(object), typeof(string))]
        public void Expression_Binary_BuiltIn_Concat_Null_Both(Type leftType, Type rightType)
        {
            var result = EvaluateBinary("+", leftType, rightType, null, null);
            Assert.Null(result);
        }

        // Built-in Sub

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Subtract(Type argumentType)
        {
            var left = Convert.ChangeType(25, argumentType);
            var right = Convert.ChangeType(8, argumentType);
            var expectedResult = Convert.ChangeType(25 - 8, argumentType);

            Assert.IsType(argumentType, left);
            Assert.IsType(argumentType, right);
            Assert.IsType(argumentType, expectedResult);

            var result = EvaluateBinary("-", argumentType, argumentType, left, right);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Subtract_Null_Left(Type argumentType)
        {
            var right = Convert.ChangeType(8, argumentType);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary("-", argumentType, argumentType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Subtract_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(25, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary("-", argumentType, argumentType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Subtract_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary("-", argumentType, argumentType, null, null);
            Assert.Null(result);
        }

        // Built-in LeftShift

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_LeftShift(Type argumentType)
        {
            var left = Convert.ChangeType(3, argumentType);
            var right = 2;
            var expectedResult = Convert.ChangeType(3 << 2, argumentType);

            Assert.IsType(argumentType, left);
            Assert.IsType(argumentType, expectedResult);

            var result = EvaluateBinary("<<", argumentType, typeof(int), left, right);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_LeftShift_Null_Left(Type argumentType)
        {
            var right = 2;

            var result = EvaluateBinary("<<", argumentType, typeof(int), null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_LeftShift_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(3, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary("<<", argumentType, typeof(int), left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_LeftShift_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary("<<", argumentType, typeof(int), null, null);
            Assert.Null(result);
        }

        // Built-in RightShift

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_RightShift(Type argumentType)
        {
            var left = Convert.ChangeType(8, argumentType);
            var right = 2;
            var expectedResult = Convert.ChangeType(8 >> 2, argumentType);

            Assert.IsType(argumentType, left);
            Assert.IsType(argumentType, expectedResult);

            var result = EvaluateBinary(">>", argumentType, typeof(int), left, right);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_RightShift_Null_Left(Type argumentType)
        {
            var right = 2;

            var result = EvaluateBinary(">>", argumentType, typeof(int), null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_RightShift_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(8, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary(">>", argumentType, typeof(int), left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_RightShift_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary(">>", argumentType, typeof(int), null, null);
            Assert.Null(result);
        }

        // Built-in Equal

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_Equal_Numerical(Type argumentType)
        {
            var left = Convert.ChangeType(2, argumentType);
            var right = left;

            Assert.IsType(argumentType, left);

            var result = EvaluateBinary("=", argumentType, argumentType, left, right);
            Assert.Equal(true, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_Equal_Numerical_Null_Left(Type argumentType)
        {
            var right = Convert.ChangeType(2, argumentType);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary("=", argumentType, argumentType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_Equal_Numerical_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(2, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary("=", argumentType, argumentType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_Equal_Numerical_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary("=", argumentType, argumentType, null, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Equal_String()
        {
            var left = "Hello";
            var right = left;
            var type = typeof(string);
            var result = EvaluateBinary("=", type, type, left, right);
            Assert.Equal(true, result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Equal_String_Null_Left()
        {
            var right = "Hello";
            var type = typeof(string);
            var result = EvaluateBinary("=", type, type, null, right);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Equal_String_Null_Right()
        {
            var left = "Hello";
            var type = typeof(string);
            var result = EvaluateBinary("=", type, type, left, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Equal_String_Null_Both()
        {
            var type = typeof(string);
            var result = EvaluateBinary("=", type, type, null, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Equal_Object()
        {
            var left = new object();
            var right = left;
            var type = typeof(object);
            var result = EvaluateBinary("=", type, type, left, right);
            Assert.Equal(true, result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Equal_Object_Null_Left()
        {
            var right = new object();
            var type = typeof(object);
            var result = EvaluateBinary("=", type, type, null, right);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Equal_Object_Null_Right()
        {
            var left = new object();
            var type = typeof(object);
            var result = EvaluateBinary("=", type, type, left, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Equal_Object_Null_Both()
        {
            var type = typeof(object);
            var result = EvaluateBinary("=", type, type, null, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Equal_Boolean()
        {
            var left = true;
            var right = left;
            var type = typeof(bool);
            var result = EvaluateBinary("=", type, type, left, right);
            Assert.Equal(true, result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Equal_Boolean_Null_Left()
        {
            var right = true;
            var type = typeof(bool);
            var result = EvaluateBinary("=", type, type, null, right);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Equal_Boolean_Null_Right()
        {
            var left = true;
            var type = typeof(bool);
            var result = EvaluateBinary("=", type, type, left, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Equal_Boolean_Null_Both()
        {
            var type = typeof(bool);
            var result = EvaluateBinary("=", type, type, null, null);
            Assert.Null(result);
        }

        // Built-in NotEqual

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_NotEqual_Numerical(Type argumentType)
        {
            var left = Convert.ChangeType(2, argumentType);
            var right = left;

            Assert.IsType(argumentType, left);

            var result = EvaluateBinary("!=", argumentType, argumentType, left, right);
            Assert.NotEqual(true, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_NotEqual_Numerical_Null_Left(Type argumentType)
        {
            var right = Convert.ChangeType(2, argumentType);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary("!=", argumentType, argumentType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_NotEqual_Numerical_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(2, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary("!=", argumentType, argumentType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_NotEqual_Numerical_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary("!=", argumentType, argumentType, null, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_NotEqual_String()
        {
            var left = "Hello";
            var right = left;
            var type = typeof(string);
            var result = EvaluateBinary("!=", type, type, left, right);
            Assert.NotEqual(true, result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_NotEqual_String_Null_Left()
        {
            var right = "Hello";
            var type = typeof(string);
            var result = EvaluateBinary("!=", type, type, null, right);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_NotEqual_String_Null_Right()
        {
            var left = "Hello";
            var type = typeof(string);
            var result = EvaluateBinary("!=", type, type, left, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_NotEqual_String_Null_Both()
        {
            var type = typeof(string);
            var result = EvaluateBinary("!=", type, type, null, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_NotEqual_Object()
        {
            var left = new object();
            var right = left;
            var type = typeof(object);
            var result = EvaluateBinary("!=", type, type, left, right);
            Assert.NotEqual(true, result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_NotEqual_Object_Null_Left()
        {
            var right = new object();
            var type = typeof(object);
            var result = EvaluateBinary("!=", type, type, null, right);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_NotEqual_Object_Null_Right()
        {
            var left = new object();
            var type = typeof(object);
            var result = EvaluateBinary("!=", type, type, left, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_NotEqual_Object_Null_Both()
        {
            var type = typeof(object);
            var result = EvaluateBinary("!=", type, type, null, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_NotEqual_Boolean()
        {
            var left = true;
            var right = left;
            var type = typeof(bool);
            var result = EvaluateBinary("!=", type, type, left, right);
            Assert.NotEqual(true, result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_NotEqual_Boolean_Null_Left()
        {
            var right = true;
            var type = typeof(bool);
            var result = EvaluateBinary("!=", type, type, null, right);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_NotEqual_Boolean_Null_Right()
        {
            var left = true;
            var type = typeof(bool);
            var result = EvaluateBinary("!=", type, type, left, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_NotEqual_Boolean_Null_Both()
        {
            var type = typeof(bool);
            var result = EvaluateBinary("!=", type, type, null, null);
            Assert.Null(result);
        }

        // Built-in Less

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Less(Type argumentType)
        {
            var left = Convert.ChangeType(8, argumentType);
            var right = Convert.ChangeType(25, argumentType);

            Assert.IsType(argumentType, left);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary("<", argumentType, argumentType, left, right);
            Assert.Equal(true, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Less_Null_Left(Type argumentType)
        {
            var right = Convert.ChangeType(25, argumentType);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary("<", argumentType, argumentType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Less_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(8, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary("<", argumentType, argumentType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Less_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary("<", argumentType, argumentType, null, null);
            Assert.Null(result);
        }

        // Built-in Greater

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Greater(Type argumentType)
        {
            var left = Convert.ChangeType(25, argumentType);
            var right = Convert.ChangeType(8, argumentType);

            Assert.IsType(argumentType, left);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary(">", argumentType, argumentType, left, right);
            Assert.Equal(true, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Greater_Null_Left(Type argumentType)
        {
            var right = Convert.ChangeType(8, argumentType);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary(">", argumentType, argumentType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Greater_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(25, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary(">", argumentType, argumentType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_Greater_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary(">", argumentType, argumentType, null, null);
            Assert.Null(result);
        }

        // Built-in LessOrEqual

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_LessOrEqual(Type argumentType)
        {
            var left = Convert.ChangeType(8, argumentType);
            var right = Convert.ChangeType(8, argumentType);

            Assert.IsType(argumentType, left);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary("<=", argumentType, argumentType, left, right);
            Assert.Equal(true, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_LessOrEqual_Null_Left(Type argumentType)
        {
            var right = Convert.ChangeType(8, argumentType);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary("<=", argumentType, argumentType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_LessOrEqual_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(8, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary("<=", argumentType, argumentType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_LessOrEqual_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary("<=", argumentType, argumentType, null, null);
            Assert.Null(result);
        }

        // Built-in GreaterOrEqual

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_GreaterOrEqual(Type argumentType)
        {
            var left = Convert.ChangeType(8, argumentType);
            var right = Convert.ChangeType(8, argumentType);

            Assert.IsType(argumentType, left);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary(">=", argumentType, argumentType, left, right);
            Assert.Equal(true, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_GreaterOrEqual_Null_Left(Type argumentType)
        {
            var right = Convert.ChangeType(8, argumentType);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary(">=", argumentType, argumentType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_GreaterOrEqual_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(8, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary(">=", argumentType, argumentType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Binary_BuiltIn_GreaterOrEqual_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary(">=", argumentType, argumentType, null, null);
            Assert.Null(result);
        }

        // Built-in BitAnd

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_BitAnd(Type argumentType)
        {
            var left = Convert.ChangeType(10, argumentType);
            var right = Convert.ChangeType(2, argumentType);
            var expectedResult = Convert.ChangeType(10 & 2, argumentType);

            Assert.IsType(argumentType, left);
            Assert.IsType(argumentType, right);
            Assert.IsType(argumentType, expectedResult);

            var result = EvaluateBinary("&", argumentType, argumentType, left, right);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_BitAnd_Null_Left(Type argumentType)
        {
            var right = Convert.ChangeType(2, argumentType);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary("&", argumentType, argumentType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_BitAnd_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(10, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary("&", argumentType, argumentType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_BitAnd_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary("&", argumentType, argumentType, null, null);
            Assert.Null(result);
        }

        // Built-in BitOr

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_BitOr(Type argumentType)
        {
            var left = Convert.ChangeType(10, argumentType);
            var right = Convert.ChangeType(1, argumentType);
            var expectedResult = Convert.ChangeType(10 | 1, argumentType);

            Assert.IsType(argumentType, left);
            Assert.IsType(argumentType, right);
            Assert.IsType(argumentType, expectedResult);

            var result = EvaluateBinary("|", argumentType, argumentType, left, right);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_BitOr_Null_Left(Type argumentType)
        {
            var right = Convert.ChangeType(1, argumentType);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary("|", argumentType, argumentType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_BitOr_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(10, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary("|", argumentType, argumentType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_BitOr_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary("|", argumentType, argumentType, null, null);
            Assert.Null(result);
        }

        // Built-in BitXor

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_BitXor(Type argumentType)
        {
            var left = Convert.ChangeType(3, argumentType);
            var right = Convert.ChangeType(7, argumentType);
            var expectedResult = Convert.ChangeType(3 ^ 7, argumentType);

            Assert.IsType(argumentType, left);
            Assert.IsType(argumentType, right);
            Assert.IsType(argumentType, expectedResult);

            var result = EvaluateBinary("^", argumentType, argumentType, left, right);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_BitXor_Null_Left(Type argumentType)
        {
            var right = Convert.ChangeType(7, argumentType);
            Assert.IsType(argumentType, right);

            var result = EvaluateBinary("^", argumentType, argumentType, null, right);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_BitXor_Null_Right(Type argumentType)
        {
            var left = Convert.ChangeType(3, argumentType);
            Assert.IsType(argumentType, left);

            var result = EvaluateBinary("^", argumentType, argumentType, left, null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Binary_BuiltIn_BitXor_Null_Both(Type argumentType)
        {
            var result = EvaluateBinary("^", argumentType, argumentType, null, null);
            Assert.Null(result);
        }

        // Built-in Like

        [Fact]
        public void Expression_Binary_BuiltIn_Like()
        {
            var left = "Hello";
            var right = "He%";
            var type = typeof(string);
            var result = EvaluateBinary("LIKE", type, type, left, right);
            Assert.Equal(true, result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Like_Null_Left()
        {
            var right = "He%";
            var type = typeof(string);
            var result = EvaluateBinary("LIKE", type, type, null, right);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Like_Null_Right()
        {
            var left = "Hello";
            var type = typeof(string);
            var result = EvaluateBinary("LIKE", type, type, left, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Like_Null_Both()
        {
            var type = typeof(string);
            var result = EvaluateBinary("LIKE", type, type, null, null);
            Assert.Null(result);
        }

        // Built-in Power

        [Fact]
        public void Expression_Binary_BuiltIn_Power()
        {
            var left = 3.0;
            var right = 2.0;
            var type = typeof(double);
            var result = EvaluateBinary("**", type, type, left, right);
            Assert.Equal(Math.Pow(left, right), result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Power_Null_Left()
        {
            var right = 2.0;
            var type = typeof(double);
            var result = EvaluateBinary("**", type, type, null, right);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Power_Null_Right()
        {
            var left = 3.0;
            var type = typeof(double);
            var result = EvaluateBinary("**", type, type, left, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_Power_Null_Both()
        {
            var type = typeof(double);
            var result = EvaluateBinary("**", type, type, null, null);
            Assert.Null(result);
        }

        // Built-in SimilarTo

        [Fact]
        public void Expression_Binary_BuiltIn_SimilarTo()
        {
            var left = "serialize";
            var right = "seriali[sz]e";
            var type = typeof(string);
            var result = EvaluateBinary("SIMILAR TO", type, type, left, right);
            Assert.Equal(true, result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_SimilarTo_Null_Left()
        {
            var right = "seriali[sz]e";
            var type = typeof(string);
            var result = EvaluateBinary("SIMILAR TO", type, type, null, right);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_SimilarTo_Null_Right()
        {
            var left = "serialize";
            var type = typeof(string);
            var result = EvaluateBinary("SIMILAR TO", type, type, left, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_SimilarTo_Null_Both()
        {
            var type = typeof(string);
            var result = EvaluateBinary("SIMILAR TO", type, type, null, null);
            Assert.Null(result);
        }

        // Built-in SoundsLike

        [Fact]
        public void Expression_Binary_BuiltIn_SoundsLike()
        {
            var left = "Robert";
            var right = "Rupert";
            var type = typeof(string);
            var result = EvaluateBinary("SOUNDS LIKE", type, type, left, right);
            Assert.Equal(true, result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_SoundsLike_Null_Left()
        {
            var right = "Rupert";
            var type = typeof(string);
            var result = EvaluateBinary("SOUNDS LIKE", type, type, null, right);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_SoundsLike_Null_Right()
        {
            var left = "Robert";
            var type = typeof(string);
            var result = EvaluateBinary("SOUNDS LIKE", type, type, left, null);
            Assert.Null(result);
        }

        [Fact]
        public void Expression_Binary_BuiltIn_SoundsLike_Null_Both()
        {
            var type = typeof(string);
            var result = EvaluateBinary("SOUNDS LIKE", type, type, null, null);
            Assert.Null(result);
        }

        // Built-in LogicalAnd

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, true)]
        [InlineData(null, null, null)]
        [InlineData(true, null, null)]
        [InlineData(null, true, null)]
        [InlineData(false, null, false)]
        [InlineData(null, false, false)]
        public void Expression_Binary_BuiltIn_LogicalAnd(object left, object right, object expectedResult)
        {
            var type = typeof(bool);
            var result = EvaluateBinary("AND", type, type, left, right);
            Assert.Equal(expectedResult, result);
        }

        // Built-in LogicalOr

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        [InlineData(null, null, null)]
        [InlineData(true, null, true)]
        [InlineData(null, true, true)]
        [InlineData(false, null, null)]
        [InlineData(null, false, null)]
        public void Expression_Binary_BuiltIn_LogicalOr(object left, object right, object expectedResult)
        {
            var type = typeof(bool);
            var result = EvaluateBinary("OR", type, type, left, right);
            Assert.Equal(expectedResult, result);
        }
    }
}