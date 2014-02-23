using System;
using System.Linq;

using NQuery.Binding;

namespace NQuery.Algebra
{
    partial class Algebrizer
    {
        private AlgebraExpression AlgebrizeExpression(BoundExpression node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.ColumnExpression:
                    return AlgebrizeColumnExpression((BoundColumnExpression)node);
                case BoundNodeKind.ValueSlotExpression:
                    return AlgebrizeValueSlotExpression((BoundValueSlotExpression)node);
                case BoundNodeKind.UnaryExpression:
                    return AlgebrizeUnaryExpression((BoundUnaryExpression)node);
                case BoundNodeKind.BinaryExpression:
                    return AlgebrizeBinaryExpression((BoundBinaryExpression)node);
                case BoundNodeKind.LiteralExpression:
                    return AlgebrizeLiteralExpression((BoundLiteralExpression)node);
                case BoundNodeKind.VariableExpression:
                    return AlgebrizeVariableExpression((BoundVariableExpression)node);
                case BoundNodeKind.FunctionInvocationExpression:
                    return AlgebrizeFunctionInvocationExpression((BoundFunctionInvocationExpression)node);
                case BoundNodeKind.PropertyAccessExpression:
                    return AlgebrizePropertyAccessExpression((BoundPropertyAccessExpression)node);
                case BoundNodeKind.MethodInvocationExpression:
                    return AlgebrizeMethodInvocationExpression((BoundMethodInvocationExpression)node);
                case BoundNodeKind.ConversionExpression:
                    return AlgebrizeConversionExpression((BoundConversionExpression)node);
                case BoundNodeKind.IsNullExpression:
                    return AlgebrizeIsNullExpression((BoundIsNullExpression)node);
                case BoundNodeKind.CaseExpression:
                    return AlgebrizeCaseExpression((BoundCaseExpression)node);
                case BoundNodeKind.SingleRowSubselect:
                    return AlgebrizeSingleRowSubselect((BoundSingleRowSubselect)node);
                case BoundNodeKind.ExistsSubselect:
                    return AlgebrizeExistsSubselect((BoundExistsSubselect)node);
                case BoundNodeKind.AllAnySubselect:
                    return AlgebrizeAllAnySubselect((BoundAllAnySubselect)node);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private AlgebraExpression AlgebrizeColumnExpression(BoundColumnExpression node)
        {
            var symbol = node.Symbol;
            return new AlgebraValueSlotExpression(symbol.ValueSlot);
        }

        private AlgebraExpression AlgebrizeValueSlotExpression(BoundValueSlotExpression node)
        {
            return new AlgebraValueSlotExpression(node.ValueSlot);
        }

        private AlgebraExpression AlgebrizeUnaryExpression(BoundUnaryExpression node)
        {
            var expression = AlgebrizeExpression(node.Expression);
            var signature = node.Result.Selected.Signature;
            return new AlgebraUnaryExpression(expression, signature);
        }

        private AlgebraExpression AlgebrizeBinaryExpression(BoundBinaryExpression node)
        {
            var left = AlgebrizeExpression(node.Left);
            var right = AlgebrizeExpression(node.Right);
            var signature = node.Result.Selected.Signature;
            return new AlgebraBinaryExpression(left, right, signature);
        }

        private AlgebraExpression AlgebrizeLiteralExpression(BoundLiteralExpression node)
        {
            return new AlgebraLiteralExpression(node.Value);
        }

        private AlgebraExpression AlgebrizeVariableExpression(BoundVariableExpression node)
        {
            return new AlgebraVariableExpression(node.Symbol);
        }

        private AlgebraExpression AlgebrizeFunctionInvocationExpression(BoundFunctionInvocationExpression node)
        {
            var arguments = node.Arguments.Select(AlgebrizeExpression).ToArray();
            var symbol = node.Symbol;
            return new AlgebraFunctionInvocationExpression(arguments, symbol);
        }

        private AlgebraExpression AlgebrizePropertyAccessExpression(BoundPropertyAccessExpression node)
        {
            var target = AlgebrizeExpression(node.Target);
            var symbol = node.PropertySymbol;
            return new AlgebraPropertyAccessExpression(target, symbol);
        }

        private AlgebraExpression AlgebrizeMethodInvocationExpression(BoundMethodInvocationExpression node)
        {
            var target = AlgebrizeExpression(node.Target);
            var arguments = node.Arguments.Select(AlgebrizeExpression).ToArray();
            var symbol = node.Symbol;
            return new AlgebraMethodInvocationExpression(target, arguments, symbol);
        }

        private AlgebraExpression AlgebrizeConversionExpression(BoundConversionExpression node)
        {
            var expression = AlgebrizeExpression(node.Expression);
            var conversion = node.Conversion;
            var type = node.Type;
            return new AlgebraConversionExpression(expression, conversion, type);
        }

        private AlgebraExpression AlgebrizeIsNullExpression(BoundIsNullExpression node)
        {
            var expression = AlgebrizeExpression(node.Expression);
            return new AlgebraIsNullExpression(expression);
        }

        private AlgebraExpression AlgebrizeCaseExpression(BoundCaseExpression node)
        {
            var caseLabels = (from c in node.CaseLabels
                              let condition = AlgebrizeExpression(c.Condition)
                              let result = AlgebrizeExpression(c.ThenExpression)
                              select new AlgebraCaseLabel(condition, result)).ToArray();

            var elseExpression = node.ElseExpresion == null
                                     ? null
                                     : AlgebrizeExpression(node.ElseExpresion);

            return new AlgebraCaseExpression(caseLabels, elseExpression);
        }

        private AlgebraExpression AlgebrizeSingleRowSubselect(BoundSingleRowSubselect node)
        {
            var query = AlgebrizeQuery(node.BoundQuery);
            return new AlgebraSingleRowSubselect(query);
        }

        private AlgebraExpression AlgebrizeExistsSubselect(BoundExistsSubselect node)
        {
            var query = AlgebrizeQuery(node.BoundQuery);
            return new AlgebraExistsSubselect(query);
        }

        private AlgebraExpression AlgebrizeAllAnySubselect(BoundAllAnySubselect node)
        {
            var expression = AlgebrizeExpression(node.Left);
            var query = AlgebrizeQuery(node.BoundQuery);
            var signature = node.Result.Selected.Signature;
            return new AlgebraAllAnySubselect(expression, query, signature);
        }
    }
}