namespace BrightIdeasSoftware
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows.Forms;

    public class EditorRegistry
    {
        private Dictionary<System.Type, EditorCreatorDelegate> creatorMap = new Dictionary<System.Type, EditorCreatorDelegate>();
        private EditorCreatorDelegate defaultCreator;
        private EditorCreatorDelegate firstChanceCreator;

        public EditorRegistry()
        {
            this.InitializeStandardTypes();
        }

        protected Control CreateEnumEditor(System.Type type)
        {
            return new EnumCellEditor(type);
        }

        public Control GetEditor(object model, OLVColumn column, object value)
        {
            Control control;
            if (this.firstChanceCreator != null)
            {
                control = this.firstChanceCreator(model, column, value);
                if (control != null)
                {
                    return control;
                }
            }
            if ((value != null) && this.creatorMap.ContainsKey(value.GetType()))
            {
                control = this.creatorMap[value.GetType()](model, column, value);
                if (control != null)
                {
                    return control;
                }
            }
            if (this.defaultCreator != null)
            {
                return this.defaultCreator(model, column, value);
            }
            if ((value != null) && value.GetType().IsEnum)
            {
                return this.CreateEnumEditor(value.GetType());
            }
            return null;
        }

        private void InitializeStandardTypes()
        {
            this.Register(typeof(bool), typeof(BooleanCellEditor));
            this.Register(typeof(short), typeof(IntUpDown));
            this.Register(typeof(int), typeof(IntUpDown));
            this.Register(typeof(long), typeof(IntUpDown));
            this.Register(typeof(ushort), typeof(UintUpDown));
            this.Register(typeof(uint), typeof(UintUpDown));
            this.Register(typeof(ulong), typeof(UintUpDown));
            this.Register(typeof(float), typeof(FloatCellEditor));
            this.Register(typeof(double), typeof(FloatCellEditor));
            this.Register(typeof(DateTime), (EditorCreatorDelegate) ((model, column, value) => new DateTimePicker { Format = DateTimePickerFormat.Short }));
        }

        public void Register(System.Type type, EditorCreatorDelegate creator)
        {
            this.creatorMap[type] = creator;
        }

        public void Register(System.Type type, System.Type controlType)
        {
            this.Register(type, (model, column, value) => controlType.InvokeMember("", BindingFlags.CreateInstance, null, null, null) as Control);
        }

        public void RegisterDefault(EditorCreatorDelegate creator)
        {
            this.defaultCreator = creator;
        }

        public void RegisterFirstChance(EditorCreatorDelegate creator)
        {
            this.firstChanceCreator = creator;
        }
    }
}

