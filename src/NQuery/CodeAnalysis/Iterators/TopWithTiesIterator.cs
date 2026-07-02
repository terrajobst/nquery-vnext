using System.Collections;
using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Iterators;

internal sealed class TopWithTiesIterator : TopIterator
{
    private readonly ImmutableArray<TieColumn> _tieColumns;

    private bool _limitReached;

    public TopWithTiesIterator(Iterator input, int limit, IEnumerable<RowBufferEntry> tieEntries, IEnumerable<IComparer> tieComparers)
        : base(input, limit)
    {
        // One typed column per tie-breaker, so capturing the last emitted row and testing
        // each candidate for a tie compares the ORDER BY values unboxed (see TieColumn).
        _tieColumns = [.. tieEntries.Zip(tieComparers, TieColumn.Create)];
    }

    public override void Open()
    {
        base.Open();

        _limitReached = false;
    }

    public override bool Read()
    {
        if (_limitReached)
        {
            if (!Input.Read())
                return false;
        }
        else
        {
            if (!base.Read())
            {
                if (InputExhausted)
                    return false;

                _limitReached = true;
            }
            else
            {
                foreach (var column in _tieColumns)
                    column.Capture();

                return true;
            }
        }

        // Check if the tie values of this row are equal to the one of last row.

        foreach (var column in _tieColumns)
        {
            if (!column.MatchesCaptured())
                return false;
        }

        return true;
    }
}
