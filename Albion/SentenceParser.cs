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
        public Type Returns { get; private set; }
        public ParameterInfo[] Parameters { get; private set; }
        public List<IParser> Tokens { get; private set; }
        public List<string> ParametersName { get; private set; }
        public MethodInfo Method { get; private set; }
        public SentenceAttribute Attribute { get; private set; }

        private SentenceParser(List<IParser> tokens, List<string> names, MethodInfo info, SentenceAttribute attr)
        {
            Method = info;
            ParametersName = names;
            Parameters = info.GetParameters();
            Tokens = tokens;
            Returns = info.ReturnType;
            Attribute = attr;
        }

        internal bool TrySuggest(string input, out Suggestion sugg)
        {
            sugg = null;
            
            string test = input;
            List<string> suggestions = new List<string>();

            int index = 0;
            foreach (IParser token in Tokens)
            {
                index++;
                object o;

                while (!token.TryParse(test, out o) && !string.IsNullOrWhiteSpace(test))
                {
                    int del = 0;

                    for (int i = test.Length - 1; i >= 0; i--)
                    {
                        del++;
                        if (char.IsWhiteSpace(test[i]))
                            break;
                    }

                    test = test.Substring(0, test.Length - del);
                }

                if (o == null)
                {
                    return false;
                }
                else if (token.GetType() != typeof(StaticStringParser))
                {
                    suggestions.Add(token.RandomExample());
                }
                else
                {
                    suggestions.Add(test);
                }
            }

            sugg = new Suggestion(suggestions.ToArray());
            return true;
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
                        return -1;
                    }
                }
            }
            
            return coeff;
        }

        internal bool TryFinaleParse(Dictionary<int, string> vars, out Answer answer)
        {
            answer = null;

            var orderedParameters = new object[Parameters.Length];
            
            for (int o = 0; o < Parameters.Length; o++)
            {
                if (Parameters[o].ParameterType == typeof(SentenceAttribute))
                {
                    orderedParameters[o] = Attribute;
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

            answer = new Answer(Method, orderedParameters);
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
                        yield return new SentenceParser(parsed.Item1, parsed.Item2, info, attr);
                    }
                }
            }
        }

        private static Tuple<List<IParser>, List<string>> ParseSentence(string s, ParameterInfo[] parameters)
        {
            List<IParser> parsers = new List<IParser>();
            List<string> names = new List<string>();

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
                    IParser parser = Engine.GetParserForParameter(parameters.FirstOrDefault(y => y.Name.ToLower() == block)?.ParameterType);
                    parsers.Add(parser);
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

            return new Tuple<List<IParser>, List<string>>(parsers, names);
        }
    }
}
