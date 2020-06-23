namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public interface IDropSink
    {
        void DrawFeedback(Graphics g, Rectangle bounds);
        void Drop(DragEventArgs args);
        void Enter(DragEventArgs args);
        void GiveFeedback(GiveFeedbackEventArgs args);
        void Leave();
        void Over(DragEventArgs args);
        void QueryContinue(QueryContinueDragEventArgs args);

        ObjectListView ListView { get; set; }
    }
}

