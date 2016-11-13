using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Albion.Parsers;
using System.Text.RegularExpressions;

namespace Albion
{
    /// <summary>
    /// Engine used by Albion.
    /// Provides methods to Suggest and Ask.
    /// </summary>
    public class Engine
    {
        private static List<IParser> _parsers;
        internal static IParser GetParserForParameter(Type target)
        {
            if (_parsers == null)
            {
                _parsers = new List<IParser>();

                var typeInfo = typeof(IParser).GetTypeInfo();

                foreach (TypeInfo t in target.GetTypeInfo().Assembly.DefinedTypes.Concat(typeInfo.Assembly.DefinedTypes))
                {
                    if (!t.IsAbstract
                        && t.ImplementedInterfaces.Contains(typeof(IParser))
                        && t.GetCustomAttribute<PhraseParserAttribute>() != null)
                    {
                        var constructor = t.DeclaredConstructors.FirstOrDefault(x => x.GetParameters().Length == 0);

                        if (constructor == null)
                        {
                            continue;
                        }
                        else if (!constructor.ContainsGenericParameters)
                        {
                            _parsers.Add((IParser)constructor.Invoke(new object[0]));
                        }
                    }
                }
            }

            target = target.FullName.StartsWith("System.Nullable`1") ? target.GenericTypeArguments[0] : target;
            return _parsers.FirstOrDefault(x => x.ParseTo == target);
        }

        private readonly Stack<SentenceParser> Sentences;
        private readonly Stack<object> Instances;

        /// <summary>
        /// Default Language used by <see cref="Ask(string)"/> and <see cref="Suggest(string)"/>.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="Engine"/>, with for <see cref="Language"/> "en".
        /// </summary>
        public Engine() : this("en") { }
        /// <summary>
        /// Create a new instance of <see cref="Engine"/>.
        /// </summary>
        /// <param name="lang">The value of the default <see cref="Language"/>.</param>
        public Engine(string lang)
        {
            Sentences = new Stack<SentenceParser>();
            Instances = new Stack<object>();
            Language = lang;
        }

        /// <summary>
        /// Initialize a new <see cref="SentenceBuilder"/>, used for generating Sentences on runtime.
        /// The newly createed <see cref="SentenceBuilder"/> uses the default <see cref="Language"/>.
        /// </summary>
        public SentenceBuilder Build()
        {
            return Build(Language);
        }

        /// <summary>
        /// Initialize a new <see cref="SentenceBuilder"/>, used for generating Sentences on runtime.
        /// </summary>
        public SentenceBuilder Build(string lang)
        {
            return new SentenceBuilder(this, lang);
        }

        /// <summary>
        /// Clear all registed Sentences.
        /// </summary>
        public void Clear()
        {
            Sentences.Clear();
            Instances.Clear();
        }

        internal void Register(SentenceParser parser)
        {
            Sentences.Push(parser);
        }

        internal object InstanceFor(Type t)
        {
            foreach (object instance in Instances)
            {
                if (instance.GetType() == t)
                    return instance;
            }
            return null;
        }

        /// <summary>
        /// Register and compile methods containing the <see cref="SentenceAttribute"/> attribute for future use,
        /// via <see cref="Ask(string)"/> and <see cref="Suggest(string)"/>.
        /// </summary>
        /// <param name="extensions">The <see cref="Type"/>s containing the methods</param>
        public void Register(params Type[] extensions)
        {
            foreach (Type ex in extensions)
            {
                foreach (MethodInfo method in ex.GetRuntimeMethods())
                {
                    var parsers = SentenceParser.Generate(method);

                    foreach (SentenceParser parser in parsers)
                        Sentences.Push(parser);
                }
            }
        }

        /// <summary>
        /// Register and compile methods containing the <see cref="SentenceAttribute"/> attribute for future use,
        /// via <see cref="Ask(string)"/> and <see cref="Suggest(string)"/>.
        /// </summary>
        public void Register<T>()
        {
            foreach (MethodInfo method in typeof(T).GetRuntimeMethods())
            {
                var parsers = SentenceParser.Generate(method);

                foreach (SentenceParser parser in parsers)
                    Sentences.Push(parser);
            }
        }

        /// <summary>
        /// Register and compile methods containing the <see cref="SentenceAttribute"/> attribute for future use,
        /// via <see cref="Ask(string)"/> and <see cref="Suggest(string)"/>.
        /// <para>
        /// Additionally, save this instance, so <see cref="Answer.Call(object)"/>
        /// automatically uses <paramref name="instance"/>.
        /// </para>
        /// </summary>
        public void Register<T>(T instance)
        {
            Register<T>();
            Instances.Push(instance);
        }

        #region Ask
        private IEnumerable<Answer<T>> AskInternal<T>(string cleaninput, string lang)
        {
            var sentences = new List<Tuple<SentenceParser, int, Dictionary<int, string>>>();

            foreach (SentenceParser sentence in (lang == "*") ? Sentences : Sentences.Where(x => x.SentenceLanguage == lang))
            {
                Dictionary<int, string> variables = new Dictionary<int, string>();
                int coeff = sentence.Parse(cleaninput, out variables);

                if (coeff >= 0)
                {
                    sentences.Add(new Tuple<SentenceParser, int, Dictionary<int, string>>(sentence, coeff, variables));
                }
            }

            foreach (var sentence in sentences.OrderByDescending(x => x.Item2))
            {
                Answer answer;
                if (sentence.Item1.TryFinaleParse(this, sentence.Item3, out answer))
                {
                    yield return new Answer<T>(answer);
                }
            }
        }

        private IEnumerable<Answer> AskInternal(string cleaninput, string lang)
        {
            var sentences = new List<Tuple<SentenceParser, int, Dictionary<int, string>>>();

            foreach (SentenceParser sentence in (lang == "*") ? Sentences : Sentences.Where(x => x.SentenceLanguage == lang))
            {
                Dictionary<int, string> variables = new Dictionary<int, string>();
                int coeff = sentence.Parse(cleaninput, out variables);

                if (coeff >= 0)
                {
                    sentences.Add(new Tuple<SentenceParser, int, Dictionary<int, string>>(sentence, coeff, variables));
                }
            }

            foreach (var sentence in sentences.OrderByDescending(x => x.Item2))
            {
                Answer answer;

                if (sentence.Item1.TryFinaleParse(this, sentence.Item3, out answer))
                {
                    yield return answer;
                }
            }
        }

        /// <summary>
        /// Ask the <see cref="Engine"/> for the most relevant <see cref="Answer"/>, with for language <see cref="Language"/>.
        /// </summary>
        /// <param name="input">A user input, that does not need to be sanitized.</param>
        /// <returns>An <see cref="Answer"/> if it was a success. Otherwise null.</returns>
        public Answer Ask(string input)
        {
            return Ask(input, Language);
        }

        /// <summary>
        /// Ask the <see cref="Engine"/> for the most relevant <see cref="Answer"/>.
        /// </summary>
        /// <param name="input">A user input, that does not need to be sanitized.</param>
        /// <param name="lang">Target language of the sentence</param>
        /// <returns>An <see cref="Answer"/> if it was a success. Otherwise null.</returns>
        public Answer Ask(string input, string lang)
        {
            PrepareString(ref input);
            return AskInternal(input, lang).FirstOrDefault();
        }

        /// <summary>
        /// Ask the <see cref="Engine"/> for the most relevant <see cref="Answer{T}"/>, with for language <see cref="Language"/>,
        /// and <see cref="Answer{T}.ReturnType"/> T.
        /// </summary>
        /// <param name="input">A user input, that does not need to be sanitized.</param>
        /// <returns>An <see cref="Answer{T}"/> if it was a success. Otherwise null.</returns>
        public Answer<T> Ask<T>(string input)
        {
            return Ask<T>(input, Language);
        }

        /// <summary>
        /// Ask the <see cref="Engine"/> for the most relevant <see cref="Answer{T}"/>,
        /// and <see cref="Answer{T}.ReturnType"/> T.
        /// </summary>
        /// <param name="input">A user input, that does not need to be sanitized.</param>
        /// <param name="lang">Target language of the sentence</param>
        /// <returns>An <see cref="Answer{T}"/> if it was a success. Otherwise null.</returns>
        public Answer<T> Ask<T>(string input, string lang)
        {
            PrepareString(ref input);
            return AskInternal<T>(input, lang).FirstOrDefault();
        }
        #endregion

        #region Suggest
        /// <summary>
        /// Perform a normal scan of sentences, with for language <see cref="Language"/>.
        /// </summary>
        /// <param name="s">User input for which suggestions shall be made</param>
        /// <returns><see cref="Suggestion"/>s, ordered by relevance to the given input</returns>
        public IEnumerable<Suggestion> Suggest(string s)
        {
            return Suggest(s, Language, SuggestionMatchType.Normal | SuggestionMatchType.Sentence);
        }

        /// <summary>
        /// Perform a normal scan of sentences.
        /// </summary>
        /// <param name="s">User input for which suggestions shall be made</param>
        /// <param name="lang">The language of the sentences to scan</param>
        /// <returns><see cref="Suggestion"/>s, ordered by relevance to the given input</returns>
        public IEnumerable<Suggestion> Suggest(string s, string lang)
        {
            return Suggest(s, lang, SuggestionMatchType.Normal | SuggestionMatchType.Sentence);
        }

        /// <summary>
        /// Perform a scan, with for language <see cref="Language"/>.
        /// </summary>
        /// <param name="s">User input for which suggestions shall be made</param>
        /// <param name="matchType">The kind of scan you wish to make.</param>
        /// <returns><see cref="Suggestion"/>s, ordered by relevance to the given input</returns>
        public IEnumerable<Suggestion> Suggest(string s, SuggestionMatchType matchType)
        {
            return Suggest(s, Language, matchType);
        }

        /// <summary>
        /// Perform a scan.
        /// </summary>
        /// <param name="s">User input for which suggestions shall be made</param>
        /// <param name="matchType">The kind of scan you wish to make.</param>
        /// <param name="lang">The language of the sentences to scan</param>
        /// <returns><see cref="Suggestion"/>s, ordered by relevance to the given input</returns>
        public IEnumerable<Suggestion> Suggest(string s, string lang, SuggestionMatchType matchType)
        {
            Dictionary<Suggestion, int> suggs = new Dictionary<Suggestion, int>();

            foreach (SentenceParser sentence in (lang == "*") ? Sentences : Sentences.Where(x => x.SentenceLanguage == lang))
            {
                Suggestion sugg;
                int coeff;

                if (matchType.HasFlag(SuggestionMatchType.Sentence)
                    && (coeff = sentence.Suggest(s, matchType.HasFlag(SuggestionMatchType.Deep), out sugg)) > 0)
                {
                    suggs.Add(sugg, coeff);
                }
                else if (matchType.HasFlag(SuggestionMatchType.Description)
                    && ((matchType.HasFlag(SuggestionMatchType.Deep) && sentence.SentenceDescription.Contains(s.ToLower()))
                    || (sentence.SentenceDescription.StartsWith(s.ToLower()))))
                {
                    suggs.Add(new Suggestion(sentence, s, SuggestionMatchType.Description), s.Length);
                }
                else if (matchType.HasFlag(SuggestionMatchType.ID)
                    && ((matchType.HasFlag(SuggestionMatchType.Deep) && sentence.SentenceID.Contains(s.ToLower()))
                    || (sentence.SentenceID.ToLower() == s.ToLower())))
                {
                    suggs.Add(new Suggestion(sentence, s, SuggestionMatchType.ID), s.Length * 10);
                }
            }

            return suggs.OrderByDescending(x => x.Value).Select(x => x.Key);
        }
        #endregion

        private static void PrepareString(ref string s)
        {
            s = Converters.StringToInt(Regex.Replace(s.Trim(), " +", " "));
        }
    }
}
