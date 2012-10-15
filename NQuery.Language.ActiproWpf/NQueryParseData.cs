using ActiproSoftware.Text;
using ActiproSoftware.Text.Parsing;
using NQuery.Language;

namespace NQueryViewerActiproWpf
{
    public sealed class NQueryParseData : IParseData
    {
        private readonly ITextSnapshot _snapshot;
        private readonly SyntaxTree _syntaxTree;

        public NQueryParseData(ITextSnapshot snapshot, SyntaxTree syntaxTree)
        {
            _snapshot = snapshot;
            _syntaxTree = syntaxTree;
        }

        public ITextSnapshot Snapshot
        {
            get { return _snapshot; }
        }

        public SyntaxTree SyntaxTree
        {
            get { return _syntaxTree; }
        }
    }
}