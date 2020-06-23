namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;

    public class OLVDataObject : DataObject
    {
        private IList modelObjects;
        private ObjectListView objectListView;

        public OLVDataObject(ObjectListView olv) : this(olv, olv.SelectedObjects)
        {
        }

        public OLVDataObject(ObjectListView olv, IList modelObjects)
        {
            this.modelObjects = new ArrayList();
            this.objectListView = olv;
            this.modelObjects = modelObjects;
        }

        private string ConvertToHtmlFragment(string fragment)
        {
            string str = "http://www.codeproject.com/KB/list/ObjectListView.aspx";
            int length = string.Format("Version:1.0\r\nStartHTML:{0,8}\r\nEndHTML:{1,8}\r\nStartFragment:{2,8}\r\nEndFragment:{3,8}\r\nStartSelection:{2,8}\r\nEndSelection:{3,8}\r\nSourceURL:{4}\r\n{5}", new object[] { 0, 0, 0, 0, str, "" }).Length;
            string str2 = string.Format("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\"><HTML><HEAD></HEAD><BODY><!--StartFragment-->{0}<!--EndFragment--></BODY></HTML>", fragment);
            int num2 = length + str2.IndexOf(fragment);
            int num3 = num2 + fragment.Length;
            return string.Format("Version:1.0\r\nStartHTML:{0,8}\r\nEndHTML:{1,8}\r\nStartFragment:{2,8}\r\nEndFragment:{3,8}\r\nStartSelection:{2,8}\r\nEndSelection:{3,8}\r\nSourceURL:{4}\r\n{5}", new object[] { length, length + str2.Length, num2, num3, str, str2 });
        }

        public void CreateTextFormats()
        {
            List<OLVColumn> columnsInDisplayOrder = this.ListView.ColumnsInDisplayOrder;
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder("<table>");
            foreach (object obj2 in this.ModelObjects)
            {
                builder2.Append("<tr><td>");
                foreach (OLVColumn column in columnsInDisplayOrder)
                {
                    if (column != columnsInDisplayOrder[0])
                    {
                        builder.Append("\t");
                        builder2.Append("</td><td>");
                    }
                    string stringValue = column.GetStringValue(obj2);
                    builder.Append(stringValue);
                    builder2.Append(stringValue);
                }
                builder.AppendLine();
                builder2.AppendLine("</td></tr>");
            }
            builder2.AppendLine("</table>");
            this.SetData(builder.ToString());
            this.SetText(this.ConvertToHtmlFragment(builder2.ToString()), TextDataFormat.Html);
        }

        public ObjectListView ListView
        {
            get
            {
                return this.objectListView;
            }
        }

        public IList ModelObjects
        {
            get
            {
                return this.modelObjects;
            }
        }
    }
}

