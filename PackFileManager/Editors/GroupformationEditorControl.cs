using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using Filetypes;
using CommonDialogs;

namespace PackFileManager {
    public delegate T Parser<T>(string parse);
    
    interface IDataBind {
        void BindTo(object line);
    }
    public interface IModifiable {
        void SetModified(bool val);
    }

    public partial class GroupformationEditorControl : UserControl, IModifiable {
        bool modified = false;
        public bool Modified {
            get { return modified; }
            set {
                modified = value;
                if (modified) {
                    formationPreview.Formation = EditedFormation;
                    formationPreview.Invalidate();
                }
            }
        }
        
        public delegate string ListItemRenderer<T>(T o);
        public static readonly ListItemRenderer<object> ToStringRenderer = delegate(object o) { return o.ToString(); };
        
        List<IDataBind> boundLines = new List<IDataBind>();
        List<IDataBind> formationBind = new List<IDataBind>();
  
        delegate void PropertySetter(TextBox box);
        public GroupformationEditorControl() {
            InitializeComponent();

            linesList.DisplayMember = "Display";
            unitPriorityList.DisplayMember = "Display";
            
            formationBind.Add(new TextBinding<Groupformation, string>(nameInput, delegate(string s) {return s;}, "Name", this));
            formationBind.Add(new TextBinding<Groupformation, float>(priorityInput, float.Parse, "Priority", this));
            formationBind.Add(new TextBinding<Groupformation, uint>(purposeComboBox, uint.Parse, "Purpose", this));

            // create all the text bindings
            boundLines.Add(new TextBinding<RelativeLine, uint>(relativeToInput, uint.Parse, "RelativeTo", this));
            boundLines.Add(new TextBinding<RelativeLine, float>(linePriorityInput, float.Parse, "Priority", this));
            boundLines.Add(new TextBinding<RelativeLine, float>(spacingInput, float.Parse, "Spacing", this));
            boundLines.Add(new TextBinding<RelativeLine, float>(crescOffsetInput, float.Parse, "Crescent_Y_Offset", this));
            boundLines.Add(new TextBinding<RelativeLine, float>(xInput, float.Parse, "X", this));
            boundLines.Add(new TextBinding<RelativeLine, float>(yInput, float.Parse, "Y", this));
            boundLines.Add(new TextBinding<RelativeLine, int>(minThresholdInput, int.Parse, "MinThreshold", this));
            boundLines.Add(new TextBinding<RelativeLine, int>(maxThresholdInput, int.Parse, "MaxThreshold", this));
            
            boundLines.Add(new ShapeRadioButtonBinding(lineRadioButton, 0, this));
            boundLines.Add(new ShapeRadioButtonBinding(columnRadioButton, 1, this));
            boundLines.Add(new ShapeRadioButtonBinding(crescFrontRadioButton, 2, this));
            boundLines.Add(new ShapeRadioButtonBinding(crescBackRadioButton, 3, this));
            
            linesList.SelectedIndexChanged += new EventHandler(LineSelected);
            linesList.SelectedIndexChanged += new EventHandler(delegate(object sender, EventArgs args) { 
                deleteUnitPriorityButton.Enabled = false;
                editUnitPriorityButton.Enabled = false;
            });
            unitPriorityList.SelectedIndexChanged += new EventHandler(PrioritySelected);
        }
        void PrioritySelected(object sender, EventArgs args) {
            deleteUnitPriorityButton.Enabled = unitPriorityList.SelectedIndex != -1;
            //editUnitPriorityButton.Enabled = unitPriorityList.SelectedIndex != -1;
        }
        
        public void SetModified(bool val) {
            Modified = val;
        }

        private Groupformation formation;
        public Groupformation EditedFormation {
            get {
                return formation;
            }
            set {
                formation = value;
                if (formation != null) {
                    nameInput.Text = formation.Name;
                    // purposeComboBox.Text = ((int)formation.Purpose).ToString();
                    FillListBox(factionList, formation.Factions);
                    FillListBox(linesList, formation.Lines);
                    priorityInput.Text = formation.Priority.ToString();
    
                    formationPreview.Formation = formation;
                    
                    //Console.WriteLine("binding {0} fields", formationBind.Count);
                    formationBind.ForEach(b => b.BindTo(formation));
                }
            }
        }

        #region Setting Edited Line
        void LineSelected(object sender, EventArgs args) {
            SelectedLine = linesList.SelectedItem as Line;
        }
        
        Line selectedLine;
        public Line SelectedLine {
            get { return selectedLine; }
            set {
                BasicLine relativeLine = value as BasicLine;
                foreach(IDataBind editor in boundLines) {
                    editor.BindTo(relativeLine);
                }

                selectedLine = value;
                if (selectedLine is BasicLine) {
                    BasicLine line = selectedLine as BasicLine;
                    FillListBox(unitPriorityList, line.PriorityClassPairs);
                } else if (selectedLine is SpanningLine) {
                    FillListBox(unitPriorityList, (selectedLine as SpanningLine).Blocks);
                }
                formationPreview.SelectedLine = value;
                
                // can't delete main line
                deleteLineButton.Enabled = CanDeleteLine(SelectedLine);
                addUnitPriorityButton.Enabled = SelectedLine != null;
            }
        }
        #endregion
        
        void FillListBox<T>(ListBox listbox, List<T> list) {
            listbox.Items.Clear();
            list.ForEach(val => { listbox.Items.Add(val); });
        }
  
        #region Line Delete
        private void deleteLineButton_Click(object sender, EventArgs e) {
            if (SelectedLine != null) {
                EditedFormation.Lines.Remove(SelectedLine);
                linesList.Items.Remove(SelectedLine);
                linesList.SelectedIndex = linesList.Items.Count - 1;
                SetModified(true);
            }
        }

        bool CanDeleteLine(Line line) {
            bool result = line != null;
            if (result) {
                result &= line.Id == linesList.Items.Count-1;
                result &= line.Type != LineType.absolute;
                // can't delete lines used as reference for relative position
                // or lines contained in a spanned line
                if (result) {
                    EditedFormation.Lines.ForEach(l => result &= !RelatesTo (line, l) && !ContainsAsSpan(line, l));
                }
            }
            return result;
        }
        static bool RelatesTo(Line line, Line targetCandidate) {
            bool result = false;
            if (targetCandidate is RelativeLine) {
                result = (targetCandidate as RelativeLine).RelativeTo == line.Id;
            }
            return result;
        }
        static bool ContainsAsSpan(Line line, Line spanCandidate) {
            SpanningLine span = spanCandidate as SpanningLine;
            bool result = (span != null) && span.Blocks.Contains(line.Id);
            return result;
        }
        #endregion

        #region Unit Priority / Spanned Lines
        private void addUnitPriorityButton_Click(object sender, EventArgs e) {
            Console.WriteLine("adding priority");
            if (SelectedLine is BasicLine) {
                PriorityClassPair pair = PromptPriorityClassPair();
                if (pair != null) {
                    unitPriorityList.Items.Add(pair);
                    (SelectedLine as BasicLine).PriorityClassPairs.Add(pair);
                    SetModified(true);
                }
            } else if (SelectedLine is SpanningLine) {
                InputBox box = new InputBox();
                SpanningLine spanningLine = SelectedLine as SpanningLine;
                if (box.ShowDialog() == DialogResult.OK) {
                    int add;
                    if (int.TryParse(box.Input, out add)) {
                        if (!spanningLine.Blocks.Contains(add)) {
                            spanningLine.Blocks.Add(add);
                            unitPriorityList.Items.Add(add);
                            Modified = true;
                        }
                    } else {
                        MessageBox.Show("Invalid input");
                        return;
                    }
                }
            }
        }

        private void editUnitPriorityButton_Click(object sender, EventArgs e) {
            if (SelectedLine is BasicLine) {
                PriorityClassPair edited = unitPriorityList.SelectedItem as PriorityClassPair;
                PriorityClassPair entered = PromptPriorityClassPair(edited);
                if (edited.Priority != entered.Priority && edited.UnitClass.ClassIndex != entered.UnitClass.ClassIndex) {
                    Console.WriteLine("changed prio/unit pair");
                    edited.Priority = entered.Priority;
                    edited.UnitClass.ClassIndex = entered.UnitClass.ClassIndex;
                    Modified = true;
                }
            }
        }
        
        PriorityClassPair PromptPriorityClassPair(PriorityClassPair original = null) {
            PriorityClassPair result = null;
            string defaultPriority = (original != null) ? original.Priority.ToString() : "1";
            InputBox box = new InputBox {
                Input = defaultPriority
            };
            if (box.ShowDialog() == DialogResult.OK) {
                float priority;
                int classIndex;
                if (float.TryParse(box.Input, out priority)) {
                    box.Input = (original != null) ? original.UnitClass.ClassIndex.ToString() : "0";
                    if (box.ShowDialog() == DialogResult.OK) {
                        if (int.TryParse(box.Input, out classIndex)) {
                            result = new PriorityClassPair {
                                Priority = priority
                            };
                            result.UnitClass.ClassIndex = classIndex;
                        } else {
                            MessageBox.Show("Invalid input");
                        }
                    }
                } else {
                    MessageBox.Show("Invalid input");
                }
            }
            return result;
        }

        private void deleteUnitPriorityButton_Click(object sender, EventArgs e) {
            int deleteAt = unitPriorityList.SelectedIndex;
            if (SelectedLine is BasicLine) {
                (SelectedLine as BasicLine).PriorityClassPairs.RemoveAt(deleteAt);
            } else {
                (SelectedLine as SpanningLine).Blocks.RemoveAt(deleteAt);
            }
            SetModified(true);
            unitPriorityList.Items.RemoveAt(deleteAt);
        }
        #endregion

        private void preview_Click(object sender, EventArgs e) {
            Console.WriteLine("showing preview");
            Form previewForm = new Form {
                Size = new Size(300, 300)
            };
            FormationPreview drawPanel = new FormationPreview {
                Dock = DockStyle.Fill,
                Formation = EditedFormation
            };
            previewForm.Controls.Add(drawPanel);
            previewForm.ShowDialog();
        }

        private void addLineButton_Click(object sender, EventArgs e) {
            Line newLine = new RelativeLine {
                Id = linesList.Items.Count,
                RelativeTo = 0
            };
            AddLine(newLine);
        }

        private void addSpanButton_Click(object sender, EventArgs e) {
            AddLine(new SpanningLine {
                Id = linesList.Items.Count
            });
        }
        private void AddLine(Line newLine) {
            linesList.Items.Add(newLine);
            EditedFormation.Lines.Add(newLine);
            Modified = true;
        }
    }

    /*
     * Class implementing the binding of text box to a Relative Line's property.
     * Need to do this manually because the mono implementation doesn't seem to respect the
     * DataBindings.Clear() and will still set values to past objects.
     */
    public class TextBinding<T, D> : IDataBind {
        Parser<D> Parse;
        TextBox TextBox;
        PropertyInfo Info;
        object bindTarget;
        IModifiable NotificationTarget;
        
        // create an instance, bound to the given text box, using the given parser
        // to set the given property.
        public TextBinding(TextBox box, Parser<D> parser, string propertyName, IModifiable notify) {
            Parse = parser;
            Info = typeof(T).GetProperty(propertyName);
            if (Info == null) {
                throw new ArgumentException(string.Format("property {0} not valid", propertyName));
            }
            TextBox = box;
            box.LostFocus += delegate(object sender, EventArgs args) { SetValue(); };
            box.Validating += Validator;
            NotificationTarget = notify;
        }

        // the line for which to edit the property
        public object Target { 
            get {
                return bindTarget;
            }
            set {
                SetValue ();
                bindTarget = value;
                try {
                if (bindTarget != null) {
                    object val = Info.GetValue(bindTarget, null);
                    TextBox.Text = val.ToString();
                    TextBox.Enabled = true;
                } else {
                    TextBox.Text = "";
                    TextBox.Enabled = false;
                }
                } catch {
                    TextBox.Text = "";
                    TextBox.Enabled = false;
                }
            }
        }
        
        // implement ILineEditor
        public void BindTo(object l) {
            Target = l;
        }
        
        // helper method, called from Line property and LostFocus event
        void SetValue() {
            if (bindTarget is T && TextBox.Enabled) {
                try {
                    D newValue = Parse(TextBox.Text);
                    D oldValue = (D) Info.GetValue(bindTarget, null);
                    
                    if (Comparer<D>.Default.Compare(oldValue, newValue) != 0) {
                        Info.SetValue(bindTarget, Parse(TextBox.Text), null);
                        if (NotificationTarget != null) {
                            NotificationTarget.SetModified(true);
                        }
                    }
                } catch (Exception e) {
                    Console.WriteLine(string.Format("setting value {0} to {1} failed: {2}", Info.Name, TextBox.Text, e));
                }
            }
        }
        
        // validates the box by trying to parse its text with the parser
        // cancels upon exception
        void Validator(object sender, CancelEventArgs args) {
            try {
                if (bindTarget != null && TextBox.Enabled) {
                    Parse((sender as TextBox).Text);
                }
            } catch {
                Console.WriteLine("Failed validation of {0} for {1}", Info.Name, TextBox.Text);
                args.Cancel = true;
            }
        }
    }

    public class ShapeRadioButtonBinding : IDataBind {
        private RadioButton Button;
        private int Shape;
        BasicLine Line;
        IModifiable NotifyTarget;

        public ShapeRadioButtonBinding(RadioButton button, int shape, IModifiable toNotify) {
            Button = button;
            Shape = shape;
            NotifyTarget = toNotify;
            
            Button.Click += new EventHandler(delegate (object sender, EventArgs args) { if (Line != null && Line.Shape != Shape) {
                    Line.Shape = Shape;
                    NotifyTarget.SetModified(true);
                }});
        }
        
        public void BindTo(object o) {
            if (o == null) {
                Button.Checked = false;
                Button.Enabled = false;
            } else {
                Line = (BasicLine) o;
                Button.Checked = Line.Shape == Shape;
                Button.Enabled = true;
            }
        }
    }
}
