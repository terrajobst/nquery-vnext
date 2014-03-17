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
        private readonly Collection<IBraceMatcher> _matchers = new Collection<IBraceMatcher>();

        public Collection<IBraceMatcher> Matchers
        {
            get { return _matchers; }
        }

        public IStructureMatchResultSet Match(TextSnapshotOffset snapshotOffset, IStructureMatchOptions options)
        {
            var snapshot = snapshotOffset.Snapshot;
            var parseData = snapshot.GetParseData();
            if (parseData == null)
                return null;

            var syntaxTree = parseData.SyntaxTree;
            var textBuffer = syntaxTree.TextBuffer;
            var position = snapshotOffset.ToOffset(textBuffer);

            var result = syntaxTree.MatchBraces(position, _matchers);
            if (!result.IsValid)
                return null;

            var leftRange = textBuffer.ToSnapshotRange(snapshot, result.Left);
            var rightRange = textBuffer.ToSnapshotRange(snapshot, result.Right);
            var results = new StructureMatchResultCollection
                              {
                                  new StructureMatchResult(leftRange),
                                  new StructureMatchResult(rightRange)
                              };

            return new StructureMatchResultSet(results);
        }
    }
}