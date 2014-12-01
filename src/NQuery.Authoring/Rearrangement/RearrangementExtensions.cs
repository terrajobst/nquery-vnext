using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Authoring.Rearrangement.Rearrangers;

namespace NQuery.Authoring.Rearrangement
{
    // TODO: ORDER BY Column    (h)
    // TODO: GROOUP BY Colum    (h)
    // TODO: Table Reference    (v)
    // TODO: Join Side          (h)
    // TODO: Binary operator    (h)
    // TODO: CTE Table          (v)
    // TODO: Union Query        (v)
    // TODO: Except query       (v)
    // TODO: Intersect query    (v)

    public static class RearrangementExtensions
    {
        public static IEnumerable<IRearranger> GetStandardRearrangers()
        {
            return new IRearranger[]
                   {
                       new CommonTableExpressionRearranger(),
                       new SelectColumnRearranger()
                   };
        }

        public static Arrangement GetRearrangement(this SyntaxTree syntaxTree, int position)
        {
            var rearrangers = GetStandardRearrangers();
            return syntaxTree.GetRearrangement(position, rearrangers);
        }

        public static Arrangement GetRearrangement(this SyntaxTree syntaxTree, int position, IEnumerable<IRearranger> rearrangers)
        {
             var arrangments = (from r in rearrangers
                                let a = r.GetArrangement(syntaxTree, position)
                                where a != null
                                select a).ToImmutableArray();

            var closestVertical = (from a in arrangments
                                   where a.VerticalOperation != null
                                   orderby a.VerticalOperation.Span.Start descending
                                   select a.VerticalOperation).FirstOrDefault();

            var closestHorizontal = (from a in arrangments
                                     where a.HorizontalOperation != null
                                     orderby a.HorizontalOperation.Span.Start descending
                                     select a.HorizontalOperation).FirstOrDefault();

            if (closestVertical == null && closestHorizontal == null)
                return null;

            return new Arrangement(closestVertical, closestHorizontal);
        }
    }
}