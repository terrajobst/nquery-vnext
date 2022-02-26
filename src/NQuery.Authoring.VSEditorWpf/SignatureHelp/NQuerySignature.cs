using System.Collections.Immutable;
using System.Collections.ObjectModel;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.VSEditorWpf.SignatureHelp
{
    internal class NQuerySignature : ISignature
    {
        private IParameter _currentParameter;

        internal NQuerySignature(ITrackingSpan applicableSpan, SignatureItem signatureItem, int selectedParameter)
        {
            var parameters = signatureItem.Parameters.Select(CreateParameter).OfType<IParameter>().ToImmutableArray();

            ApplicableToSpan = applicableSpan;
            Content = signatureItem.Content;
            Parameters = new ReadOnlyCollection<IParameter>(parameters);
            CurrentParameter = selectedParameter >= 0 && selectedParameter < parameters.Length
                                   ? parameters[selectedParameter]
                                   : null;
        }

        private NQueryParameter CreateParameter(ParameterItem p)
        {
            return new NQueryParameter(this, p.Name, string.Empty, new Span(p.Span.Start, p.Span.Length));
        }

        public IParameter CurrentParameter
        {
            get { return _currentParameter; }
            internal set
            {
                if (_currentParameter != value)
                {
                    var prevCurrentParameter = _currentParameter;
                    _currentParameter = value;
                    RaiseCurrentParameterChanged(prevCurrentParameter, _currentParameter);
                }
            }
        }

        private void RaiseCurrentParameterChanged(IParameter prevCurrentParameter, IParameter newCurrentParameter)
        {
            var tempHandler = CurrentParameterChanged;
            if (tempHandler != null)
                tempHandler(this, new CurrentParameterChangedEventArgs(prevCurrentParameter, newCurrentParameter));
        }

        public ITrackingSpan ApplicableToSpan { get; private set; }
        public string Content { get; private set; }
        public string PrettyPrintedContent { get; private set; }
        public string Documentation { get; private set; }
        public ReadOnlyCollection<IParameter> Parameters { get; internal set; }
        public event EventHandler<CurrentParameterChangedEventArgs> CurrentParameterChanged;
    }
}