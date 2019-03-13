#nullable enable

using System;

namespace NQuery.Symbols.Aggregation
{
    public sealed class SumAggregateDefinition : AggregateDefinition
    {
        public override string Name
        {
            get { return @"SUM"; }
        }

        public override IAggregatable? CreateAggregatable(Type argumentType)
        {
            // Create an expression to determine the type of inputType + inputType

            var addDataContext = DataContext.Empty.AddVariables(new VariableSymbol(@"Left", argumentType),
                                                                new VariableSymbol(@"Right", argumentType));

            var addExpression = Expression<object>.Create(addDataContext, @"Left + Right");

            Type sumType;
            try
            {
                sumType = addExpression.Resolve();
            }
            catch (CompilationException)
            {
                return null;
            }

            // In some cases, the result of the adding both arguments might be different from
            // arguments type, e.g. adding to bytes will result in an int.
            //
            // In order to get the correct answer, let's compose a new expression.

            var leftVariable = new VariableSymbol(@"Left", argumentType);
            var rightVariable = new VariableSymbol(@"Right", argumentType);
            var sumDataContext = DataContext.Empty.AddVariables(leftVariable, rightVariable);
            var sumExpression = Expression<object>.Create(sumDataContext, @"Left + Right");

            try
            {
                var resultType = sumExpression.Resolve();
                if (resultType != sumType)
                    return null;
            }
            catch (CompilationException)
            {
                return null;
            }

            // Conversion from inputType to sumType

            var conversionInputVariable = new VariableSymbol(@"Input", argumentType);
            var convertionDataContext = DataContext.Empty.AddVariables(conversionInputVariable);
            var conversionExpression = Expression<object>.Create(convertionDataContext, $"CAST(@Input AS {SyntaxFacts.GetValidIdentifier(sumType.Name)})");

            try
            {
                conversionExpression.Resolve();
            }
            catch (CompilationException)
            {
                return null;
            }

            return new SumAggregatable(sumExpression, leftVariable, rightVariable, conversionExpression, conversionInputVariable);
        }

        private sealed class SumAggregatable : IAggregatable
        {
            private readonly Expression<object> _sumExpression;
            private readonly VariableSymbol _leftParameter;
            private readonly VariableSymbol _rightParameter;
            private readonly Expression<object> _convertInputToSumExpression;
            private readonly VariableSymbol _conversionInputVariable;

            public SumAggregatable(Expression<object> sumExpression, VariableSymbol leftParameter, VariableSymbol rightParameter, Expression<object> convertInputToSumExpression, VariableSymbol conversionInputVariable)
            {
                _sumExpression = sumExpression;
                _leftParameter = leftParameter;
                _rightParameter = rightParameter;
                _convertInputToSumExpression = convertInputToSumExpression;
                _conversionInputVariable = conversionInputVariable;
            }

            public IAggregator CreateAggregator()
            {
                return new SumAggregator(_sumExpression, _leftParameter, _rightParameter, _convertInputToSumExpression, _conversionInputVariable);
            }

            public Type ReturnType
            {
                get { return _sumExpression.Resolve(); }
            }
        }

        private sealed class SumAggregator : IAggregator
        {
            private readonly Expression<object> _addExpression;
            private readonly VariableSymbol _leftParameter;
            private readonly VariableSymbol _rightParameter;
            private readonly Expression<object> _convertInputToSumExpression;
            private readonly VariableSymbol _conversionInputVariable;

            private object? _sum;

            public SumAggregator(Expression<object> addExpression, VariableSymbol leftParameter, VariableSymbol rightParameter, Expression<object> convertInputToSumExpression, VariableSymbol conversionInputVariable)
            {
                _addExpression = addExpression;
                _leftParameter = leftParameter;
                _rightParameter = rightParameter;
                _convertInputToSumExpression = convertInputToSumExpression;
                _conversionInputVariable = conversionInputVariable;
            }

            public void Initialize()
            {
                _sum = null;
            }

            public void Accumulate(object? value)
            {
                if (value != null)
                {
                    if (_sum == null)
                    {
                        _conversionInputVariable.Value = value;
                        _sum = _convertInputToSumExpression.Evaluate();
                    }
                    else
                    {
                        _leftParameter.Value = _sum;
                        _rightParameter.Value = value;
                        _sum = _addExpression.Evaluate();
                    }
                }
            }

            public object? GetResult()
            {
                return _sum;
            }
        }
    }
}