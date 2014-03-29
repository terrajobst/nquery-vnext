using System;

namespace NQuery.Binding
{
    partial class BoundTreeRewriter
    {
        public virtual BoundExpression RewriteExpression(BoundExpression node)
        {
            if (node == null)
                return null;

            switch (node.Kind)
            {
                case BoundNodeKind.TableExpression:
                    return RewriteTableExpression((BoundTableExpression)node);
                case BoundNodeKind.ColumnExpression:
                    return RewriteColumnExpression((BoundColumnExpression)node);
                case BoundNodeKind.UnaryExpression:
                    return RewriteUnaryExpression((BoundUnaryExpression)node);
                case BoundNodeKind.BinaryExpression:
                    return RewriteBinaryExpression((BoundBinaryExpression)node);
                case BoundNodeKind.LiteralExpression:
                    return RewriteLiteralExpression((BoundLiteralExpression)node);
                case BoundNodeKind.VariableExpression:
                    return RewriteVariableExpression((BoundVariableExpression)node);
                case BoundNodeKind.FunctionInvocationExpression:
                    return RewriteFunctionInvocationExpression((BoundFunctionInvocationExpression)node);
                case BoundNodeKind.AggregateExpression:
                    return RewriteAggregateExpression((BoundAggregateExpression)node);
                case BoundNodeKind.PropertyAccessExpression:
                    return RewritePropertyAccessExpression((BoundPropertyAccessExpression)node);
                case BoundNodeKind.MethodInvocationExpression:
                    return RewriteMethodInvocationExpression((BoundMethodInvocationExpression)node);
                case BoundNodeKind.ConversionExpression:
                    return RewriteConversionExpression((BoundConversionExpression)node);
                case BoundNodeKind.IsNullExpression:
                    return RewriteIsNullExpression((BoundIsNullExpression)node);
                case BoundNodeKind.CaseExpression:
                    return RewriteCaseExpression((BoundCaseExpression)node);
                case BoundNodeKind.SingleRowSubselect:
                    return RewriteSingleRowSubselect((BoundSingleRowSubselect)node);
                case BoundNodeKind.ExistsSubselect:
                    return RewriteExistsSubselect((BoundExistsSubselect)node);
                case BoundNodeKind.AllAnySubselect:
                    return RewriteAllAnySubselect((BoundAllAnySubselect)node);
                case BoundNodeKind.ValueSlotExpression:
                    return RewriteValueSlotExpression((BoundValueSlotExpression)node);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual BoundExpression RewriteTableExpression(BoundTableExpression node)
        {
            return node;
        }

        protected virtual BoundExpression RewriteColumnExpression(BoundColumnExpression node)
        {
            return node;
        }

        protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression node)
        {
            return node.Update(RewriteExpression(node.Expression),
                               node.Result);
        }

        protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression node)
        {
            return node.Update(RewriteExpression(node.Left),
                               node.Result,
                               RewriteExpression(node.Right));
        }

        protected virtual BoundExpression RewriteLiteralExpression(BoundLiteralExpression node)
        {
            return node;
        }

        protected virtual BoundExpression RewriteVariableExpression(BoundVariableExpression node)
        {
            return node;
        }

        protected virtual BoundExpression RewriteFunctionInvocationExpression(BoundFunctionInvocationExpression node)
        {
            return node.Update(RewriteExpressions(node.Arguments),
                               node.Result);
        }

        protected virtual BoundExpression RewriteAggregateExpression(BoundAggregateExpression node)
        {
            return node.Update(node.Aggregate,
                               RewriteExpression(node.Argument));
        }

        protected virtual BoundExpression RewritePropertyAccessExpression(BoundPropertyAccessExpression node)
        {
            return node.Update(RewriteExpression(node.Target),
                               node.PropertySymbol);
        }

        protected virtual BoundExpression RewriteMethodInvocationExpression(BoundMethodInvocationExpression node)
        {
            return node.Update(RewriteExpression(node.Target),
                               RewriteExpressions(node.Arguments),
                               node.Result);
        }

        protected virtual BoundExpression RewriteConversionExpression(BoundConversionExpression node)
        {
            return node.Update(RewriteExpression(node.Expression),
                               node.Type,
                               node.Conversion);
        }

        protected virtual BoundExpression RewriteIsNullExpression(BoundIsNullExpression node)
        {
            return node.Update(RewriteExpression(node.Expression));
        }

        protected virtual BoundExpression RewriteCaseExpression(BoundCaseExpression node)
        {
            return node.Update(RewriteCaseLabels(node.CaseLabels),
                               RewriteExpression(node.ElseExpression));
        }

        protected virtual BoundExpression RewriteSingleRowSubselect(BoundSingleRowSubselect node)
        {
            return node.Update(RewriteValueSlot(node.Value),
                               RewriteRelation(node.Relation));
        }

        protected virtual BoundExpression RewriteExistsSubselect(BoundExistsSubselect node)
        {
            return node.Update(RewriteRelation(node.Relation));
        }

        protected virtual BoundExpression RewriteAllAnySubselect(BoundAllAnySubselect node)
        {
            return node.Update(RewriteExpression(node.Left),
                               RewriteRelation(node.Relation),
                               node.Result);
        }

        protected virtual BoundExpression RewriteValueSlotExpression(BoundValueSlotExpression node)
        {
            return node.Update(RewriteValueSlot(node.ValueSlot));
        }
    }
}