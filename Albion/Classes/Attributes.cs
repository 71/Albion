using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Albion
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class SentenceAttribute : Attribute
    {
        public SentenceAttribute(params string[] FormattedSentence)
        {
            this.sentence = FormattedSentence;
        }

        protected string[] sentence;
        public string[] Sentence { get { return this.sentence; } }

        protected string id = "";
        public string ID { get { return this.id; } set { this.id = value; } }

        protected string descr = "";
        public string Description { get { return this.descr; } set { this.descr = value; } }

        protected string lang = "";
        public string Language { get { return this.lang; } set { this.lang = value; } }
    }

    /// <summary>
    /// The ID corresponds to an identifier used to call results
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ExtensionAttribute : Attribute
    {
        public ExtensionAttribute() { }

        protected string id = "";
        public string ID { get { return this.id; } set { this.id = value; } }

        protected string lang = "en-US";
        public string Language { get { return this.lang; } set { this.lang = value; } }
    }

    /// <summary>
    /// Convert the string value using the specified converter.
    /// The converter must accept a string and should return the parameter type
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class ConverterAttribute : Attribute
    {
        /// <summary>
        /// Tries to automatically convert the given value.
        /// If can't convert, returns null.
        /// Currently supported: int, double
        /// </summary>
        public ConverterAttribute()
        {
            MethodName = "Auto";
            ConvertMethod = typeof(Converters).GetTypeInfo().GetDeclaredMethod("Auto");
        }

        public ConverterAttribute(string methodname)
        {
            MethodName = methodname;
            ConvertMethod = Converter.GetTypeInfo().DeclaredMethods.First(x => x.Name == MethodName && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(string));
            if (ConvertMethod == null) 
               throw new Exception("No corresponding method found.");
        }

        public string MethodName { get; protected set; }

        protected Type converters = typeof(Converters);
        public Type Converter { get { return converters; } set { converters = value; } }

        public MethodInfo ConvertMethod { get; private set; }
    }
}
