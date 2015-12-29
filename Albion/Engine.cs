using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Albion
{
    public class Engine
    {
        internal static List<IInputConverter> Converters { get; set; }

        private Stack<SmartSentence> Sentences { get; set; }
        public string Language { get; set; }

        /// <summary>
        /// Initialize an Albion Engine, used to translate commands.
        /// </summary>
        public Engine(string Lang)
        {
            Sentences = new Stack<SmartSentence>();
            Language = Lang;
        }

        /// <summary>
        /// Initialize an Albion Engine, used to translate commands.
        /// Language used is 'en-US'.
        /// </summary>
        public Engine()
        {
            Sentences = new Stack<SmartSentence>();
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
                    Sentence sentence = null;
                    SentenceAttribute[] sas = (SentenceAttribute[])m.GetCustomAttributes(typeof(SentenceAttribute), true);

                    foreach (SentenceAttribute sa in sas)
                    {
						for (int i = 0; i < sa.Sentence.Length; i++)
						{
                            if (!Sentences.Any(x => x.Sentence == (sentence = new Sentence(m, sa, i))))
                            {
                                Sentences.Push(new SmartSentence(sentence));
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
                Sentence sentence = null;
                SentenceAttribute[] sas = (SentenceAttribute[])m.GetCustomAttributes(typeof(SentenceAttribute), true);

                foreach (SentenceAttribute sa in sas)
                {
					for (int i = 0; i < sa.Sentence.Length; i++)
					{
                        if (!Sentences.Any(x => x.Sentence == (sentence = new Sentence(m, sa, i))))
                        {
                            Sentences.Push(new SmartSentence(sentence));
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
		public List<Suggestion> Suggest(string input, string lang)
		{
			input = FilterForRegex(input);
			List<Suggestion> l = new List<Suggestion>();

			foreach (Sentence s in Sentences.Select(x => x.Sentence).Where(x => x.Language == lang))
			{
				Suggestion su = new Suggestion(input, s);
				if (su.Matched)
                    l.Add(su);
			}

			return (from i in l orderby i.Result[0].Length ascending orderby Regex.Matches(i.Sentence.Template, @"{[\w\d]+}").Count select i).ToList();
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

			if (string.IsNullOrWhiteSpace(input))
				return new Answer(new Exception("Empty input."));
			else if (input.Contains("{") || input.Contains("}"))
				return new Answer(new Exception("Unauthorized character(s): '{' and '}' are forbidden."));

            List<Answer> possibilities = new List<Answer>();

            Answer a = null;
            foreach (SmartSentence s in Sentences.Where(x => x.Sentence.Language == lang))
            {
                if (s.Sentence.Method.ReturnType == typeof(T) && !(a = GetAnswer(s, input)).Failed)
                {
                    return a;
                }
                else if (a != null)
                {
                    possibilities.Add(a);
                }
            }

            if (possibilities.Count == 0)
                return new Answer(new Exception("No match."));
            else
                return new Answer(new AggregateException(possibilities.Select(x => x.Error)));
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

            if (string.IsNullOrWhiteSpace(input))
                return new Answer(new Exception("Empty input."));
            else if (input.Contains("{") || input.Contains("}"))
                return new Answer(new Exception("Unauthorized character(s): '{' and '}' are forbidden."));

            List<Answer> possibilities = new List<Answer>();

            Answer a = null;
            foreach (SmartSentence s in Sentences.Where(x => x.Sentence.Language == lang))
            {
                if (!(a = GetAnswer(s, input)).Failed)
                {
                    return a;
                }
                else if (a != null)
                {
                    possibilities.Add(a);
                }
            }

            if (possibilities.Count == 0)
                return new Answer(new Exception("No match."));
            else
                return new Answer(new AggregateException(possibilities.Select(x => x.Error)));
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

        private Answer GetAnswer(SmartSentence ss, string input)
        {
            string originalInput = input;

            if (ss.VariablesCount == 0)
            {
                if (ss.Sentence.Template == input.ToLower())
                    return new Answer(ss.Sentence, new Dictionary<string, object>(0));
                else
                    return null;
            }
            
            Dictionary<string, object> vars = new Dictionary<string, object>();

            using (StringReader sr = new StringReader(input))
            {
                string var = "";
                char c;

                var tokens = ss.ToArray();

                int i = 0;
                while ((c = (char)sr.Read()) != char.MaxValue)
                {
                    if (tokens.Length == i)
                    {
                        return null;
                    }
                    else if (tokens[i].Type == SmartSentence.TokenType.String)
                    {
                        // we previously had found a variable, try to convert it
                        if (var != null)
                        {
                            var value = tokens[i - 1].Convert(var);

                            if (value != null)
                            {
                                vars.Add(tokens[i - 1].Value, value);
                                var = null;
                            }
                            else
                            {
                                return null;
                            }
                        }

                        // test if we have the same value
                        char[] buffer = new char[tokens[i].Value.Length];
                        if (sr.Read(buffer, 0, buffer.Length) == buffer.Length && new string(buffer) == tokens[i].Value)
                        {
                            i++;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (tokens[i].Type == SmartSentence.TokenType.Variable)
                    {
                        if (var == null) var = "";
                        var += c;

                        if (i + 1 < tokens.Length)
                        {
                            // check for next string
                            // TODO: check whether a constant is in fact a variable
                            //       ex: "Hello {someone} you" "Hello you you"

                            char[] buffer = new char[tokens[i + 1].Value.Length];

                            if (sr.ReadBlock(buffer, 0, buffer.Length) == buffer.Length && new string(buffer) == tokens[i + 1].Value)
                            {
                                i++;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                if (var != null)
                {
                    var value = tokens[i].Convert(var);
                    if (value != null)
                    {
                        vars.Add(tokens[i].Value, value);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            if (vars.Count == ss.VariablesCount)
            {
                return new Answer(ss.Sentence, vars);
            }
            else
            {
                return null;
            }
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
            return Format(template, this.Language);
        }

		/// <summary>
		/// Returns a list of all sentences, formatted using the given template.
		/// </summary>
		/// <param name="template">{0}: Template ; {1}: Return type ; {2}: Descr ; {3}: Params</param>
		/// <returns>Null if the template isn't in the right format. Else, an array.</returns>
		public string[] Format(string template, string lang)
		{
			List<string> results = new List<string> (Sentences.Count);

			foreach (Sentence s in Sentences.Select(x => x.Sentence).Where(x => x.Language == lang))
			{
				results.Add (string.Format (template,
					s.Template,
					s.Method.ReturnType.Name,
					s.Description,
					String.Join("|", s.Method.GetParameters().Select(x => "(" + x.ParameterType.Name + ") " + x.Name))));
			}

			return results.ToArray();
		}

        /// <summary>
        /// Register a converter that will convert a string to a type.
        /// </summary>
        public static void RegisterConverter<T>(IInputConverter converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }
            else if (converter.GetType().GetRuntimeMethod("", new Type[] { typeof(string) }).ReturnType == converter.Type)
            {
                Converters.Add(converter);
            }
            else
            {
                throw new InvalidCastException(string.Format("{0}.Convert(string) does not return {1}.", converter.GetType().Name, converter.Type.Name));
            }
        }
    }
}
