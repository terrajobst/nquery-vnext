using System;

using ActiproSoftware.Text.Implementation;

namespace NQuery.Language.ActiproWpf
{
    internal interface ILanguageServiceRegistrar
    {
        void RegisterServices(SyntaxLanguage syntaxLanguage);
    }
}