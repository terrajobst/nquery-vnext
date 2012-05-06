using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;

using NQuery.Language;

namespace NQueryViewer.EditorIntegration
{
    internal sealed class NQuerySyntaxTreeManager : INQuerySyntaxTreeManager
    {
        private readonly ITextBuffer _textBuffer;
        private SyntaxTree _syntaxTree;
        private CancellationTokenSource _cancellationTokenSource;

        public NQuerySyntaxTreeManager(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
            _textBuffer.PostChanged += TextBufferOnPostChanged;
            QueueParseRequest();
        }

        public SyntaxTree SyntaxTree
        {
            get { return _syntaxTree; }
            private set
            {
                if (_syntaxTree != value)
                {
                    _syntaxTree = value;
                    OnSyntaxTreeChanged(EventArgs.Empty);
                }
            }
        }

        private void TextBufferOnPostChanged(object sender, EventArgs e)
        {
            QueueParseRequest();
        }

        private void QueueParseRequest()
        {
            var snapshot = _textBuffer.CurrentSnapshot;

            if (_cancellationTokenSource != null)
                _cancellationTokenSource.Cancel();

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            var sc = SynchronizationContext.Current;
            Task.Factory
                .StartNew(() => SyntaxTree.ParseQuery(snapshot.GetText()), cancellationToken)
                .ContinueWith(t => sc.Post(s =>
                {
                    if (_textBuffer.CurrentSnapshot == snapshot)
                        SyntaxTree = t.Result;
                }, null), TaskContinuationOptions.NotOnCanceled);
        }

        private void OnSyntaxTreeChanged(EventArgs e)
        {
            var handler = SyntaxTreeChanged;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<EventArgs> SyntaxTreeChanged;
    }
}