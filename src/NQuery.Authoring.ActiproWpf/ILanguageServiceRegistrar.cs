using System;

using ActiproSoftware.Text.Implementation;

namespace NQuery.Authoring.ActiproWpf
{
    internal interface ILanguageServiceRegistrar
    {
        void RegisterServices(SyntaxLanguage syntaxLanguage);
    }
}