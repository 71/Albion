using Albion.Attributes;
using Albion.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Albion.Parsers
{
    internal class SentenceParser
    {
        private static Random random = new Random();
        private static string AnyOf(string[] strs)
        {
            return strs[random.Next(strs.Length)];
        }

        private bool m_built;
        private Action<dynamic> m_action;
        private Func<dynamic, object> m_func;

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
                return string.Join("", CustomSuggestions == null ? new string[0] : CustomSuggestions.Select(x => AnyOf(x)));
            }
        }

        internal SentenceParser(List<IParser> tokens, List<string> names, Func<dynamic, object> deleg, List<string[]> suggestions, string sentence, string lang, string descr, string id)
        {
            var info = deleg.GetMethodInfo();

            SentenceLanguage = lang;
            SentenceDescription = descr;
            SentenceID = id;
            Templates = new string[] { sentence };

            Method = info;
            ParametersName = names;
            Parameters = info.GetParameters();
            Tokens = tokens;
            ReturnType = info.ReturnType;
            CustomSuggestions = suggestions;
            m_built = true;
            m_func = deleg;
        }

        internal SentenceParser(List<IParser> tokens, List<string> names, Action<dynamic> deleg, List<string[]> suggestions, string sentence, string lang, string descr, string id)
        {
            var info = deleg.GetMethodInfo();

            SentenceLanguage = lang;
            SentenceDescription = descr;
            SentenceID = id;
            Templates = new string[] { sentence };

            Method = info;
            ParametersName = names;
            Parameters = info.GetParameters();
            Tokens = tokens;
            ReturnType = info.ReturnType;
            CustomSuggestions = suggestions;
            m_built = true;
            m_action = deleg;
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
        }

        internal int Suggest(string input, bool deep, out Suggestion sugg)
        {
            sugg = null;

            int coeff = 0;
            string test = input.ToLower();
            
            if (!deep)
            {
                IParser token = Tokens.FirstOrDefault();
                if (token != null && token is StaticStringParser && ((StaticStringParser)token).Reference.StartsWith(test))
                {
                    string a = ((StaticStringParser)token).Reference.Substring(test.Length);

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
                                test = test.Substring((token as StaticStringParser).Reference.Length);
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

                                while ((del = (Tokens[i + 1] as StaticStringParser).MatchLength(test)) == 0 && test.Length > 0)
                                {
                                    test = test.Substring(1);
                                }

                                test = test.Substring(del);
                                i++;
                            }
                            else // no support for chained variables
                            {
                                throw new NotImplementedException("no support for chained variables");
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

                                while ((del = (Tokens[i + 1] as StaticStringParser).MatchLength(test)) == 0 && test.Length > 0)
                                {
                                    test = test.Substring(1);
                                }

                                test = test.Substring(del);
                                i++;
                            }
                            else // no support for chained variables
                            {
                                throw new NotImplementedException("no support for chained variables yet");
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
            variables = new Dictionary<int, string>();

            for (int i = 0; i < Tokens.Count; i++)
            {
                IParser token = Tokens[i];

                if (token is StaticStringParser)
                {
                    int del = (token as StaticStringParser).MatchLength(test);

                    if (del > 0) // we have a static match
                    {
                        coeff += del;
                    }
                    else // no static match
                    {
                        return -1;
                    }

                    test = test.Substring(del);
                }
                else
                {
                    if (Tokens.Count == i + 1) // last token
                    {
                        variables.Add(i, test);
                    }
                    else if (Tokens[i + 1] is StaticStringParser) // following token is a static string
                    {
                        string variable = "";
                        int del = 0;

                        while ((del = (Tokens[i + 1] as StaticStringParser).MatchLength(test)) == 0 && test.Length > 0)
                        {
                            variable += test[0];
                            test = test.Substring(1);
                        }

                        test = test.Substring(del);
                        variables.Add(i, variable);
                        i++;
                    }
                    else // no support for chained variables yet
                    {
                        throw new NotImplementedException("no support for chained variables");
                    }
                }
            }
            
            return coeff;
        }

        internal bool TryFinaleParse(Dictionary<int, string> vars, out Answer answer)
        {
            answer = null;

            if (m_built)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();

                int i = 0;
                foreach (KeyValuePair<int, string> pair in vars)
                {
                    object parameter;
                    if (Tokens[pair.Key].TryParse(pair.Value, out parameter))
                        dic[ParametersName[pair.Key]] = parameter;
                    else
                        return false;
                    i++;
                }

                answer = m_action == null
                    ? new Answer(m_func, new DynamicDictionary(dic), SentenceLanguage, SentenceDescription, SentenceID)
                    : new Answer(m_action, new DynamicDictionary(dic), SentenceLanguage, SentenceDescription, SentenceID);
            }
            else
            {
                var orderedParameters = new object[Parameters.Length];

                for (int o = 0; o < Parameters.Length; o++)
                {
                    if (Parameters[o].ParameterType == typeof(SentenceAttribute))
                    {
                        orderedParameters[o] = new SentenceAttribute(Templates)
                        {
                            Description = SentenceDescription,
                            ID = SentenceID,
                            Language = SentenceLanguage
                        };
                    }
                }

                int i = 0;
                foreach (KeyValuePair<int, string> pair in vars)
                {
                    object parameter;
                    if (Tokens[pair.Key].TryParse(pair.Value, out parameter))
                    {
                        int pos = 0;
                        while (Parameters[pos].Name != ParametersName[pair.Key])
                            pos++;

                        if (parameter == null)
                        {
                            if (Parameters[pos].HasDefaultValue)
                                parameter = Parameters[pos].DefaultValue;
                            else
                                throw new ArgumentNullException();
                        }

                        orderedParameters[pos] = parameter;
                    }
                    else
                    {
                        return false;
                    }
                    i++;
                }

                answer = new Answer(Method, orderedParameters, SentenceLanguage, SentenceDescription, SentenceID);
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

            foreach (char c in s.ToCharArray())
            {
                if (c == '{')
                {
                    if (block.Length > 0)
                    {
                        parsers.Add(new StaticStringParser(block));
                        names.Add(null);
                        block = String.Empty;
                    }
                    op = 1;
                }
                else if (c == '}' && op == 1)
                {
                    block = block.ToLower();
                    IParser parser = parameters[block].Parser;

                    parsers.Add(parser);
                    suggestions.Add(parameters[block].HasExamples ? parameters[block].Examples.ToArray() : parser.Examples.ToArray());
                    names.Add(block);

                    op = 0;
                    block = String.Empty;
                }
                else
                {
                    block += c;
                }
            }

            if (!String.IsNullOrWhiteSpace(block))
            {
                parsers.Add(new StaticStringParser(block));
                names.Add(null);
            }

            return new Tuple<List<IParser>, List<string>, List<string[]>>(parsers, names, suggestions);
        }

        internal static Tuple<List<IParser>, List<string>, List<string[]>> ParseSentence(string s, ParameterInfo[] parameters)
        {
            List<IParser> parsers = new List<IParser>();
            List<string> names = new List<string>();
            List<string[]> suggestions = new List<string[]>();

            string block = "";
            byte op = 0;

            foreach (char c in s.ToCharArray())
            {
                if (c == '{')
                {
                    if (block.Length > 0)
                    {
                        parsers.Add(new StaticStringParser(block));
                        names.Add(null);
                        block = String.Empty;
                    }
                    op = 1;
                }
                else if (c == '}' && op == 1)
                {
                    block = block.ToLower();
                    ParameterInfo para = parameters.FirstOrDefault(y => y.Name.ToLower() == block);
                    ParserAttribute attr = para.GetCustomAttribute<ParserAttribute>();

                    IParser parser = attr?.CustomParser ?? Engine.GetParserForParameter(para.ParameterType);

                    parsers.Add(parser);
                    suggestions.Add(attr != null && attr.Examples.Length > 0 ? attr.Examples : parser.Examples.ToArray());
                    names.Add(block);

                    op = 0;
                    block = String.Empty;
                }
                else
                {
                    block += c;
                }
            }

            if (!String.IsNullOrWhiteSpace(block))
            {
                parsers.Add(new StaticStringParser(block));
                names.Add(null);
            }

            return new Tuple<List<IParser>, List<string>, List<string[]>>(parsers, names, suggestions);
        }

        public override string ToString()
        {
            return Full;
        }
    }
}
