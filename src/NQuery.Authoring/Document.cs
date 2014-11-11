using System;
using System.Threading;
using System.Threading.Tasks;

using NQuery.Text;

namespace NQuery.Authoring
{
    public sealed class Document
    {
        private readonly DocumentKind _kind;
        private readonly DataContext _dataContext;
        private readonly SourceText _text;

        private Task<SyntaxTree> _syntaxTreeTask;
        private Task<Compilation> _compilationTask;
        private Task<SemanticModel> _semanticModelTask;

        public Document(DocumentKind kind, DataContext dataContext, SourceText text)
        {
            _kind = kind;
            _dataContext = dataContext;
            _text = text;
        }

        public DocumentKind Kind
        {
            get { return _kind; }
        }

        public DataContext DataContext
        {
            get { return _dataContext; }
        }

        public SourceText Text
        {
            get { return _text; }
        }

        public bool TryGetSyntaxTree(out SyntaxTree syntaxTree)
        {
            if (_syntaxTreeTask != null && _syntaxTreeTask.IsCompleted)
            {
                syntaxTree = _syntaxTreeTask.Result;
                return true;
            }

            syntaxTree = null;
            return false;
        }

        public bool TryGetCompilation(out Compilation compilation)
        {
            if (_compilationTask != null && _compilationTask.IsCompleted)
            {
                compilation = _compilationTask.Result;
                return true;
            }

            compilation = null;
            return false;
        }

        public bool TryGetSemanticModel(out SemanticModel semanticModel)
        {
            if (_semanticModelTask != null && _semanticModelTask.IsCompleted)
            {
                semanticModel = _semanticModelTask.Result;
                return true;
            }

            semanticModel = null;
            return false;
        }

        public Task<SyntaxTree> GetSyntaxTreeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_syntaxTreeTask != null)
                return _syntaxTreeTask;

            var task = Task.Run(() => ComputeSyntaxTree(), cancellationToken);
            Interlocked.CompareExchange(ref _syntaxTreeTask, task, null);

            return _syntaxTreeTask;
        }

        public Task<Compilation> GetCompilationAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_compilationTask!= null)
                return _compilationTask;

            var task = ComputeCompilationAsync(cancellationToken);
            Interlocked.CompareExchange(ref _compilationTask, task, null);

            return _compilationTask;
        }

        public Task<SemanticModel> GetSemanticModelAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_semanticModelTask != null)
                return _semanticModelTask;

            var task = ComputeSemanticModelAsync(cancellationToken);
            Interlocked.CompareExchange(ref _semanticModelTask, task, null);

            return _semanticModelTask;
        }

        private SyntaxTree ComputeSyntaxTree()
        {
            switch (_kind)
            {
                case DocumentKind.Query:
                    return SyntaxTree.ParseQuery(_text);
                case DocumentKind.Expression:
                    return SyntaxTree.ParseExpression(_text);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task<Compilation> ComputeCompilationAsync(CancellationToken cancellationToken)
        {
            var syntaxTree = await GetSyntaxTreeAsync(cancellationToken);
            return new Compilation(_dataContext, syntaxTree);
        }

        private async Task<SemanticModel> ComputeSemanticModelAsync(CancellationToken cancellationToken)
        {
            var compilation = await GetCompilationAsync(cancellationToken);
            return await Task.Run(() => compilation.GetSemanticModel(), cancellationToken);
        }

        public Document WithKind(DocumentKind kind)
        {
            return kind == _kind ? this : new Document(kind, _dataContext, _text);
        }

        public Document WithDataContext(DataContext dataContext)
        {
            return dataContext == _dataContext ? this : new Document(_kind, dataContext, _text);
        }

        public Document WithText(SourceText text)
        {
            return text == _text ? this : new Document(_kind, _dataContext, text);
        }
    }
}