namespace BrightIdeasSoftware
{
    using System;
    using System.Windows.Forms;

    public class RearrangingDropSink : SimpleDropSink
    {
        private bool acceptExternal;

        public RearrangingDropSink()
        {
            this.acceptExternal = false;
            base.CanDropBetween = true;
            base.CanDropOnBackground = true;
            base.CanDropOnItem = false;
        }

        public RearrangingDropSink(bool acceptDropsFromOtherLists) : this()
        {
            this.AcceptExternal = acceptDropsFromOtherLists;
        }

        protected override void OnModelCanDrop(ModelDropEventArgs args)
        {
            base.OnModelCanDrop(args);
            if (!args.Handled)
            {
                args.Effect = DragDropEffects.Move;
                if (!(this.AcceptExternal || (args.SourceListView == this.ListView)))
                {
                    args.Effect = DragDropEffects.None;
                    args.DropTargetLocation = DropTargetLocation.None;
                    args.InfoMessage = "This list doesn't accept drops from other lists";
                }
                if ((args.DropTargetLocation == DropTargetLocation.Background) && (args.SourceListView == this.ListView))
                {
                    args.Effect = DragDropEffects.None;
                    args.DropTargetLocation = DropTargetLocation.None;
                }
            }
        }

        protected override void OnModelDropped(ModelDropEventArgs args)
        {
            base.OnModelDropped(args);
            if (!args.Handled)
            {
                this.RearrangeModels(args);
            }
        }

        public virtual void RearrangeModels(ModelDropEventArgs args)
        {
            DropTargetLocation dropTargetLocation = args.DropTargetLocation;
            if (dropTargetLocation == DropTargetLocation.Background)
            {
                this.ListView.AddObjects(args.SourceModels);
            }
            else if (dropTargetLocation != DropTargetLocation.AboveItem)
            {
                if (dropTargetLocation != DropTargetLocation.BelowItem)
                {
                    return;
                }
                this.ListView.MoveObjects(args.DropTargetIndex + 1, args.SourceModels);
            }
            else
            {
                this.ListView.MoveObjects(args.DropTargetIndex, args.SourceModels);
            }
            if (args.SourceListView != this.ListView)
            {
                args.SourceListView.RemoveObjects(args.SourceModels);
            }
        }

        public bool AcceptExternal
        {
            get
            {
                return this.acceptExternal;
            }
            set
            {
                this.acceptExternal = value;
            }
        }
    }
}

