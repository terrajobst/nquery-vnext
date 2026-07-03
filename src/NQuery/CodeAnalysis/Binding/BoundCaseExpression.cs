using System.Collections.Immutable;
using System.Text;

namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundCaseExpression : BoundExpression
{
    public BoundCaseExpression(IEnumerable<BoundCaseLabel> caseLabels, BoundExpression? elseExpression)
    {
        ThrowIfNull(caseLabels);

        CaseLabels = [.. caseLabels];
        ElseExpression = elseExpression;
    }

    public override BoundNodeKind Kind
    {
        get { return BoundNodeKind.CaseExpression; }
    }

    public override Type Type
    {
        get { return CaseLabels.First().ThenExpression.Type; }
    }

    public ImmutableArray<BoundCaseLabel> CaseLabels { get; }

    public BoundExpression? ElseExpression { get; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(@"CASE ");

        foreach (var boundCaseLabel in CaseLabels)
        {
            sb.Append(@"WHEN ");
            sb.Append(boundCaseLabel.Condition);
            sb.Append(@" THEN ");
            sb.Append(boundCaseLabel.ThenExpression);
        }

        if (ElseExpression is not null)
        {
            sb.Append(@" ELSE ");
            sb.Append(ElseExpression);
        }

        sb.Append(@" END");

        return sb.ToString();
    }
}
