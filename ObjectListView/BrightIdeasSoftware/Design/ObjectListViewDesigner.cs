namespace BrightIdeasSoftware.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;

    internal class ObjectListViewDesigner : ListViewDesigner
    {
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            List<string> list = new List<string>(new string[] { "BackgroundImage", "BackgroundImageTiled", "HotTracking", "HoverSelection", "LabelEdit", "VirtualListSize", "VirtualMode" });
            foreach (string str in properties.Keys)
            {
                if (str.StartsWith("ToolTip"))
                {
                    list.Add(str);
                }
            }
            foreach (string str2 in list)
            {
                PropertyDescriptor descriptor = TypeDescriptor.CreateProperty(typeof(ObjectListViewDesigner), (PropertyDescriptor) properties[str2], new Attribute[] { new BrowsableAttribute(false) });
                properties[str2] = descriptor;
            }
        }
    }
}

