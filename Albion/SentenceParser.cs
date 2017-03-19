using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Albion.Parsers
{
    internal class SentenceParser
    {
        private static readonly Random random = new Random();
        private static string AnyOf(string[] strs) => strs[random.Next(strs.Length)];

        private readonly bool m_built;
        private readonly Action<dynamic> m_action;
        private readonly Func<dynamic, object> m_func;

        public Type ReturnType { get; private set; }
        public ParameterInfo[] Parameters { get; private set; }
        public List<IParser> Tokens { get; private set; }
        public List<string> ParametersName { get; private set; }
        public List<string[]> CustomSuggestions { get; private set; }
        public MethodInfo Method { get; private set; }

        public string SentenceDescription { get; private set; }
        public string SentenceID { get; private set; }
        public string SentenceLanguage { get; private set; }
        public string[] Templates { get; private set; }

        public string Full
        {
            get
            {
                if (CustomSuggestions == null || CustomSuggestions.Count == 0)
                    return AnyOf(Templates);
                return string.Join("", CustomSuggestions.Select(AnyOf));
            }
        }

        internal SentenceParser(List<IParser> tokens, List<string> names, Func<dynamic, object> deleg, List<string[]> suggestions, string sentence, string lang, string descr, string id)
        {
            var info = deleg.GetMethodInfo();

            SentenceLanguage = lang;
            SentenceDescription = descr;
            SentenceID = id;
            Templates = new[] { sentence };

            Method = info;
            ParametersName = names;
            Parameters = info.GetParameters();
            Tokens = tokens;
            ReturnType = info.ReturnType;
            CustomSuggestions = suggestions;
            m_built = true;
            m_func = deleg;

            Verify();
        }

        internal SentenceParser(List<IParser> tokens, List<string> names, Action<dynamic> deleg, List<string[]> suggestions, string sentence, string lang, string descr, string id)
        {
            var info = deleg.GetMethodInfo();

            SentenceLanguage = lang;
            SentenceDescription = descr;
            SentenceID = id;
            Templates = new[] { sentence };

            Method = info;
            ParametersName = names;
            Parameters = info.GetParameters();
            Tokens = tokens;
            ReturnType = info.ReturnType;
            CustomSuggestions = suggestions;
            m_built = true;
            m_action = deleg;

            Verify();
        }

        private SentenceParser(List<IParser> tokens, List<string> names, MethodInfo info, SentenceAttribute attr, List<string[]> suggestions)
        {
            SentenceLanguage = attr.Language;
            SentenceDescription = attr.Description;
            SentenceID = attr.ID;
            Templates = attr.Sentences;

            Method = info;
            ParametersName = names;
            Parameters = info.GetParameters();
            Tokens = tokens;
            ReturnType = info.ReturnType;
            CustomSuggestions = suggestions;
            m_built = false;

            Verify();
        }

        private void Verify()
        {
            // This method makes sure we have no chained variables
            StaticStringParser previous = Tokens[0] as StaticStringParser;

            for (int i = 1; i < Tokens.Count; i++)
            {
                if (previous != null)
                    previous = null;
                else if ((previous = Tokens[i] as StaticStringParser) == null)
                    throw new NotSupportedException("Chained variables are not supported.");
            }
        }

        internal int Suggest(string input, bool deep, out Suggestion sugg)
        {
            sugg = null;

            int coeff = 0;
            string test = input.ToLower();
            
            if (!deep)
            {
                IParser token = Tokens.FirstOrDefault();

                if (token is StaticStringParser stringParser && stringParser.Reference.StartsWith(test))
                {
                    string a = stringParser.Reference.Substring(test.Length);

                    foreach (IParser parser in Tokens.Skip(1))
                        a += parser.RandomExample() + ' ';

                    sugg = new Suggestion(this, "", a, input);
                    return input.Length * 100;
                }
            }

            byte op = 0;
            string before = "";
            string after = "";

            for (int i = 0; i < Tokens.Count; i++)
            {
                IParser token = Tokens[i];

                if (token is StaticStringParser)
                {
                    if (op < 2)
                    {
                        int del = (token as StaticStringParser).MatchLength(test);

                        if (op == 0) // before
                        {
                            if (del > 0)
                            {
                                op = 1;
                                test = test.Substring(del);
                            }
                            else if (!deep)
                            {
                                return -1;
                            }
                            else
                            {
                                before += (token as StaticStringParser).Reference;
                            }
                        }
                        else // current
                        {
                            if (del == 0)
                            {
                                op = 2;
                            }
                            else
                            {
                                test = test.Substring((token as StaticStringParser).Reference.Length);
                            }
                        }
                    }
                    else // after
                    {
                        after += (token as StaticStringParser).Reference;
                    }
                }
                else
                {
                    if (op < 2)
                    {
                        object o;

                        if (op == 0) // before
                        {
                            if (Tokens.Count == i + 1) // last token
                            {
                                bool ok = token.TryParse(test, out o);

                                if (ok)
                                {
                                    coeff += test.Length * token.Coeff;
                                    test = "";
                                }
                                else
                                {
                                    return -1;
                                }
                            }
                            else if (Tokens[i + 1] is StaticStringParser) // following token is a static string
                            {
                                int del = 0;

                                StaticStringParser staticStringParser = Tokens[i + 1] as StaticStringParser;
                                while (staticStringParser != null && (del = staticStringParser.MatchLength(test)) == 0 && test.Length > 0)
                                {
                                    test = test.Substring(1);
                                }

                                test = test.Substring(del);
                                i++;
                            }
                            else // no support for chained variables
                            {
                                throw new NotSupportedException("no support for chained variables");
                            }
                        }
                        else // during
                        {
                            if (Tokens.Count == i + 1) // last token
                            {
                                bool ok = token.TryParse(test, out o);

                                if (ok)
                                {
                                    coeff += test.Length * token.Coeff;
                                    test = "";
                                }
                                else
                                {
                                    op = 2;
                                    after = token.RandomExample();
                                }
                            }
                            else if (Tokens[i + 1] is StaticStringParser) // following token is a static string
                            {
                                int del = 0;

                                StaticStringParser staticStringParser = Tokens[i + 1] as StaticStringParser;

                                while (staticStringParser != null && (del = staticStringParser.MatchLength(test)) == 0 && test.Length > 0)
                                {
                                    test = test.Substring(1);
                                }

                                test = test.Substring(del);
                                i++;
                            }
                            else // no support for chained variables
                            {
                                throw new NotSupportedException("no support for chained variables yet");
                            }
                        }
                    }
                    else // after
                    {
                        string ex = token.RandomExample();
                        after += ex;
                        test = test.Substring(ex.Length);
                    }
                }
            }

            if (coeff > 0)
                sugg = new Suggestion(this, before, after, input);
            return coeff;
        }

        internal int Parse(string input, out Dictionary<int, string> variables)
        {
            int coeff = 0;
            string test = input;
            StringBuilder builder = new StringBuilder();
            variables = new Dictionary<int, string>();

            for (int i = 0; i < Tokens.Count; i++)
            {
                IParser token = Tokens[i];

                if (token is StaticStringParser staticParser)
                {
                    int del = staticParser.MatchLength(test);

                    if (del > 0) // we have a static match
                        coeff += del;
                    else // no static match
                        return -1;

                    test = test.Substring(del);
                }
                else if (Tokens.Count == i + 1) // last token
                {
                    variables.Add(i, test);
                    test = "";
                }
                else // attempt to find a match till next static parser
                {
                    builder.Clear();
                    int del;

                    StaticStringParser parser = Tokens[i + 1] as StaticStringParser;
                    while ((del = parser.MatchLength(test)) == 0 && test.Length > 0)
                    {
                        builder.Append(test[0]);
                        test = test.Substring(1);
                    }

                    test = test.Substring(del);
                    variables.Add(i, builder.ToString());
                    i++;
                }
            }

            if (!string.IsNullOrWhiteSpace(test))
                // No match till end
                return -1;

            return coeff;
        }

        private Answer<T> CreateAnswer<T>(Engine en, MethodInfo method, object target, params object[] args)
        {
            return typeof(T) == typeof(object)
                ? (Answer<T>)(object)new Answer(en, SentenceDescription, SentenceLanguage, SentenceID, method, target, args)
                : new Answer<T>(en, SentenceDescription, SentenceLanguage, SentenceID, method, target, args);
        }

        internal bool TryFinaleParse<T>(Engine en, Dictionary<int, string> vars, out Answer<T> answer)
        {
            answer = null;

            if (m_built)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();

                foreach (KeyValuePair<int, string> pair in vars)
                {
                    if (Tokens[pair.Key].TryParse(pair.Value, out object parameter))
                        dic[ParametersName[pair.Key]] = parameter;
                    else
                        return false;
                }

                Delegate @delegate = (Delegate)m_action ?? m_func;
                answer = CreateAnswer<T>(en, @delegate.GetMethodInfo(), @delegate.Target, new DynamicDictionary(dic));
            }
            else
            {
                var orderedArgs = new object[Parameters.Length];

                for (int o = 0; o < Parameters.Length; o++)
                {
                    ParameterInfo parameter = Parameters[o];

                    if (parameter.ParameterType == typeof(SentenceAttribute))
                    {
                        orderedArgs[o] = new SentenceAttribute(Templates)
                        {
                            Description = SentenceDescription,
                            ID = SentenceID,
                            Language = SentenceLanguage
                        };
                    }
                    else if (parameter.HasDefaultValue)
                    {
                        orderedArgs[o] = parameter.DefaultValue;
                    }
                }

                foreach (KeyValuePair<int, string> pair in vars)
                {
                    if (Tokens[pair.Key].TryParse(pair.Value, out object arg))
                    {
                        int pos = 0;
                        try
                        {
                            while (Parameters[pos].Name != ParametersName[pair.Key])
                                pos++;
                        }
                        catch (Exception)
                        {
                            throw new Exception($"Expected a parameter named '{ParametersName[pair.Key]}'");
                        }

                        orderedArgs[pos] = arg ?? throw new ArgumentNullException();
                    }
                    else
                    {
                        return false;
                    }
                }

                answer = CreateAnswer<T>(en, Method, null, orderedArgs);
            }
            return true;
        }

        internal static IEnumerable<SentenceParser> Generate(MethodInfo info)
        {
            var attrs = info.GetCustomAttributes<SentenceAttribute>();

            foreach (SentenceAttribute attr in attrs)
            {
                foreach (string s in attr.Sentences)
                {
                    var parsed = ParseSentence(s, info.GetParameters());

                    if (parsed != null)
                    {
                        yield return new SentenceParser(parsed.Item1, parsed.Item2, info, attr, parsed.Item3);
                    }
                }
            }
        }

        internal static Tuple<List<IParser>, List<string>, List<string[]>> ParseSentence(
            string s, IDictionary<string, SentenceBuilder.SentenceBuilderParameter> parameters)
        {
            List<IParser> parsers = new List<IParser>();
            List<string> names = new List<string>();
            List<string[]> suggestions = new List<string[]>();

            string block = "";
            byte op = 0;

            foreach (char c in s)
            {
                if (c == '{')
                {
                    if (block.Length > 0)
                    {
                        parsers.Add(new StaticStringParser(block));
                        suggestions.Add(new[] { block });
                        names.Add(null);

                        block = string.Empty;
                    }
                    op = 1;
                }
                else if (c == '}' && op == 1)
                {
                    IParser parser = parameters[block].Parser;

                    parsers.Add(parser);
                    suggestions.Add(parameters[block].HasExamples ? parameters[block].Examples.ToArray() : parser.Examples.ToArray());
                    names.Add(block);

                    op = 0;
                    block = string.Empty;
                }
                else
                {
                    block += c;
                }
            }

            if (!string.IsNullOrWhiteSpace(block))
            {
                parsers.Add(new StaticStringParser(block));
                suggestions.Add(new[] { block });
                names.Add(null);
            }

            return Tuple.Create(parsers, names, suggestions);
        }

        internal static Tuple<List<IParser>, List<string>, List<string[]>> ParseSentence(string s, ParameterInfo[] parameters)
        {
            List<IParser> parsers = new List<IParser>();
            List<string> names = new List<string>();
            List<string[]> suggestions = new List<string[]>();

            string block = "";
            byte op = 0;

            foreach (char c in s)
            {
                if (c == '{')
                {
                    if (block.Length > 0)
                    {
                        parsers.Add(new StaticStringParser(block));
                        suggestions.Add(new[] { block });
                        names.Add(null);
                        block = string.Empty;
                    }
                    op = 1;
                }
                else if (c == '}' && op == 1)
                {
                    ParameterInfo para = parameters.FirstOrDefault(y => string.Equals(y.Name, block, StringComparison.CurrentCultureIgnoreCase));
                    ParserAttribute attr = para.GetCustomAttribute<ParserAttribute>();

                    IParser parser = attr?.CustomParser ?? Engine.GetParserForParameter(para.ParameterType);

                    if (parser == null)
                        throw new Exception($"Cannot find a parser for type {para.ParameterType}");

                    parsers.Add(parser);
                    suggestions.Add(attr?.Examples != null && attr.Examples.Length > 0 ? attr.Examples : parser.Examples.ToArray());
                    names.Add(block);

                    op = 0;
                    block = string.Empty;
                }
                else
                {
                    block += c;
                }
            }

            if (!string.IsNullOrWhiteSpace(block))
            {
                parsers.Add(new StaticStringParser(block));
                suggestions.Add(new[] { block });
                names.Add(null);
            }

            return Tuple.Create(parsers, names, suggestions);
        }

        public override string ToString() => Full;
    }
}
