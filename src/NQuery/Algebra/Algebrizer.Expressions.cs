using System;
using System.Collections.Generic;

using NQuery.BoundNodes;

namespace NQuery.Algebra
{
    partial class Algebrizer
    {
        private AlgebrizedExpression AlgebrizeExpression(AlgebraNode input, BoundExpression node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.NameExpression:
                    return AlgebrizeNameExpression(input, (BoundNameExpression)node);
                case BoundNodeKind.UnaryExpression:
                    return AlgebrizeUnaryExpression(input, (BoundUnaryExpression)node);
                case BoundNodeKind.BinaryExpression:
                    return AlgebrizeBinaryExpression(input, (BoundBinaryExpression)node);
                case BoundNodeKind.LiteralExpression:
                    return AlgebrizeLiteralExpression(input, (BoundLiteralExpression)node);
                case BoundNodeKind.VariableExpression:
                    return AlgebrizeVariableExpression(input, (BoundVariableExpression)node);
                case BoundNodeKind.FunctionInvocationExpression:
                    return AlgebrizeFunctionInvocationExpression(input, (BoundFunctionInvocationExpression)node);
                case BoundNodeKind.AggregateExpression:
                    return AlgebrizeBoundAggregateExpression(input, (BoundAggregateExpression)node);
                case BoundNodeKind.PropertyAccessExpression:
                    return AlgebrizePropertyAccessExpression(input, (BoundPropertyAccessExpression)node);
                case BoundNodeKind.MethodInvocationExpression:
                    return AlgebrizeMethodInvocationExpression(input, (BoundMethodInvocationExpression)node);
                case BoundNodeKind.CastExpression:
                    return AlgebrizeCastExpression(input, (BoundCastExpression)node);
                case BoundNodeKind.IsNullExpression:
                    return AlgebrizeIsNullExpression(input, (BoundIsNullExpression)node);
                case BoundNodeKind.CaseExpression:
                    return AlgebrizeCaseExpression(input, (BoundCaseExpression)node);
                case BoundNodeKind.SingleRowSubselect:
                    return AlgebrizeSingleRowSubselect(input, (BoundSingleRowSubselect)node);
                case BoundNodeKind.ExistsSubselect:
                    return AlgebrizeExistsSubselect(input, (BoundExistsSubselect)node);
                case BoundNodeKind.AllAnySubselect:
                    return AlgebrizeAllAnySubselect(input, (BoundAllAnySubselect)node);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private AlgebrizedExpressionList AlgebrizeExpressionList(AlgebraNode input, IList<BoundExpression> nodes)
        {
            var resultInput = input;
            var resultArguments = new BoundExpression[nodes.Count];

            for (var i = 0; i < nodes.Count; i++)
            {
                var algebrizedExpression = AlgebrizeExpression(resultInput, nodes[i]);
                resultInput = algebrizedExpression.Input;
                resultArguments[i] = algebrizedExpression.Expression;
            }

            return new AlgebrizedExpressionList(resultInput, resultArguments);
        }

        private AlgebrizedExpression AlgebrizeNameExpression(AlgebraNode input, BoundNameExpression node)
        {
            // TODO: Do we need this?
            return new AlgebrizedExpression(input, node);
        }

        private AlgebrizedExpression AlgebrizeUnaryExpression(AlgebraNode input, BoundUnaryExpression node)
        {
            var algebrizedExpression = AlgebrizeExpression(input, node.Expression);

            var resultInput = algebrizedExpression.Input;
            var resultNode = node.Update(algebrizedExpression.Expression);

            return new AlgebrizedExpression(resultInput, resultNode);
        }

        private AlgebrizedExpression AlgebrizeBinaryExpression(AlgebraNode input, BoundBinaryExpression node)
        {
            var algebrizedLeft = AlgebrizeExpression(input, node.Left);
            var algebrizedRight = AlgebrizeExpression(algebrizedLeft.Input, node.Right);

            var resultInput = algebrizedRight.Input;
            var resultNode = node.Update(algebrizedLeft.Expression, algebrizedRight.Expression);

            return new AlgebrizedExpression(resultInput, resultNode);
        }

        private AlgebrizedExpression AlgebrizeLiteralExpression(AlgebraNode input, BoundLiteralExpression node)
        {
            // TODO: Do we need this?
            return new AlgebrizedExpression(input, node);
        }

        private AlgebrizedExpression AlgebrizeVariableExpression(AlgebraNode input, BoundVariableExpression node)
        {
            // TODO: Do we need this?
            return new AlgebrizedExpression(input, node);
        }

        private AlgebrizedExpression AlgebrizeFunctionInvocationExpression(AlgebraNode input, BoundFunctionInvocationExpression node)
        {
            var algebrizedArguments = AlgebrizeExpressionList(input, node.Arguments);

            var resultInput = algebrizedArguments.Input;
            var resultArguments = algebrizedArguments.Expressions;
            var resultNode = node.Update(resultArguments);

            return new AlgebrizedExpression(resultInput, resultNode);
        }

        private AlgebrizedExpression AlgebrizeBoundAggregateExpression(AlgebraNode input, BoundAggregateExpression node)
        {
            // TODO: Algebrize aggregation. We may need to do this from the BoundSelectQuery.
            return new AlgebrizedExpression(input, node);
        }

        private AlgebrizedExpression AlgebrizePropertyAccessExpression(AlgebraNode input, BoundPropertyAccessExpression node)
        {
            var algebrizedTarget = AlgebrizeExpression(input, node.Target);

            var resultInput = algebrizedTarget.Input;
            var resultNode = node.Update(algebrizedTarget.Expression);

            return new AlgebrizedExpression(resultInput, resultNode);
        }

        private AlgebrizedExpression AlgebrizeMethodInvocationExpression(AlgebraNode input, BoundMethodInvocationExpression node)
        {
            var algebrizedTarget = AlgebrizeExpression(input, node.Target);
            var algebrizedArguments = AlgebrizeExpressionList(algebrizedTarget.Input, node.Arguments);

            var resultTarget = algebrizedTarget.Expression;
            var resultArguments = algebrizedArguments.Expressions;

            var resultInput = algebrizedArguments.Input;
            var resultNode = node.Update(resultTarget, resultArguments);

            return new AlgebrizedExpression(resultInput, resultNode);
        }

        private AlgebrizedExpression AlgebrizeCastExpression(AlgebraNode input, BoundCastExpression node)
        {
            var algebrizedExpression = AlgebrizeExpression(input, node.Expression);

            var resultInput = algebrizedExpression.Input;
            var resultNode = node.Update(algebrizedExpression.Expression);

            return new AlgebrizedExpression(resultInput, resultNode);
        }

        private AlgebrizedExpression AlgebrizeIsNullExpression(AlgebraNode input, BoundIsNullExpression node)
        {
            var algebrizedExpression = AlgebrizeExpression(input, node.Expression);

            var resultInput = algebrizedExpression.Input;
            var resultNode = node.Update(algebrizedExpression.Expression);

            return new AlgebrizedExpression(resultInput, resultNode);
        }

        private AlgebrizedExpression AlgebrizeCaseExpression(AlgebraNode input, BoundCaseExpression node)
        {
            // TODO: Algebrize CASE
            return new AlgebrizedExpression(input, node);
        }

        private AlgebrizedExpression AlgebrizeSingleRowSubselect(AlgebraNode input, BoundSingleRowSubselect node)
        {
            var algebrizedQuery = AlgebrizeQuery(node.BoundQuery);

            // TODO: We need to output an ASSERT operator that guarantees that input returns at most one row.
            // TODO: We need to produce a value slot that represents the result of the query.

            var valueSlot = CreateValueSlot(node.BoundQuery.SelectColumns[0].Expression.Type);
            var resultInput = new AlgebraJoinNode(AlgebraJoinKind.LeftOuter, input, algebrizedQuery, null, null);
            var resultNode = new BoundValueSlotExpression(valueSlot);

            return new AlgebrizedExpression(resultInput, resultNode);
        }

        private AlgebrizedExpression AlgebrizeExistsSubselect(AlgebraNode input, BoundExistsSubselect node)
        {
            var algebrizedQuery = AlgebrizeQuery(node.BoundQuery);

            // TODO: Mark the join as a probing LEFT SEMI JOIN.
            var valueSlot = CreateValueSlot(typeof(bool));
            var resultInput = new AlgebraJoinNode(AlgebraJoinKind.LeftSemiJoin, input, algebrizedQuery, valueSlot, null);
            var resultNode = new BoundValueSlotExpression(valueSlot);

            return new AlgebrizedExpression(resultInput, resultNode);
        }

        private AlgebrizedExpression AlgebrizeAllAnySubselect(AlgebraNode input, BoundAllAnySubselect node)
        {
            // TODO: We may want to re-write part of this during binding.
            //
            // left OP ALL (SELECT right FROM ...)
            // ==>
            // NOT EXISTS (SELECT right FROM ... WHERE NOT(left OP right))
            //
            // left OP ANY/SOME (SELECT right FROM ...)
            // ==>
            // EXISTS (SELECT right FROM ... WHERE left OP right)

            return new AlgebrizedExpression(input, node);
        }

        private struct AlgebrizedExpression
        {
            private readonly AlgebraNode _input;
            private readonly BoundExpression _expression;

            public AlgebrizedExpression(AlgebraNode input, BoundExpression expression)
            {
                _input = input;
                _expression = expression;
            }

            public AlgebraNode Input
            {
                get { return _input; }
            }

            public BoundExpression Expression
            {
                get { return _expression; }
            }
        }

        private struct AlgebrizedExpressionList
        {
            private readonly AlgebraNode _input;
            private readonly BoundExpression[] _expressions;

            public AlgebrizedExpressionList(AlgebraNode input, BoundExpression[] expressions)
            {
                _input = input;
                _expressions = expressions;
            }

            public AlgebraNode Input
            {
                get { return _input; }
            }

            public BoundExpression[] Expressions
            {
                get { return _expressions; }
            }
        }
    }
}