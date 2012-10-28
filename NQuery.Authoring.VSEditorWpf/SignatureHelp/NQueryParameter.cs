using System;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace NQuery.Language.VSEditor.SignatureHelp
{
    internal class NQueryParameter : IParameter
    {
        public NQueryParameter(ISignature signature, string name, string documentation, Span locus)
        {
            Signature = signature;
            Name = name;
            Documentation = documentation;
            Locus = locus;
        }

        public ISignature Signature { get; private set; }
        public string Name { get; private set; }
        public string Documentation { get; private set; }
        public Span Locus { get; private set; }
        public Span PrettyPrintedLocus { get; private set; } 
    }
}