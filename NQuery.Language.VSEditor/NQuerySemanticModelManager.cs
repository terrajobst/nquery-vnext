using System;
using System.Threading;
using System.Threading.Tasks;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQuerySemanticModelManager : INQuerySemanticModelManager
    {
        private readonly INQuerySyntaxTreeManager _syntaxTreeManager;
        private Compilation _compilation;
        private SemanticModel _semanticModel;
        private CancellationTokenSource _cancellationTokenSource;

        public NQuerySemanticModelManager(INQuerySyntaxTreeManager syntaxTreeManager)
        {
            _syntaxTreeManager = syntaxTreeManager;
            _syntaxTreeManager.SyntaxTreeChanged += SyntaxTreeManagerOnSyntaxTreeChanged;
            _compilation = Compilation.Empty;
            QueueUpdateSemanticModelRequest();
        }

        public Compilation Compilation
        {
            get { return _compilation; }
            set
            {
                if (_compilation != value)
                {
                    _compilation = value;
                    QueueUpdateSemanticModelRequest();
                }
            }
        }

        public SemanticModel SemanticModel
        {
            get { return _semanticModel; }
            private set
            {
                if (_semanticModel != value)
                {
                    _semanticModel = value;
                    OnSemanticModelChanged(EventArgs.Empty);
                }
            }
        }

        private void SyntaxTreeManagerOnSyntaxTreeChanged(object sender, EventArgs e)
        {
            QueueUpdateSemanticModelRequest();
        }

        private void QueueUpdateSemanticModelRequest()
        {
            var syntaxTree = _syntaxTreeManager.SyntaxTree;

            if (_cancellationTokenSource != null)
                _cancellationTokenSource.Cancel();

            if (syntaxTree == null)
                return;

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            _compilation = _compilation.WithSyntaxTree(syntaxTree);

            var compilation = _compilation;

            var sc = SynchronizationContext.Current;
            Task.Factory
                .StartNew(() => compilation.GetSemanticModel(), cancellationToken)
                .ContinueWith(t => sc.Post(s =>
                {
                    SemanticModel = t.Result;
                }, null), TaskContinuationOptions.NotOnCanceled);
        }

        private void OnSemanticModelChanged(EventArgs e)
        {
            var handler = SemanticModelChanged;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<EventArgs> SemanticModelChanged;
    }
}