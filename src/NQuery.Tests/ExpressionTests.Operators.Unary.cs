using System;
using System.Collections.Generic;

using NQuery.Symbols;

using Xunit;

namespace NQuery.Tests
{
    public partial class ExpressionTests
    {
        private static object EvaluateUnary(string op, Type argumentType, object argument)
        {
            var variable = new VariableSymbol("arg", argumentType, argument);
            var dataContext = DataContext.Default.AddVariables(variable);
            var expression = Expression<object>.Create(dataContext, op + " " + variable.Name);
            return expression.Evaluate();
        }

        public static IEnumerable<object[]> GetOverloadableUnaryOperators()
        {
            return new[]
            {
                new object[] {"+"},
                new object[] {"-"},
                new object[] {"~"},
                new object[] {"NOT"}
            };
        }

        // All overloadable operators with custom structs and classses

        [Theory]
        [MemberData(nameof(GetOverloadableUnaryOperators))]
        public void Expression_Unary_Overloadable_Struct(string op)
        {
            var value = new MockedOperatorStruct("value");
            var result = EvaluateUnary(op, value.GetType(), value);
            Assert.Equal(new MockedOperatorStruct(op + " value"), result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableUnaryOperators))]
        public void Expression_Unary_Overloadable_Struct_Null(string op)
        {
            var result = EvaluateUnary(op, typeof(MockedOperatorStruct), null);
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableUnaryOperators))]
        public void Expression_Unary_Overloadable_Class(string op)
        {
            var value = new MockedOperatorClass("value");
            var result = EvaluateUnary(op, value.GetType(), value);
            Assert.Equal(new MockedOperatorClass(op + " value"), result);
        }

        [Theory]
        [MemberData(nameof(GetOverloadableUnaryOperators))]
        public void Expression_Unary_Overloadable_Class_Null(string op)
        {
            var result = EvaluateUnary(op, typeof(MockedOperatorClass), null);
            Assert.Null(result);
        }

        // Built-in Identity

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Unary_BuiltIn_Identity(Type type)
        {
            var value = Convert.ChangeType(5, type);
            Assert.IsType(type, value);

            var result = EvaluateUnary("+", type, value);
            Assert.Equal(value, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInNumericTypes))]
        public void Expression_Unary_BuiltIn_Identity_Null(Type type)
        {
            var result = EvaluateUnary("+", type, null);
            Assert.Null(result);
        }

        // Built-in Negation

        [Theory]
        [MemberData(nameof(GetBuiltInSignedNumericTypes))]
        public void Expression_Unary_BuiltIn_Negation(Type type)
        {
            const int value = 5;
            var typedValue = Convert.ChangeType(value, type);
            var expectedResult = Convert.ChangeType(-value, type);
            Assert.IsType(type, expectedResult);

            var result = EvaluateUnary("-", type, typedValue);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInSignedNumericTypes))]
        public void Expression_Unary_BuiltIn_Negation_Null(Type type)
        {
            var result = EvaluateUnary("-", type, null);
            Assert.Null(result);
        }

        // Built-in Complement

        [Theory]
        [InlineData(typeof(int), 5, ~5)]
        [InlineData(typeof(uint), (uint)5, ~(uint)5)]
        [InlineData(typeof(long), (long)5, ~(long)5)]
        [InlineData(typeof(ulong), (ulong)5, ~(ulong)5)]
        public void Expression_Unary_BuiltIn_Complement(Type type, object value, object expectedResult)
        {
            Assert.IsType(type, value);
            Assert.IsType(type, expectedResult);

            var result = EvaluateUnary("~", type, value);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetBuiltInIntegralTypes))]
        public void Expression_Unary_BuiltIn_Complement_Null(Type type)
        {
            var result = EvaluateUnary("~", type, null);
            Assert.Null(result);
        }

        // Built-in LogicalNot

        [Fact]
        public void Expression_Unary_BuiltIn_LogicalNot()
        {
            var result = EvaluateUnary("NOT", typeof(bool), false);
            Assert.Equal(true, result);
        }

        [Fact]
        public void Expression_Unary_BuiltIn_LogicalNot_Null()
        {
            var result = EvaluateUnary("NOT", typeof(bool), null);
            Assert.Null(result);
        }
    }
}