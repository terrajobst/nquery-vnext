using System;

namespace NQuery.Binding
{
    partial class BoundTreeWalker
    {
        public virtual void VisitExpression(BoundExpression node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.TableExpression:
                    VisitTableExpression((BoundTableExpression)node);
                    break;
                case BoundNodeKind.ColumnExpression:
                    VisitColumnExpression((BoundColumnExpression)node);
                    break;
                case BoundNodeKind.UnaryExpression:
                    VisitUnaryExpression((BoundUnaryExpression)node);
                    break;
                case BoundNodeKind.BinaryExpression:
                    VisitBinaryExpression((BoundBinaryExpression)node);
                    break;
                case BoundNodeKind.LiteralExpression:
                    VisitLiteralExpression((BoundLiteralExpression)node);
                    break;
                case BoundNodeKind.VariableExpression:
                    VisitVariableExpression((BoundVariableExpression)node);
                    break;
                case BoundNodeKind.FunctionInvocationExpression:
                    VisitFunctionInvocationExpression((BoundFunctionInvocationExpression)node);
                    break;
                case BoundNodeKind.AggregateExpression:
                    VisitAggregateExpression((BoundAggregateExpression)node);
                    break;
                case BoundNodeKind.PropertyAccessExpression:
                    VisitPropertyAccessExpression((BoundPropertyAccessExpression)node);
                    break;
                case BoundNodeKind.MethodInvocationExpression:
                    VisitMethodInvocationExpression((BoundMethodInvocationExpression)node);
                    break;
                case BoundNodeKind.ConversionExpression:
                    VisitConversionExpression((BoundConversionExpression)node);
                    break;
                case BoundNodeKind.IsNullExpression:
                    VisitIsNullExpression((BoundIsNullExpression)node);
                    break;
                case BoundNodeKind.CaseExpression:
                    VisitCaseExpression((BoundCaseExpression)node);
                    break;
                case BoundNodeKind.SingleRowSubselect:
                    VisitSingleRowSubselect((BoundSingleRowSubselect)node);
                    break;
                case BoundNodeKind.ExistsSubselect:
                    VisitExistsSubselect((BoundExistsSubselect)node);
                    break;
                case BoundNodeKind.ValueSlotExpression:
                    VisitValueSlotExpression((BoundValueSlotExpression)node);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual void VisitTableExpression(BoundTableExpression node)
        {
        }

        protected virtual void VisitColumnExpression(BoundColumnExpression node)
        {
        }

        protected virtual void VisitUnaryExpression(BoundUnaryExpression node)
        {
            VisitExpression(node.Expression);
        }

        protected virtual void VisitBinaryExpression(BoundBinaryExpression node)
        {
            VisitExpression(node.Left);
            VisitExpression(node.Right);
        }

        protected virtual void VisitLiteralExpression(BoundLiteralExpression node)
        {
        }

        protected virtual void VisitVariableExpression(BoundVariableExpression node)
        {
        }

        protected virtual void VisitFunctionInvocationExpression(BoundFunctionInvocationExpression node)
        {
            foreach (var argument in node.Arguments)
                VisitExpression(argument);
        }

        protected virtual void VisitAggregateExpression(BoundAggregateExpression node)
        {
            VisitExpression(node.Argument);
        }

        protected virtual void VisitPropertyAccessExpression(BoundPropertyAccessExpression node)
        {
            VisitExpression(node.Target);
        }

        protected virtual void VisitMethodInvocationExpression(BoundMethodInvocationExpression node)
        {
            VisitExpression(node.Target);
            foreach (var argument in node.Arguments)
                VisitExpression(argument);
        }

        protected virtual void VisitConversionExpression(BoundConversionExpression node)
        {
            VisitExpression(node.Expression);
        }

        protected virtual void VisitIsNullExpression(BoundIsNullExpression node)
        {
            VisitExpression(node.Expression);
        }

        protected virtual void VisitCaseExpression(BoundCaseExpression node)
        {
            foreach (var caseLabel in node.CaseLabels)
            {
                VisitExpression(caseLabel.Condition);
                VisitExpression(caseLabel.ThenExpression);
            }

            if (node.ElseExpression != null)
                VisitExpression(node.ElseExpression);
        }

        protected virtual void VisitSingleRowSubselect(BoundSingleRowSubselect node)
        {
            VisitRelation(node.Relation);
        }

        protected virtual void VisitExistsSubselect(BoundExistsSubselect node)
        {
            VisitRelation(node.Relation);
        }

        protected virtual void VisitValueSlotExpression(BoundValueSlotExpression node)
        {
        }
    }
}