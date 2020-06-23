using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Filetypes;

namespace PackFileManager {
    public partial class FormationPreview : UserControl {
        Dictionary<Line, RectangleF> basicLines = new Dictionary<Line,RectangleF>();
        Dictionary<Line, RectangleF> spanningLines = new Dictionary<Line, RectangleF>();
        RectangleF fullRegion = new RectangleF();
        const int ItemSize = 2;

        Groupformation formation;
        public Groupformation Formation {
            get { return formation; }
            set {
                formation = value;
                basicLines.Clear();
                spanningLines.Clear();
                fullRegion = new Rectangle();
                if (formation != null) {
                    foreach (Line line in formation.Lines) {
                        RectangleF rect = GetRectangle(line);
                        fullRegion = RectangleF.Union(fullRegion, rect);
                    }
                }
                Invalidate();
                //Console.WriteLine("full region is {0}", fullRegion);
            }
        }

        public FormationPreview() {
            Paint += new PaintEventHandler(PaintFormations);
            //Resize += new EventHandler(delegate(object o, EventArgs args) { Console.WriteLine("size now {0}/{1}", Size.Width, Size.Height); });
            ResizeRedraw = true;
        }
        
        Line selectedLine;
        public Line SelectedLine {
            get { return selectedLine; }
            set {
                selectedLine = value;
                Invalidate();
            }
        }

        private void PaintFormations(object sender, PaintEventArgs args) {
            Control c = sender as Control;
            Graphics g = args.Graphics;
   
            if (Formation == null) {
                g.FillRectangle(new SolidBrush(Color.Green), args.ClipRectangle);
                return;
            }

            Brush b = new SolidBrush(Color.Black);
            Matrix transform = g.Transform;

            float heightScale = c.Height / 1.1f / fullRegion.Height;
            float widthScale = c.Width / 1.1f / fullRegion.Width;
            transform.Scale(widthScale, heightScale);

            float xTranslate = -fullRegion.X;
            float yTranslate = -fullRegion.Y;
            transform.Translate(xTranslate, yTranslate);
            g.Transform = transform;

            Font f = new Font(FontFamily.GenericSansSerif, 1f);

            Pen pen = new Pen(Color.Red, 1/(Math.Min(widthScale, heightScale)));
            if (spanningLines.Count != 0) {
                g.DrawRectangles(pen, spanningLines.Values.ToArray());
            }
            pen.Color = Color.Blue;
            g.DrawRectangles(pen, basicLines.Values.ToArray());
            foreach (Line l in basicLines.Keys) {
                RectangleF r = GetRectangle(l);
                g.DrawString(l.Id.ToString(), f, b, r.X, r.Y);
            }
            if (SelectedLine != null) {
                pen.Color = Color.Yellow;
                RectangleF toPaint = GetRectangle(SelectedLine);
                g.DrawRectangles(pen, new RectangleF[] { toPaint });
                if (SelectedLine is RelativeLine) {
                    int reference = (int)(SelectedLine as RelativeLine).RelativeTo;
                    RectangleF refRect;
                    if (!FindIdRect(reference, basicLines, out refRect) && !FindIdRect(reference, spanningLines, out refRect)) {
                        Console.WriteLine("couldn't find reference {1} for {0}", SelectedLine.Id, reference);
                    } else {
                        pen.Color = Color.Green;
                        g.DrawRectangles(pen, new RectangleF[] { refRect });
                    }
                }
            }
        }
        bool FindIdRect(int id, Dictionary<Line, RectangleF> dict, out RectangleF refRect) {
            foreach (Line key in dict.Keys) {
                if (key.Id == id) {
                    refRect = dict[key];
                    return true;
                }
            }
            refRect = new RectangleF();
            return false;
        }

        // create the extension of the given line
        RectangleF GetRectangle(Line line) {
            //Console.WriteLine("retrieving rect for {0}", line.Id);
            if (basicLines.ContainsKey(line)) {
                return basicLines[line];
            } else if (spanningLines.ContainsKey(line)) {
                return spanningLines[line];
            }
            RectangleF result = new RectangleF(0, 0, ItemSize, ItemSize);
            if (line is RelativeLine) {
                BasicLine thisLine = line as RelativeLine;
                Line relativeTo = formation.Lines[(int)(line as RelativeLine).RelativeTo];
                RectangleF relationRect = GetRectangle(relativeTo);
                result.X = (relationRect.X + thisLine.X - Math.Sign(thisLine.X) * ItemSize);
                //if (thisLine.Y == 0) {
                result.X += (relationRect.Width - ItemSize) / 2;
                //}
                result.Y = (relationRect.Y + thisLine.Y - Math.Sign(thisLine.Y) * ItemSize);
                //if (thisLine.X == 0) {
                result.Y += (relationRect.Height - ItemSize) / 2;
                //}
                basicLines.Add(line, result);
            } else if (line is SpanningLine) {
                SpanningLine sl = line as SpanningLine;
                formation.Lines.ForEach(l => {
                    if (sl.Blocks.Contains(l.Id)) {
                        result = RectangleF.Union(result, GetRectangle(l));
                    }
                });
                spanningLines.Add(line, result);
                //result = new Rectangle(minX, minY, Math.Abs(maxX - minX), Math.Abs(maxY - minY));
            } else {
                BasicLine baseLine = line as BasicLine;
                result = new RectangleF(baseLine.X, baseLine.Y, ItemSize, ItemSize);
                basicLines.Add(line, result);
            }
            //Console.WriteLine("rect for {1} is {0}", result, line.Id);
            return result;
        }

        private void InitializeComponent() {
            this.SuspendLayout();
            // 
            // FormationPreview
            // 
            this.Name = "FormationPreview";
            this.Size = new System.Drawing.Size(365, 495);
            this.ResumeLayout(false);

        }
        //List<Rectangle> inverted(ICollection<Rectangle> toInvert) {
        //    List<Rectangle> result = new List<Rectangle>(toInvert.Count);
        //    foreach (Rectangle r in toInvert) {
        //        result.Add(new Rectangle(r.X, -r.Y, r.Width, r.Height));
        //    }
        //    return result;
        //}
    }
}
