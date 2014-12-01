using System;

namespace NQuery.Authoring.VSEditorWpf.Rearrangement
{
    public interface IRearrangeModelManager
    {
        void MoveUp();
        void MoveDown();
        void MoveLeft();
        void MoveRight();
        bool IsVisible { get; set; }
        RearrangeModel Model { get; }
        event EventHandler<EventArgs> ModelChanged;
        event EventHandler<EventArgs> IsVisibleChanged;
    }
}