using ActiproSoftware.Text.Parsing;
using NQuery.Language;

namespace NQueryViewerActiproWpf
{
    [ExportLanguageService(typeof(IParser))]
    internal sealed class NQueryParser : IParser
    {
        public string Key
        {
            get { return GetType().Name; }
        }

        public IParseData Parse(IParseRequest request)
        {
            var snapshot = request.Snapshot;
            var syntaxTree = SyntaxTree.ParseQuery(snapshot.Text);
            return new NQueryParseData(snapshot, syntaxTree);
        }
    }
}