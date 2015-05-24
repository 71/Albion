using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Albion
{
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
    }

    public class Answer
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool Failed { get; set; }
        public Type Returns { get; set; }
        public Exception Error { get; set; }
        private MethodInfo Infos { get; set; }
        public string ExtensionID { get; set; }
        private Dictionary<string, string> Parameters { get; set; }

        public Answer(Exception err)
        {
            Failed = true;
            Error = err;
            Returns = null;
            Infos = null;
            Parameters = null;
            Name = null;
            ExtensionID = null;
            ID = null;
        }

        public Answer(MethodInfo info, Dictionary<string, string> pa, int i)
        {
            Returns = info.ReturnType;
            Infos = info;
            Parameters = pa;
            Name = info.Name;
            ExtensionID = (info.DeclaringType.GetCustomAttributes(typeof(ExtensionAttribute), false)[0] as ExtensionAttribute).ID;
            ID = (info.GetCustomAttributes(typeof(SentenceAttribute), false)[i] as SentenceAttribute).ID;
            Error = null;
            Failed = false;
        }

        /// <summary>
        /// Call the static method.
        /// </summary>
        /// <returns>What the method returns (this.Returns)</returns>
        public object Call()
        {
            List<dynamic> param = new List<dynamic>();
            foreach (var i in Infos.GetParameters())
            {
                bool isCustom = false;
                foreach (var o in i.GetCustomAttributes(typeof(AConvertAttribute), false))
                {
                    isCustom = true;
                    AConvertAttribute a = o as AConvertAttribute;
                    if (a.MethodName == "Auto")
                        param.Add(a.ConvertMethod.Invoke(null, new string[1] { Parameters[i.Name] }));
                    else if (a.ConvertMethod != null && a.ConvertMethod.ReturnType == i.ParameterType)
                        param.Add(a.ConvertMethod.Invoke(null, new string[1] { Parameters[i.Name] }));
                }
                if (!isCustom) param.Add(Parameters[i.Name]);
            }
            return Infos.Invoke(null, param.ToArray());
        }
    }
    
    public class Sentence
    {
        public Sentence(Type type, string full, string template, Dictionary<string, string> variables)
        {
            Full = full;
            Template = template;
            Variables = variables;
            Ty = type;
            Method = FindMethod();
        }

        public string Full { get; private set; }
        public string Template { get; private set; }
        public Dictionary<string, string> Variables { get; private set; }
        private Type Ty { get; set; }
        public MethodInfo Method { get; private set; }

        private MethodInfo FindMethod()
        {
            foreach (var i in Ty.GetMethods())
                foreach (var o in i.GetCustomAttributes(typeof(SentenceAttribute), false))
                    if ((o as SentenceAttribute).Sentence == Template)
                        return i;

            return null;
        }
    }
}
