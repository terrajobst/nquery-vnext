using System;
using System.ComponentModel.Composition;

using ActiproSoftware.Text.Implementation;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.ActiproWpf.CodeActions
{
    [Export(typeof(ILanguageServiceRegistrar))]
    internal sealed class CodeRefactoringServiceRegistrar : ILanguageServiceRegistrar
    {
        [Import]
        public ICodeRefactoringService CodeRefactoringService { get; set; }

        public void RegisterServices(SyntaxLanguage syntaxLanguage)
        {
            syntaxLanguage.RegisterService(CodeRefactoringService);
        }
    }
}