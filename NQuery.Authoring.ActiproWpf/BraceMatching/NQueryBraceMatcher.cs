using System;
using System.ComponentModel.Composition;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Analysis;
using ActiproSoftware.Text.Analysis.Implementation;

using NQuery.Language.Services.BraceMatching;

namespace NQuery.Language.ActiproWpf.BraceMatching
{
    [ExportLanguageService(typeof(IStructureMatcher))]
    internal sealed class NQueryBraceMatcher : IStructureMatcher
    {
        private readonly IBraceMatchingService _braceMatchingService;

        [ImportingConstructor]
        public NQueryBraceMatcher(IBraceMatchingService braceMatchingService)
        {
            _braceMatchingService = braceMatchingService;
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

            TextSpan leftSpan;
            TextSpan rightSpan;
            if (!_braceMatchingService.TryFindBrace(syntaxTree, position, out leftSpan, out rightSpan))
                return null;

            var leftRange = textBuffer.ToSnapshotRange(snapshot, leftSpan);
            var rightRange = textBuffer.ToSnapshotRange(snapshot, rightSpan);
            var results = new StructureMatchResultCollection
                              {
                                  new StructureMatchResult(leftRange),
                                  new StructureMatchResult(rightRange)
                              };

            return new StructureMatchResultSet(results);
        }
    }
}