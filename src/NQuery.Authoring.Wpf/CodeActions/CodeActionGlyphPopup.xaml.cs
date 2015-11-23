using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NQuery.Authoring.Wpf.CodeActions
{
    public sealed partial class CodeActionGlyphPopup
    {
        private GlyphState _state;

        public CodeActionGlyphPopup()
        {
            InitializeComponent();

            GlyphContextMenu.PlacementTarget = Glyph;
        }

        public void UpdateModels(IReadOnlyCollection<CodeActionModel> actionModels)
        {
            var hasIssueFixes = actionModels.Any(m => m.Kind == CodeActionKind.IssueFix);
            GlyphImage.Source = hasIssueFixes
                ? GetImageSource(CodeActionKind.IssueFix)
                : GetImageSource(CodeActionKind.Refactoring);

            GlyphContextMenu.Items.Clear();
            foreach (var model in actionModels)
            {
                var menuItem = new MenuItem();
                menuItem.Icon = new Image { Source = GetImageSource(model.Kind) };
                menuItem.Header = model.Description;
                menuItem.Click += (s, e) => model.Invoke();
                GlyphContextMenu.Items.Add(menuItem);
            }
        }

        private ImageSource GetImageSource(CodeActionKind kind)
        {
            var resourceKey = GetGlyphResourceKey(kind);
            return (ImageSource) TryFindResource(resourceKey);
        }

        private static string GetGlyphResourceKey(CodeActionKind kind)
        {
            switch (kind)
            {
                case CodeActionKind.IssueFix:
                    return "IssueFixImage";
                case CodeActionKind.Refactoring:
                    return "RefactoringImage";
                default:
                    throw ExceptionBuilder.UnexpectedValue(kind);
            }
        }

        private void GlyphOnMouseEnter(object sender, MouseEventArgs e)
        {
            if (_state == GlyphState.Icon)
                SetState(GlyphState.Hovering);
        }

        private void GlyphOnMouseLeave(object sender, MouseEventArgs e)
        {
            if (_state == GlyphState.Hovering)
                SetState(GlyphState.Icon);
        }

        private void GlyphOnClick(object sender, RoutedEventArgs e)
        {
            if (_state != GlyphState.Expanded)
                SetState(GlyphState.Expanded);
        }

        private void GlyphContextMenuOnClosed(object sender, RoutedEventArgs e)
        {
            SetState(GlyphState.Icon);
        }

        private void GlyphContextMenuOnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var menuItem = GlyphContextMenu.Items.OfType<MenuItem>().FirstOrDefault();
            if (menuItem == null)
                return;

            Dispatcher.BeginInvoke(new Action(() => menuItem.Focus()));
        }

        private void SetState(GlyphState state)
        {
            _state = state;

            switch (state)
            {
                case GlyphState.Icon:
                    Glyph.Style = (Style)TryFindResource("GlyphIconStyle");
                    GlyphContextMenu.IsOpen = false;
                    break;
                case GlyphState.Hovering:
                    Glyph.Style = (Style)TryFindResource("GlyphHoveringStyle");
                    GlyphContextMenu.IsOpen = false;
                    break;
                case GlyphState.Expanded:
                    Glyph.Style = (Style)TryFindResource("GlyphHoveringStyle");
                    GlyphContextMenu.IsOpen = true;
                    break;
                default:
                    throw ExceptionBuilder.UnexpectedValue(state);
            }
        }

        public bool IsExpanded
        {
            get { return _state == GlyphState.Expanded; }
        }

        public void Expand()
        {
            SetState(GlyphState.Expanded);
        }

        public void Collapse()
        {
            SetState(GlyphState.Icon);
        }

        private enum GlyphState
        {
            Icon,
            Hovering,
            Expanded
        }
    }
}
