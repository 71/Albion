using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Albion
{
    public class Answer
    {
        public string ID { get; set; }
        public bool Failed { get; set; }
        public Type Returns { get; set; }
        public Exception Error { get; set; }
        private MethodInfo Infos { get; set; }
        public string ExtensionID { get; set; }
        private Dictionary<string, string> Parameters { get; set; }
        private Sentence _s { get; set; }
        private object Base { get; set; }

        public Answer(Exception err)
        {
            Failed = true;
            Error = err;
            Returns = null;
            Infos = null;
            Parameters = null;
            ExtensionID = null;
            ID = null;
            _s = null;
            Base = null;
        }

        public Answer(Sentence s, Dictionary<string, string> pa)
        {
            Returns = s.Method.ReturnType;
            Infos = s.Method;
            Parameters = pa;
            ExtensionID = s.ExAttr.ID;
            ID = s.Attr.ID;
            Error = null;
            Failed = false;
            _s = s;
            Base = s.Base;
        }

        /// <summary>
        /// Call the static method.
        /// </summary>
        /// <returns>What the method returns (this.Returns)</returns>
        public object Call()
        {
            if (Failed) throw Error;

            List<dynamic> param = new List<dynamic>();
            foreach (var i in Infos.GetParameters())
            {
                ConverterAttribute a = (ConverterAttribute)i.GetCustomAttributes(typeof(ConverterAttribute), true).FirstOrDefault();

                if (a == null && i.ParameterType == typeof(Sentence)) param.Add(_s);
                else if (a == null) param.Add(Parameters[i.Name]);
                else if (a.MethodName == "Auto")
                    param.Add(a.ConvertMethod.Invoke(null, new string[1] { Parameters[i.Name] }));
                else if (a.ConvertMethod != null && a.ConvertMethod.ReturnType == i.ParameterType)
                    param.Add(a.ConvertMethod.Invoke(null, new string[1] { Parameters[i.Name] }));
            }

            try
            {
                return Infos.Invoke(Base, param.ToArray());
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }

    public class Suggestion
    {
        public Suggestion(string input, string tested, string descr)
        {
            if (tested.Contains("{") && tested.Contains("}") && input.IndexOf(tested.Substring(0, tested.IndexOf('{'))) >= 0)
            {
                string i = "(" + Regex.Replace(tested.ToLower(), @"{[\d\w]+}", "|").Trim('|') + @")|([^\1])";
                input = Regex.Replace(input, i, "$1|", RegexOptions.IgnoreCase);
                input = Regex.Replace(input, @"\|+", "\\?");
                if (input == "\\?" || input == "") return;
            }
            tested = Regex.Replace(tested, @"{[\d\w]+}", "?");
            var r = Regex.Match(tested, "(.*?)(" + input + ")(.*)", RegexOptions.IgnoreCase);
            Full = r.Groups[1].Value + r.Groups[2].Value + r.Groups[3].Value;
            Result = new string[] { r.Groups[1].Value, r.Groups[2].Value, r.Groups[3].Value };
            Matched = r.Groups[2].Value;
            Input = input;
            Description = descr;
            Sentence = tested;
        }
        
        public string Full { get; set; }
        public string[] Result { get; set; }
        public string Matched { get; set; }
        public string Input { get; set; }
        public string Description { get; set; }
        public string Sentence { get; set; }

        public override string ToString()
        {
            return this.Full;
        }
    }

    public class Sentence
    {
        public Sentence(MethodInfo m, SentenceAttribute a, int i)
        {
            Template = a.Sentence[i];
            Method = m;
            Attr = a;
            ExAttr = (ExtensionAttribute)m.DeclaringType.GetCustomAttributes(typeof(ExtensionAttribute), true).First();
            Variables = Regex.Matches(Template, @"{[\d\w]+}").Cast<Match>().Select(x => x.Value.Substring(1, x.Value.Length - 2));
            Base = null;
        }

        public Sentence(MethodInfo m, SentenceAttribute a, int i, object b)
        {
            Template = a.Sentence[i];
            Method = m;
            Attr = a;
            ExAttr = (ExtensionAttribute)m.DeclaringType.GetCustomAttributes(typeof(ExtensionAttribute), true).First();
            Variables = Regex.Matches(Template, @"{[\d\w]+}").Cast<Match>().Select(x => x.Value.Substring(1, x.Value.Length - 2));
            Base = b;
        }

        public string Template { get; private set; }
        public MethodInfo Method { get; private set; }
        public SentenceAttribute Attr { get; private set; }
        public ExtensionAttribute ExAttr { get; private set; }
        public IEnumerable<string> Variables { get; private set; }
        public object Base { get; private set; }

        public override string ToString()
        {
            return this.Template;
        }
    }



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
            ConvertMethod = typeof(Converters).GetMethod("Auto");
        }

        public ConverterAttribute(string methodname)
        {
            MethodName = methodname;
            ConvertMethod = Converter.GetMethods().First(x => x.Name == MethodName && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(string));
            if (ConvertMethod == null) throw new Exception("No corresponding method found.");
        }

        public string MethodName { get; protected set; }

        protected Type converters = typeof(Converters);
        public Type Converter { get { return converters; } set { converters = value; } }

        public MethodInfo ConvertMethod { get; private set; }
    }
}
