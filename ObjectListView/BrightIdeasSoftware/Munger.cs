namespace BrightIdeasSoftware
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;

    public class Munger
    {
        private string aspectName;
        private List<string> aspectNameParts;

        public Munger()
        {
            this.aspectNameParts = new List<string>();
        }

        public Munger(string aspectName)
        {
            this.aspectNameParts = new List<string>();
            this.AspectName = aspectName;
        }

        public object GetValue(object target)
        {
            if (this.aspectNameParts.Count == 0)
            {
                return null;
            }
            foreach (string str in this.aspectNameParts)
            {
                if (target == null)
                {
                    return target;
                }
                try
                {
                    target = target.GetType().InvokeMember(str, BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, target, null);
                }
                catch (MissingMethodException)
                {
                    try
                    {
                        target = target.GetType().InvokeMember("Item", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, target, new object[] { str });
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }
            }
            return target;
        }

        public void PutValue(object target, object value)
        {
            if (this.aspectNameParts.Count != 0)
            {
                for (int i = 0; i < (this.aspectNameParts.Count - 1); i++)
                {
                    if (target == null)
                    {
                        break;
                    }
                    try
                    {
                        target = target.GetType().InvokeMember(this.aspectNameParts[i], BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, target, null);
                    }
                    catch (MissingMethodException)
                    {
                        try
                        {
                            target = target.GetType().InvokeMember("Item", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, target, new object[] { this.aspectNameParts[i] });
                        }
                        catch
                        {
                            Debug.WriteLine(string.Format("Cannot invoke '{0}' on a {1}", this.aspectNameParts[i], target.GetType()));
                            return;
                        }
                    }
                }
                if (target != null)
                {
                    string name = this.aspectNameParts[this.aspectNameParts.Count - 1];
                    try
                    {
                        target.GetType().InvokeMember(name, BindingFlags.SetProperty | BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, target, new object[] { value });
                    }
                    catch (MissingMethodException exception)
                    {
                        try
                        {
                            target.GetType().InvokeMember(name, BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, target, new object[] { value });
                        }
                        catch (MissingMethodException exception2)
                        {
                            try
                            {
                                target = target.GetType().InvokeMember("Item", BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance, null, target, new object[] { name, value });
                            }
                            catch
                            {
                                Debug.WriteLine("Invoke PutAspectByName failed:");
                                Debug.WriteLine(exception);
                                Debug.WriteLine(exception2);
                            }
                        }
                    }
                }
            }
        }

        public string AspectName
        {
            get
            {
                return this.aspectName;
            }
            set
            {
                this.aspectName = value;
                if (string.IsNullOrEmpty(this.aspectName))
                {
                    this.aspectNameParts = new List<string>();
                }
                else
                {
                    this.aspectNameParts = new List<string>(this.aspectName.Split(new char[] { '.' }));
                }
            }
        }
    }
}

