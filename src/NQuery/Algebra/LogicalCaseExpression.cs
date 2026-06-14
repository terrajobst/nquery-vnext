#nullable enable

using System.Collections.Immutable;

namespace NQuery.Algebra;

internal sealed class LogicalCaseExpression : LogicalExpression
{
    public LogicalCaseExpression(ImmutableArray<LogicalCaseLabel> caseLabels, LogicalExpression? elseExpression)
    {
        CaseLabels = caseLabels;
        ElseExpression = elseExpression;
    }

    public override LogicalExpressionKind Kind => LogicalExpressionKind.Case;

    public override Type Type => CaseLabels[0].ThenExpression.Type;

    public ImmutableArray<LogicalCaseLabel> CaseLabels { get; }

    public LogicalExpression? ElseExpression { get; }
}
