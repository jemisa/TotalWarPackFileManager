namespace BrightIdeasSoftware
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;

    public class TypedColumn<T> where T: class
    {
        private TypedAspectGetterDelegate<T> aspectGetter;
        private TypedAspectPutterDelegate<T> aspectPutter;
        private OLVColumn column;
        private TypedGroupKeyGetterDelegate<T> groupKeyGetter;
        private TypedImageGetterDelegate<T> imageGetter;

        public TypedColumn(OLVColumn column)
        {
            this.column = column;
        }

        public void GenerateAspectGetter()
        {
            if (!string.IsNullOrEmpty(this.column.AspectName))
            {
                this.AspectGetter = this.GenerateAspectGetter(typeof(T), this.column.AspectName);
            }
        }

        private TypedAspectGetterDelegate<T> GenerateAspectGetter(Type type, string path)
        {
            DynamicMethod method = new DynamicMethod(string.Empty, typeof(object), new Type[] { type }, type, true);
            this.GenerateIL(type, path, method.GetILGenerator());
            return (TypedAspectGetterDelegate<T>) method.CreateDelegate(typeof(TypedAspectGetterDelegate<T>));
        }

        private void GenerateIL(Type type, string path, ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            string[] strArray = path.Split(new char[] { '.' });
            for (int i = 0; i < strArray.Length; i++)
            {
                type = this.GeneratePart(il, type, strArray[i], i == (strArray.Length - 1));
                if (type == null)
                {
                    break;
                }
            }
            if (!(((type == null) || !type.IsValueType) || typeof(T).IsValueType))
            {
                il.Emit(OpCodes.Box, type);
            }
            il.Emit(OpCodes.Ret);
        }

        private Type GeneratePart(ILGenerator il, Type type, string pathPart, bool isLastPart)
        {
            MemberInfo info = new List<MemberInfo>(type.GetMember(pathPart)).Find(x => ((x.MemberType == MemberTypes.Field) || (x.MemberType == MemberTypes.Property)) || ((x.MemberType == MemberTypes.Method) && (((MethodInfo) x).GetParameters().Length == 0)));
            if (info == null)
            {
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Ldstr, string.Format("'{0}' is not a parameter-less method, property or field of type '{1}'", pathPart, type.FullName));
                return null;
            }
            Type localType = null;
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                {
                    FieldInfo field = (FieldInfo) info;
                    il.Emit(OpCodes.Ldfld, field);
                    localType = field.FieldType;
                    goto Label_0112;
                }
                case MemberTypes.Method:
                {
                    MethodInfo meth = (MethodInfo) info;
                    if (meth.IsVirtual)
                    {
                        il.Emit(OpCodes.Callvirt, meth);
                    }
                    else
                    {
                        il.Emit(OpCodes.Call, meth);
                    }
                    localType = meth.ReturnType;
                    break;
                }
                case MemberTypes.Property:
                {
                    PropertyInfo info3 = (PropertyInfo) info;
                    il.Emit(OpCodes.Call, info3.GetGetMethod());
                    localType = info3.PropertyType;
                    goto Label_0112;
                }
            }
        Label_0112:
            if (!(!localType.IsValueType || isLastPart))
            {
                LocalBuilder local = il.DeclareLocal(localType);
                il.Emit(OpCodes.Stloc, local);
                il.Emit(OpCodes.Ldloca, local);
            }
            return localType;
        }

        public TypedAspectGetterDelegate<T> AspectGetter
        {
            get
            {
                return this.aspectGetter;
            }
            set
            {
                this.aspectGetter = value;
                if (value == null)
                {
                    this.column.AspectGetter = null;
                }
                else
                {
                    this.column.AspectGetter = x => base.aspectGetter((T) x);
                }
            }
        }

        public TypedAspectPutterDelegate<T> AspectPutter
        {
            get
            {
                return this.aspectPutter;
            }
            set
            {
                this.aspectPutter = value;
                if (value == null)
                {
                    this.column.AspectPutter = null;
                }
                else
                {
                    this.column.AspectPutter = delegate (object x, object newValue) {
                        base.aspectPutter((T) x, newValue);
                    };
                }
            }
        }

        public TypedGroupKeyGetterDelegate<T> GroupKeyGetter
        {
            get
            {
                return this.groupKeyGetter;
            }
            set
            {
                this.groupKeyGetter = value;
                if (value == null)
                {
                    this.column.GroupKeyGetter = null;
                }
                else
                {
                    this.column.GroupKeyGetter = x => base.groupKeyGetter((T) x);
                }
            }
        }

        public TypedImageGetterDelegate<T> ImageGetter
        {
            get
            {
                return this.imageGetter;
            }
            set
            {
                this.imageGetter = value;
                if (value == null)
                {
                    this.column.ImageGetter = null;
                }
                else
                {
                    this.column.ImageGetter = x => base.imageGetter((T) x);
                }
            }
        }

        public delegate object TypedAspectGetterDelegate(T rowObject);

        public delegate void TypedAspectPutterDelegate(T rowObject, object newValue);

        public delegate object TypedGroupKeyGetterDelegate(T rowObject);

        public delegate object TypedImageGetterDelegate(T rowObject);
    }
}

