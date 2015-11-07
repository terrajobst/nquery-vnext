using System;
using System.Collections.ObjectModel;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Analysis;
using ActiproSoftware.Text.Analysis.Implementation;

using NQuery.Authoring.BraceMatching;

namespace NQuery.Authoring.ActiproWpf.BraceMatching
{
    internal sealed class NQueryBraceMatcher : INQueryBraceMatcher
    {
        public Collection<IBraceMatcher> Matchers { get; } = new Collection<IBraceMatcher>();

        public IStructureMatchResultSet Match(TextSnapshotOffset snapshotOffset, IStructureMatchOptions options)
        {
            var snapshot = snapshotOffset.GetDocumentView();
            if (snapshot == null)
                return null;

            SyntaxTree syntaxTree;
            if (!snapshot.Document.TryGetSyntaxTree(out syntaxTree))
                return null;

            var position = snapshot.Position;

            var result = syntaxTree.MatchBraces(position, Matchers);
            if (!result.IsValid)
                return null;

            var leftRange = snapshot.ToSnapshotRange(result.Left);
            var rightRange = snapshot.ToSnapshotRange(result.Right);
            var results = new StructureMatchResultCollection
                              {
                                  new StructureMatchResult(leftRange),
                                  new StructureMatchResult(rightRange)
                              };

            return new StructureMatchResultSet(results);
        }
    }
}