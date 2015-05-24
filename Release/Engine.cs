using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Albion
{
    public class Engine
    {
        private List<Type> Extensions { get; set; }

        /// <summary>
        /// Initialize an Albion Engine, used to translate commands.
        /// </summary>
        public Engine()
        {
            Extensions = new List<Type>();
        }

        /// <summary>
        /// Add the specified extensions to this instance of Albion.
        /// </summary>
        /// <param name="ts">Types of classes (must have the Extension Attribute)</param>
        /// <returns>The number of added extensions</returns>
        public int AddExtensions(params Type[] ts)
        {
            int extensions = Extensions.Count;
            foreach (Type t in ts)
                if (t.GetCustomAttributes(typeof(ExtensionAttribute), false).Length == 1 && t.IsClass) Extensions.Add(t);
            return Extensions.Count - extensions;
        }

        public List<Suggestion> Suggest(string s)
        {
            List<Suggestion> l = new List<Suggestion>();
            foreach (var ex in Extensions)
                foreach (var m in ex.GetMethods())
                    foreach (var a in m.GetCustomAttributes(typeof(SentenceAttribute), false))
                        if (a != null && !String.IsNullOrWhiteSpace((a as SentenceAttribute).Sentence))
                        {
                            Suggestion suggestion = new Suggestion(s, (a as SentenceAttribute).Sentence, (a as SentenceAttribute).Description);
                            if (suggestion != null && suggestion.Matched != null && suggestion.Matched != "")
                                l.Add(suggestion);
                        }

            return (from i in l orderby i.Result[0].Length ascending orderby Regex.Matches(i.Sentence, @"{[\w\d]+}").Count select i).ToList();
        }

        /// <summary>
        /// Translates a sentence to a method.
        /// </summary>
        /// <param name="input">The sentence entered by the user.</param>
        /// <returns>Nothing yet</returns>
        public Answer Ex(string input)
        {
            if (String.IsNullOrWhiteSpace(input)) return new Answer(new Exception("The input mustn't be empty."));
            List<Answer> Possibilities = new List<Answer>();
            foreach (Type t in Extensions)
                Possibilities.Add(_Ex(t, input));
            return Possibilities.Where(x => x != null).FirstOrDefault(x => !x.Failed);
        }

        private Answer _Ex(Type type, string input)
        {
            List<string> p = (from m in type.GetMethods()
                              from a in m.GetCustomAttributes(typeof(SentenceAttribute), false)
                              where a != null && !String.IsNullOrWhiteSpace((a as SentenceAttribute).Sentence)
                              select (a as SentenceAttribute).Sentence)
                              .ToList<string>();

            input = input.Trim();

            int _test = 0;
            foreach (string s in p)
            {
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
                        var emethod = esentence.Method;
                        if (emethod == null) continue;
                        else
                        {
                            int e__test = _test;
                            _test++;
                            Dictionary<string, string> vs = new Dictionary<string, string>(0);

                            return new Answer(emethod, vs, e__test);
                        }
                    };
                }
                if (Regex.Replace(input, ".*?" + reg.Replace("|", ".*?") + ".*", "").Trim() != "")
                    continue;

                string r = Regex.Replace(input, reg, "{");
                string[] matches = r.Split('{');
                matches = matches.Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => x).ToArray();

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
                    return new Answer(method, vs, __test);
                }
            }

            return null;
        }
    }

}
