using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Albion.Parsers;
using System.Text.RegularExpressions;

namespace Albion
{
    public class Engine
    {
        private static List<IParser> _parsers;
        internal static IParser GetParserForParameter(Type target)
        {
            if (_parsers == null)
            {
                _parsers = new List<IParser>();

                var typeInfo = typeof(IParser).GetTypeInfo();

                foreach (TypeInfo t in typeInfo.Assembly.DefinedTypes)
                {
                    if (!t.IsAbstract
                        && t.ImplementedInterfaces.Contains(typeof(IParser))
                        && t.GetCustomAttribute<Attributes.ParserAttribute>() != null)
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

        private Stack<SentenceParser> Sentences { get; set; }

        public string Language { get; set; }

        public Engine() : this("en") { }
        public Engine(string lang)
        {
            Sentences = new Stack<SentenceParser>();
            Language = lang;
        }

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

        private IEnumerable<Answer<T>> AskInternal<T>(string cleaninput, string lang)
        {
            var sentences = new List<Tuple<SentenceParser, int, Dictionary<int, string>>>();

            foreach (SentenceParser sentence in Sentences.Where(x => x.Attribute.Language == lang))
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
                if (sentence.Item1.TryFinaleParse(sentence.Item3, out answer))
                {
                    yield return new Answer<T>(answer);
                }
            }
        }

        private IEnumerable<Answer> AskInternal(string cleaninput, string lang)
        {
            var sentences = new List<Tuple<SentenceParser, int, Dictionary<int, string>>>();

            foreach (SentenceParser sentence in Sentences.Where(x => x.Attribute.Language == lang))
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

                if (sentence.Item1.TryFinaleParse(sentence.Item3, out answer))
                {
                    yield return answer;
                }
            }
        }

        public Answer Ask(string input)
        {
            return Ask(input, Language);
        }

        public Answer Ask(string input, string lang)
        {
            PrepareString(ref input);
            return AskInternal(input, lang).FirstOrDefault();
        }

        public Answer<T> Ask<T>(string input)
        {
            return Ask<T>(input, Language);
        }

        public Answer<T> Ask<T>(string input, string lang)
        {
            PrepareString(ref input);
            return AskInternal<T>(input, lang).FirstOrDefault();
        }

        public IEnumerable<Suggestion> Suggest(string s)
        {
            return Suggest(s, Language, SuggestionMatchType.Normal | SuggestionMatchType.Sentence);
        }

        public IEnumerable<Suggestion> Suggest(string s, string lang)
        {
            return Suggest(s, lang, SuggestionMatchType.Normal | SuggestionMatchType.Sentence);
        }

        public IEnumerable<Suggestion> Suggest(string s, SuggestionMatchType matchType)
        {
            return Suggest(s, Language, matchType);
        }

        public IEnumerable<Suggestion> Suggest(string s, string lang, SuggestionMatchType matchType)
        {
            Dictionary<Suggestion, int> suggs = new Dictionary<Suggestion, int>();

            foreach (SentenceParser sentence in Sentences.Where(x => x.Attribute.Language == lang))
            {
                Suggestion sugg;
                int coeff;

                if (matchType.HasFlag(SuggestionMatchType.Sentence)
                    && (coeff = sentence.Suggest(s, matchType.HasFlag(SuggestionMatchType.Deep), out sugg)) > 0)
                {
                    suggs.Add(sugg, coeff * 100);
                }
                else if (matchType.HasFlag(SuggestionMatchType.Description)
                    && ((matchType.HasFlag(SuggestionMatchType.Deep) && sentence.Attribute.Description.Contains(s.ToLower()))
                    || (sentence.Attribute.Description.StartsWith(s.ToLower()))))
                {
                    suggs.Add(new Suggestion(sentence, s, SuggestionMatchType.Description), s.Length);
                }
                else if (matchType.HasFlag(SuggestionMatchType.ID)
                    && ((matchType.HasFlag(SuggestionMatchType.Deep) && sentence.Attribute.ID.Contains(s.ToLower()))
                    || (sentence.Attribute.ID.ToLower() == s.ToLower())))
                {
                    suggs.Add(new Suggestion(sentence, s, SuggestionMatchType.ID), s.Length * 10);
                }
            }

            return suggs.OrderByDescending(x => x.Value).Select(x => x.Key);
        }
        
        private static void PrepareString(ref string s)
        {
            s = Converters.StringToInt(Regex.Replace(s.Trim(), " +", " "));
        }
    }
}
