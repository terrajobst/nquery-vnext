using System;

using ActiproSoftware.Text.Analysis;
using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Text.Tagging.Implementation;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;
using ActiproSoftware.Windows.Extensions;

using NQuery.Authoring.ActiproWpf.BraceMatching;
using NQuery.Authoring.ActiproWpf.Classification;
using NQuery.Authoring.ActiproWpf.Completion;
using NQuery.Authoring.ActiproWpf.Outlining;
using NQuery.Authoring.ActiproWpf.QuickInfo;
using NQuery.Authoring.ActiproWpf.SignatureHelp;
using NQuery.Authoring.ActiproWpf.Squiggles;
using NQuery.Authoring.ActiproWpf.SymbolContent;
using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.Completion;
using NQuery.Authoring.Outlining;
using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.SignatureHelp;

using ICompletionProvider = ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.ICompletionProvider;
using IOutliner = ActiproSoftware.Windows.Controls.SyntaxEditor.Outlining.IOutliner;

namespace NQuery.Authoring.ActiproWpf
{
    public sealed class NQueryLanguage : SyntaxLanguage
    {
        public NQueryLanguage()
            : base("NQuery")
        {
            RegisterStructureMatcher();
            RegisterClassificationTypes();
            RegisterUnnecessaryCodeClassifier();
            RegisterSemanticClassifier();
            RegisterSyntacticClassifier();
            RegisterOutliner();
            RegisterCompletionProvider();
            RegisterQuickInfoProvider();
            RegisterSignatureHelpProvider();
            RegisterSquiggleProviders();
            RegisterSymbolContentProvider();
        }

        private void RegisterStructureMatcher()
        {
            var matcher = new NQueryBraceMatcher();
            matcher.Matchers.AddRange(BraceMatchingExtensions.GetStandardBraceMatchers());

            RegisterService<INQueryBraceMatcher>(matcher);
            RegisterService<IStructureMatcher>(matcher);

            var delimterHighlightingTagger = new TextViewTaggerProvider<DelimiterHighlightTagger>(typeof(DelimiterHighlightTagger));
            RegisterService(delimterHighlightingTagger);
        }

        private void RegisterClassificationTypes()
        {
            var classificationTypes = new NQueryClassificationTypes();
            RegisterService<INQueryClassificationTypes>(classificationTypes);
        }

        private void RegisterUnnecessaryCodeClassifier()
        {
            var provider = new NQueryUnnecessaryCodeClassifierProvider();
            RegisterService(provider);
        }

        private void RegisterSemanticClassifier()
        {
            var provider = new NQuerySemanticClassifierProvider();
            RegisterService(provider);
        }

        private void RegisterSyntacticClassifier()
        {
            var provider = new NQuerySyntacticClassifierProvider();
            RegisterService(provider);
        }

        private void RegisterOutliner()
        {
            var provider = new CollapsedRegionQuickInfoProvider();
            RegisterService(provider);

            var outliner = new NQueryOutliner();
            outliner.Outliners.AddRange(OutliningExtensions.GetStandardOutliners());

            RegisterService<INQueryOutliner>(outliner);
            RegisterService<IOutliner>(outliner);
        }

        private void RegisterCompletionProvider()
        {
            var controller = new NQueryCompletionController();
            RegisterService(controller);

            var provider = new NQueryCompletionProvider(this);
            provider.Providers.AddRange(CompletionExtensions.GetStandardCompletionProviders());

            RegisterService<INQueryCompletionProvider>(provider);
            RegisterService<ICompletionProvider>(provider);
        }

        private void RegisterQuickInfoProvider()
        {
            var provider = new NQueryQuickInfoProvider(this);
            provider.Providers.AddRange(QuickInfoExtensions.GetStandardQuickInfoModelProviders());

            RegisterService<INQueryQuickInfoProvider>(provider);
            RegisterService<IQuickInfoProvider>(provider);
        }

        private void RegisterSignatureHelpProvider()
        {
            var controller = new NQuerySignatureHelpController();
            RegisterService(controller);

            var provider = new NQuerySignatureHelpProvider();
            provider.Providers.AddRange(SignatureHelpExtensions.GetStandardSignatureHelpModelProviders());

            RegisterService<INQuerySignatureHelpProvider>(provider);
            RegisterService<IParameterInfoProvider>(provider);
        }

        private void RegisterSquiggleProviders()
        {
            var syntaxProvider = new NQuerySyntaxErrorSquiggleClassifierProvider();
            RegisterService<CodeDocumentTaggerProvider<NQuerySyntaxErrorSquiggleClassifier>>(syntaxProvider);

            var semanticProvider = new NQuerySemanticErrorSquiggleClassifierProvider();
            RegisterService<CodeDocumentTaggerProvider<NQuerySemanticErrorSquiggleClassifier>>(semanticProvider);

            var issueProvider = new NQuerySemanticIssueSquiggleClassifierProvider();
            RegisterService<CodeDocumentTaggerProvider<NQuerySemanticIssueSquiggleClassifier>>(issueProvider);

            RegisterService(new SquiggleTagQuickInfoProvider());
        }

        private void RegisterSymbolContentProvider()
        {
            var provider = new NQuerySymbolContentProvider(this);
            RegisterService<INQuerySymbolContentProvider>(provider);
        }
    }
}