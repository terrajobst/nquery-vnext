using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Binding;

// A bound SELECT query, kept in syntax-shaped (per-clause) form. It carries the resolved
// ingredients the algebrizer needs to lower it into relational/logical operators -- it does
// NOT itself perform that lowering (the legacy binder used to, building Filter/Compute/
// GroupBy/Sort/Top/Project relations inline). The resolved sort lives on the ORDER BY clause
// and the DISTINCT keys on the SELECT clause; the output columns are the SELECT columns.
internal sealed class BoundSelectQuery : BoundQuery
{
    public BoundSelectQuery(
        BoundFromClause from,
        BoundWhereClause where,
        BoundGroupByClause groupBy,
        BoundHavingClause having,
        BoundSelectClause select,
        BoundOrderByClause orderBy,
        BoundTopClause top)
    {
        From = from;
        Where = where;
        GroupBy = groupBy;
        Having = having;
        Select = select;
        OrderBy = orderBy;
        Top = top;
        OutputColumns = select.Columns.Select(c => c.Column).ToImmutableArray();
    }

    public override BoundNodeKind Kind => BoundNodeKind.SelectQuery;

    public BoundFromClause From { get; }

    public BoundWhereClause Where { get; }

    public BoundGroupByClause GroupBy { get; }

    public BoundHavingClause Having { get; }

    public BoundSelectClause Select { get; }

    public BoundOrderByClause OrderBy { get; }

    public BoundTopClause Top { get; }

    public override ImmutableArray<QueryColumnInstanceSymbol> OutputColumns { get; }
}
