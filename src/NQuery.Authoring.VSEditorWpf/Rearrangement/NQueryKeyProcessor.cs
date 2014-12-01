using System;
using System.Windows.Input;

using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Authoring.VSEditorWpf.Rearrangement
{
    internal sealed class NQueryRearrangementKeyProcessor : KeyProcessor
    {
        private readonly IRearrangeModelManager _rearrangeModelManager;

        public NQueryRearrangementKeyProcessor(IRearrangeModelManager rearrangeModelManager)
        {
            _rearrangeModelManager = rearrangeModelManager;
        }

        private static bool AreModifiersPressed(ModifierKeys modifiers)
        {
            return modifiers == (ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt);
        }

        private void UpdateVisibility()
        {
            _rearrangeModelManager.IsVisible = AreModifiersPressed(Keyboard.Modifiers);
        }

        public override void KeyDown(KeyEventArgs args)
        {
            UpdateVisibility();

            var key = args.Key;
            var modifiers = args.KeyboardDevice.Modifiers;

            if (AreModifiersPressed(modifiers))
            {
                switch (key)
                {
                    case Key.Up:
                        args.Handled = true;
                        _rearrangeModelManager.MoveUp();
                        break;
                    case Key.Down:
                        args.Handled = true;
                        _rearrangeModelManager.MoveDown();
                        break;
                    case Key.Left:
                        args.Handled = true;
                        _rearrangeModelManager.MoveLeft();
                        break;
                    case Key.Right:
                        args.Handled = true;
                        _rearrangeModelManager.MoveRight();
                        break;
                }                
            }

            base.KeyDown(args);
        }

        public override void KeyUp(KeyEventArgs args)
        {
            UpdateVisibility();
            base.KeyUp(args);
        }
    }
}