using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Albion
{
    public class A2
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool Failed { get; set; }
        public Type Returns { get; set; }
        public Exception Error { get; set; }
        private MethodInfo Infos { get; set; }
        public string ExtensionID { get; set; }
        private Dictionary<string, string> Parameters { get; set; }

        public A2(Exception err)
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

        public A2(MethodInfo info, Dictionary<string, string> pa, int i)
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

            if (Failed) throw Error;

            try
            {
                return Infos.Invoke(null, param.ToArray());
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }
    
    public class V2
    {
        private List<Type> Extensions { get; set; }

        /// <summary>
        /// Initialize an Albion Engine, used to translate commands.
        /// </summary>
        public V2()
        {
            Extensions = new List<Type>();
        }

        /// <summary>
        /// Add the specified extensions to this instance of Albion.
        /// </summary>
        /// <param name="ts">Types of classes (must have the Extension Attribute)</param>
        /// <returns>The number of added extensions</returns>
        public int Subscribe(params Type[] ts)
        {
            int extensions = Extensions.Count;
            foreach (Type t in ts)
            {
                if (t.GetCustomAttributes(typeof(ExtensionAttribute), false).Length > 1) throw new Exception("Only one Extension Attribute can be set.");
                else if (t.GetCustomAttributes(typeof(ExtensionAttribute), false).Length < 1) throw new Exception("An Extension Attribute must be set.");
                else if (!t.IsClass) throw new Exception("Only a class can be given.");
                else if (!Extensions.Contains(t)) Extensions.Add(t);
            }
            return Extensions.Count - extensions;
        }

        /// <summary>
        /// Translates a sentence to a method.
        /// </summary>
        /// <param name="input">The sentence entered by the user.</param>
        /// <returns>An answer containing either the method and informations, or an exception</returns>
        public A2 Ask(string input)
        {
            if (String.IsNullOrWhiteSpace(input)) return new A2(new Exception("Empty input."));
            else if (input.Contains("{") || input.Contains("}")) return new A2(new Exception("Unauthorized character(s): '{' and '}' are forbidden."));
            List<A2> Possibilities = new List<A2>();
            foreach (Type t in Extensions) Possibilities.Add(Ex(t, input));

            if (Possibilities.Count(x => x != null) == 0)
                return new A2(new Exception("No match."));
            else if (Possibilities.Where(x => x != null).Count(x => !x.Failed) == 0)
                return new A2(Possibilities.Where(x => x != null).FirstOrDefault().Error);
            else 
                return Possibilities.Where(x => x != null).FirstOrDefault(x => !x.Failed);
        }

        private A2 Ex(Type type, string input)
        {
            List<SentenceAttribute> p = (from m in type.GetMethods()
                              from a in m.GetCustomAttributes(typeof(SentenceAttribute), true)
                              where a != null && !String.IsNullOrWhiteSpace((a as SentenceAttribute).Sentence)
                              select a as SentenceAttribute)
                              .ToList<SentenceAttribute>();

            input = input.Trim('?', '!', ' ', ' ', '.', '\b', '\n', ',');

            int _test = 0;
            foreach (SentenceAttribute _s in p)
            {
                string s = _s.Sentence;
                MatchCollection one = Regex.Matches(s, @"{[\w\d]+}|([\w\s\d'-_]+)");

                string reg = "";
                foreach (Match m in one) if (!reg.EndsWith(m.Value) && !m.Value.EndsWith("}")) reg += m.Value + "|";
                reg = reg.Substring(0, reg.Length - 1);

                if (!s.Contains("{"))
                {
                    if (reg.ToLower().Trim() != input.ToLower().Trim()) continue;
                    else
                    {
                        Sentence esentence = new Sentence(type, input, s, new Dictionary<string, string>(0));
                        if (esentence.Method == null) continue;
                        else
                        {
                            int e__test = _test;
                            _test++;
                            Dictionary<string, string> vs = new Dictionary<string, string>(0);

                            return new A2(esentence.Method, vs, e__test);
                        }
                    };
                }

                if (Regex.Replace(input, ".*?" + reg.Replace("|", ".*?") + ".*", "").Trim() != "") continue;

                string[] matches = Regex.Replace(input, reg, "{").Split('{').Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => x).ToArray();

                Dictionary<string, string> dic = new Dictionary<string, string>(matches.Length);
                string[] _matches = Regex.Matches(s, @"{([\w\d:]+)}", RegexOptions.IgnoreCase).Cast<Match>().Select(x => x.Value.Substring(1, x.Value.Length - 2)).ToArray();
                if (_matches.Length != matches.Length || matches.Length != one.Cast<Match>().Count(x => x.Value.EndsWith("}"))) continue;

                for (int i = 0; i < matches.Length; i++)
                    dic.Add(_matches[i], matches[i]);

                Sentence sentence = new Sentence(type, input, s, dic);
                int __test = _test;
                _test++;

                var method = sentence.Method;
                if (method == null) continue;
                else
                {
                    if (method.GetParameters().Length != sentence.Variables.Count)
                        continue;
                    List<string> l = new List<string>();
                    Dictionary<string, string> vs = sentence.Variables.ToDictionary(x => Regex.Replace(x.Key, ":.+", ""), x => x.Value);

                    foreach (var pa in method.GetParameters())
                        if (vs.ContainsKey(pa.Name)) l.Add(vs[pa.Name]);
                        else continue;
                    if (l.Count != vs.Count) continue;
                    return new A2(method, vs, __test);
                }
            }

            return new A2(new Exception("'" + input + "' didn't match anything."));
        }
    }
}
