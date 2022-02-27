namespace NQuery.Symbols.Aggregation
{
    public sealed class AvgAggregateDefinition : AggregateDefinition
    {
        public override string Name
        {
            get { return @"AVG"; }
        }

        public override IAggregatable CreateAggregatable(Type argumentType)
        {
            var sumAggregate = new SumAggregateDefinition();
            var sumAggregatable = sumAggregate.CreateAggregatable(argumentType);
            if (sumAggregatable is null)
                return null;

            var countAggregate = new CountAggregateDefinition();
            var countAggregatable = countAggregate.CreateAggregatable(argumentType);
            if (countAggregatable is null)
                return null;

            var sumVariable = new VariableSymbol(@"Sum", sumAggregatable.ReturnType);
            var countVariable = new VariableSymbol(@"Count", countAggregatable.ReturnType);
            var divisionDataContext = DataContext.Empty.AddVariables(sumVariable, countVariable);
            var divisionExpression = Expression<object>.Create(divisionDataContext, @"@Sum / @Count");

            try
            {
                divisionExpression.Resolve();
            }
            catch (CompilationException)
            {
                return null;
            }

            return new AvgAggregatable(sumAggregatable, countAggregatable, divisionExpression, sumVariable, countVariable);
        }

        private sealed class AvgAggregatable : IAggregatable
        {
            private readonly IAggregatable _sumAggregatable;
            private readonly IAggregatable _countAggregatable;
            private readonly Expression<object> _divisionExpression;
            private readonly VariableSymbol _sumArgument;
            private readonly VariableSymbol _countArgument;

            public AvgAggregatable(IAggregatable sumAggregatable, IAggregatable countAggregatable, Expression<object> divisionExpression, VariableSymbol sumArgument, VariableSymbol countArgument)
            {
                _sumAggregatable = sumAggregatable;
                _countAggregatable = countAggregatable;
                _divisionExpression = divisionExpression;
                _sumArgument = sumArgument;
                _countArgument = countArgument;
            }

            public IAggregator CreateAggregator()
            {
                var sumAggregator = _sumAggregatable.CreateAggregator();
                var countAggregator = _countAggregatable.CreateAggregator();
                return new AvgAggregator(sumAggregator, countAggregator, _divisionExpression, _sumArgument, _countArgument);
            }

            public Type ReturnType
            {
                get { return _divisionExpression.Resolve(); }
            }
        }

        private sealed class AvgAggregator : IAggregator
        {
            private readonly IAggregator _sumAggregator;
            private readonly IAggregator _countAggregator;
            private readonly Expression<object> _divisionExpression;
            private readonly VariableSymbol _sumArgument;
            private readonly VariableSymbol _countArgument;

            public AvgAggregator(IAggregator sumAggregator, IAggregator countAggregator, Expression<object> divisionExpression, VariableSymbol sumArgument, VariableSymbol countArgument)
            {
                _sumAggregator = sumAggregator;
                _countAggregator = countAggregator;
                _divisionExpression = divisionExpression;
                _sumArgument = sumArgument;
                _countArgument = countArgument;
            }

            public void Initialize()
            {
                _sumAggregator.Initialize();
                _countAggregator.Initialize();
            }

            public void Accumulate(object value)
            {
                _sumAggregator.Accumulate(value);
                _countAggregator.Accumulate(value);
            }

            public object GetResult()
            {
                var sum = _sumAggregator.GetResult();
                if (sum is null)
                    return null;

                _sumArgument.Value = Convert.ChangeType(sum, _sumArgument.Type);
                _countArgument.Value = Convert.ChangeType(_countAggregator.GetResult(), _countArgument.Type);
                return _divisionExpression.Evaluate();
            }
        }
    }
}