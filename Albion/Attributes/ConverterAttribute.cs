using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Albion.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class ConverterAttribute : Attribute
    {
        public string ClassName { get; protected set; }
        public string MethodName { get; protected set; }
        public MethodInfo ConvertMethod { get; protected set; }
        public TypeInfo ConverterType { get; protected set; }
        public Type ReturnType { get; private set; }

        /// <summary>
        /// Tries to automatically convert the given value.
        /// If can't convert, returns null.
        /// </summary>
        public ConverterAttribute(string classname, string methodname)
        {
            ClassName = classname;
            MethodName = methodname;

            ConverterType = Type.GetType(classname, false)?.GetTypeInfo();

            if (ConverterType == null)
                throw new Exception("No corresponding method found.");

            ConvertMethod = ConverterType.DeclaredMethods.FirstOrDefault(x =>
                x.Name == MethodName
             && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(string));

            if (ConvertMethod == null)
                throw new Exception("No corresponding method found.");

            ReturnType = ConvertMethod.ReturnType;
        }
    }
}
