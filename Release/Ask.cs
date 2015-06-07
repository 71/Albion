using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Albion
{
    public class Engine
    {
        private Stack<Sentence> Sentences { get; set; }
        private string Language { get; set; }

        /// <summary>
        /// Initialize an Albion Engine, used to translate commands.
        /// </summary>
        public Engine(string Lang)
        {
            Sentences = new Stack<Sentence>();
            Language = Lang;
        }

        /// <summary>
        /// Initialize an Albion Engine, used to translate commands.
        /// </summary>
        public Engine()
        {
            Sentences = new Stack<Sentence>();
            Language = "en-US";
        }

        private string FilterForRegex(string s)
        {
            string _s = "";
            HashSet<char> filter = new HashSet<char>() { '.', '\\', '+', '-', '*', '?', '^', '$', '[', ']', '(', ')', '{', '}', '|', '/' };
            foreach (char i in s.ToCharArray()) 
            {
                if (filter.Contains(i)) _s += "\\";
                _s += i;
            }
            return _s;
        }

        /// <summary>
        /// Add the specified extensions to this instance of Albion.
        /// </summary>
        /// <param name="ts">Types of classes (must have the Extension Attribute)</param>
        /// <returns>The number of sentences added.</returns>
        public int Subscribe(params Type[] ts)
        {
            int began = Sentences.Count;
            foreach (Type t in ts)
            {
                if (t.GetCustomAttributes(typeof(ExtensionAttribute), false).Length > 1) throw new Exception("Only one Extension Attribute can be set.");
                else if (t.GetCustomAttributes(typeof(ExtensionAttribute), false).Length < 1) throw new Exception("An Extension Attribute must be set.");
                else if (!t.IsClass) throw new Exception("Only a class can be given.");

                foreach (var m in t.GetMethods())
                {
                    Sentence _s;
                    SentenceAttribute[] _as = (SentenceAttribute[])m.GetCustomAttributes(typeof(SentenceAttribute), true);
                    if (_as.Length > 0)
                        foreach (var a in _as)
                            if ((a.Language == Language) || (a.Language == "" && (t.GetCustomAttributes(typeof(ExtensionAttribute), true).First() as ExtensionAttribute).Language == Language))
                                for (int i = 0; i < a.Sentence.Length; i++)
                                    if (!Sentences.Contains(_s = new Sentence(m, a, i))) Sentences.Push(_s); else throw new Exception("The same sentence already exists in this Engine.");
                }
            }
            return Sentences.Count - began;
        }

        /// <summary>
        /// Add the specified extensions to this instance of Albion.
        /// </summary>
        /// <param name="ts">Types of classes (must have the Extension Attribute)</param>
        /// <returns>The number of sentences added.</returns>
        public int Subscribe<T>(T obj)
        {
            int began = Sentences.Count;
            Type t = typeof(T);
            if (t.GetCustomAttributes(typeof(ExtensionAttribute), true).Length > 1) throw new Exception("Only one Extension Attribute can be set.");
            else if (t.GetCustomAttributes(typeof(ExtensionAttribute), true).Length < 1) throw new Exception("An Extension Attribute must be set.");
            else if (!t.IsClass) throw new Exception("Only a class can be given.");

            foreach (var m in t.GetMethods())
            {
                Sentence _s;
                SentenceAttribute[] _as = (SentenceAttribute[])m.GetCustomAttributes(typeof(SentenceAttribute), true);
                if (_as.Length > 0)
                    foreach (var a in _as)
                        if ((a.Language == Language) || (a.Language == "" && (t.GetCustomAttributes(typeof(ExtensionAttribute), true).First() as ExtensionAttribute).Language == Language))
                            for (int i = 0; i < a.Sentence.Length; i++)
                                if (!Sentences.Contains(_s = new Sentence(m, a, i, obj))) Sentences.Push(_s); else throw new Exception("The same sentence already exists in this Engine.");
            }
            return Sentences.Count - began;
        }

        /// <summary>
        /// Returns an array of suggestions.
        /// </summary>
        /// <param name="s">An user input string</param>
        /// <returns>An array of suggestions.</returns>
        public List<Suggestion> Suggest(string s)
        {
            s = FilterForRegex(s);
            List<Suggestion> l = new List<Suggestion>();

            foreach (Sentence i in Sentences) 
            {
                Suggestion su = new Suggestion(s, i.Template, i.Attr.Description);
                if (su != null && su.Matched != null && su.Matched != "")
                    l.Add(su);
            }

            return (from i in l orderby i.Result[0].Length ascending orderby Regex.Matches(i.Sentence, @"{[\w\d]+}").Count select i).ToList();
        }


        /// <summary>
        /// Translates a sentence to a method.
        /// </summary>
        /// <param name="input">The sentence entered by the user.</param>
        /// <returns>An answer containing either the method and informations, or an exception</returns>
        public Answer Ask(string input)
        {
            input = FilterForRegex(input);
            if (String.IsNullOrWhiteSpace(input)) return new Answer(new Exception("Empty input."));
            else if (input.Contains("{") || input.Contains("}")) return new Answer(new Exception("Unauthorized character(s): '{' and '}' are forbidden."));
            List<Answer> Possibilities = new List<Answer>();
            Answer _a;
            foreach (Sentence s in Sentences)
                if ((_a = Ex(s, input)) != null && !_a.Failed) return _a;
                else Possibilities.Add(_a);

            if (Possibilities.Count(x => x != null) == 0) return new Answer(new Exception("No match."));
            else return new Answer(Possibilities.First().Error);
        }

        private Answer Ex(Sentence _s, string input)
        {
            string s = _s.Template.ToLower().Trim();
            if (!s.Contains("{")) 
            {
                if (s == input.ToLower()) return new Answer(_s, new Dictionary<string,string>(0));
                else return null;
            }
                
            MatchCollection one = Regex.Matches(s, @"{[\w\d]+}|([\w\s\d'-_]+)");

            string reg = "";
            foreach (Match m in one) if (!reg.EndsWith(m.Value) && !m.Value.EndsWith("}")) reg += m.Value + "|";
            reg = reg.Substring(0, reg.Length - 1);

            if (Regex.Replace(input, ".*?" + reg.Replace("|", ".*?") + ".*", "", RegexOptions.IgnoreCase).Trim() != "") return null;

            string r = Regex.Replace(input, reg, "{", RegexOptions.IgnoreCase);
            string[] matches = r.Split(new char[] { '{' }, StringSplitOptions.RemoveEmptyEntries);

            Dictionary<string, string> dic = new Dictionary<string, string>(matches.Length);
            string[] _matches = _s.Variables.ToArray();

            for (int i = 0; i < matches.Length; i++)
                dic.Add(_matches[i], matches[i]);

            return new Answer(_s, dic);
        }

        public void Reset()
        {
            Sentences = new Stack<Sentence>();
        }
    }
}
