using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Albion
{
    public class Engine
    {
        private Stack<Sentence> Sentences { get; set; }
        public string Language { get; set; }

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
        /// Language used is 'en-US'.
        /// </summary>
        public Engine()
        {
            Sentences = new Stack<Sentence>();
            Language = "en-US";
        }

        private static string FilterForRegex(string s)
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
                TypeInfo info = t.GetTypeInfo();
                ExtensionAttribute attr = info.GetCustomAttribute<ExtensionAttribute>();

                if (attr == null)
                    throw new ArgumentNullException("ts", "An Extension Attribute must be set.");

                foreach (var m in info.DeclaredMethods.Where(x => x.IsStatic == true))
                {
                    Sentence sentence;
                    SentenceAttribute[] sas = (SentenceAttribute[])m.GetCustomAttributes(typeof(SentenceAttribute), true);

                    foreach (SentenceAttribute sa in sas)
                    {
						for (int i = 0; i < sa.Sentence.Length; i++)
						{
							if (!Sentences.Contains(sentence = new Sentence(m, sa, i)))
							{
								Sentences.Push(sentence);
							}
						}
                    }
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
            TypeInfo info = t.GetTypeInfo();
            ExtensionAttribute attr = info.GetCustomAttribute<ExtensionAttribute>();

            if (attr == null)
                throw new ArgumentNullException("T", "An Extension Attribute must be set.");

            foreach (var m in info.DeclaredMethods)
            {
                Sentence sentence;
                SentenceAttribute[] sas = (SentenceAttribute[])m.GetCustomAttributes(typeof(SentenceAttribute), true);

                foreach (SentenceAttribute sa in sas)
                {
					for (int i = 0; i < sa.Sentence.Length; i++)
					{
						if (!Sentences.Contains(sentence = new Sentence(m, sa, i, obj)))
						{
							Sentences.Push(sentence);
						}
					}
                }
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
			return this.Suggest (s, this.Language);
        }

		/// <summary>
		/// Returns an array of suggestions.
		/// </summary>
		/// <param name="s">An user input string</param>
		/// <returns>An array of suggestions.</returns>
		public List<Suggestion> Suggest(string s, string lang)
		{
			s = FilterForRegex(s);
			List<Suggestion> l = new List<Suggestion>();

			foreach (Sentence i in Sentences.Where(x => x.Attr.Language == lang || (x.Attr.Language == "" && x.ExAttr.Language == lang))) 
			{
				Suggestion su = new Suggestion(s, i);
				if (su != null && su.Matched != null && su.Matched != "")
					l.Add(su);
			}

			return (from i in l orderby i.Result[0].Length ascending orderby Regex.Matches(i.Sentence, @"{[\w\d]+}").Count select i).ToList();
		}

		/// <summary>
		/// Translates a sentence to a method, returning only if the method has for return type T.
		/// </summary>
		/// <param name="input">The sentence entered by the user.</param>
		/// <returns>An answer containing either the method and informations, or an exception</returns>
		public Answer Ask<T>(string input)
		{
			return Ask<T> (input, this.Language);
		}

		/// <summary>
		/// Translates a sentence to a method, returning only if the method has for return type T.
		/// </summary>
		/// <param name="input">The sentence entered by the user.</param>
		/// <param name="lang">The language of the sentence.</param>
		/// <returns>An answer containing either the method and informations, or an exception</returns>
		public Answer Ask<T>(string input, string lang)
		{
			input = FilterForRegex(input);

			if (String.IsNullOrWhiteSpace(input))
				return new Answer(new Exception("Empty input."));
			else if (input.Contains("{") || input.Contains("}"))
				return new Answer(new Exception("Unauthorized character(s): '{' and '}' are forbidden."));

			List<Answer> Possibilities = new List<Answer>();
			Answer _a;
			foreach (Sentence s in Sentences.Where(x => x.Attr.Language == lang || (x.Attr.Language == "" && x.ExAttr.Language == lang)))
				if ((_a = Ex(s, input)) != null && _a.Returns == typeof(T) && !_a.Failed) return _a;
				else Possibilities.Add(_a);

			if (Possibilities.Count(x => x != null) == 0)
				return new Answer(new Exception("No match."));
			else
				return new Answer(new AggregateException(Possibilities.Select(x => x.Error)));
		}

		/// <summary>
		/// Translates a sentence to a method.
		/// </summary>
		/// <param name="input">The sentence entered by the user.</param>
		/// <param name="lang">The language of the sentence.</param>
		/// <returns>An answer containing either the method and informations, or an exception</returns>
		public Answer Ask(string input, string lang)
		{
			input = FilterForRegex(input);

			if (String.IsNullOrWhiteSpace(input))
				return new Answer(new Exception("Empty input."));
			else if (input.Contains("{") || input.Contains("}"))
				return new Answer(new Exception("Unauthorized character(s): '{' and '}' are forbidden."));

			List<Answer> Possibilities = new List<Answer>();
			Answer _a;
			foreach (Sentence s in Sentences.Where(x => x.Attr.Language == lang || (x.Attr.Language == "" && x.ExAttr.Language == lang)))
				if ((_a = Ex(s, input)) != null && !_a.Failed) return _a;
				else if (_a != null) Possibilities.Add(_a);

			if (Possibilities.Count == 0)
				return new Answer(new Exception("No match."));
			else
				return new Answer(new AggregateException(Possibilities.Select(x => x.Error)));
		}

        /// <summary>
        /// Translates a sentence to a method.
        /// </summary>
        /// <param name="input">The sentence entered by the user.</param>
        /// <returns>An answer containing either the method and informations, or an exception</returns>
        public Answer Ask(string input)
        {
			return Ask (input, this.Language);
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

        /// <summary>
        /// Keeps the language, but removes all extensions.
        /// </summary>
        public void Clear()
        {
            Sentences.Clear();
        }

        /// <summary>
        /// Returns a list of all sentences, formatted using the given template.
        /// </summary>
		/// <param name="template">{0}: Template ; {1}: Return type ; {2}: ID ; {3}: Descr ; {4}: Params</param>
        /// <returns>Null if the template isn't in the right format. Else, an array.</returns>
        public string[] Format(string template)
        {
			List<string> results = new List<string> (Sentences.Count);

			foreach (Sentence s in Sentences)
			{
				results.Add (String.Format (template,
					s.Template,
					s.Method.ReturnType.Name,
					s.Attr.ID,
					s.Attr.Description,
					String.Join("|", s.Method.GetParameters().Select<System.Reflection.ParameterInfo, string>(x => "(" + x.ParameterType.Name + ") " + x.Name))));
			}

			return results.ToArray();
        }

		/// <summary>
		/// Returns a list of all sentences, formatted using the given template.
		/// </summary>
		/// <param name="template">{0}: Template ; {1}: Return type ; {2}: ID ; {3}: Descr ; {4}: Params</param>
		/// <returns>Null if the template isn't in the right format. Else, an array.</returns>
		public string[] FormatForLanguage(string template, string lang)
		{
			List<string> results = new List<string> (Sentences.Count);

			foreach (Sentence s in Sentences.Where(x => x.Attr.Language == lang || (x.Attr.Language == "" && x.ExAttr.Language == lang)))
			{
				results.Add (String.Format (template,
					s.Template,
					s.Method.ReturnType.Name,
					s.Attr.ID,
					s.Attr.Description,
					String.Join("|", s.Method.GetParameters().Select<System.Reflection.ParameterInfo, string>(x => "(" + x.ParameterType.Name + ") " + x.Name))));
			}

			return results.ToArray();
		}
    }
}
