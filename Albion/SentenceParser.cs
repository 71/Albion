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
        public Type ReturnType { get; private set; }
        public ParameterInfo[] Parameters { get; private set; }
        public List<IParser> Tokens { get; private set; }
        public List<string> ParametersName { get; private set; }
        public MethodInfo Method { get; private set; }
        public SentenceAttribute Attribute { get; private set; }
        public string Full { get { return string.Join("", Tokens.Select(x => x.RandomExample())); } }

        private SentenceParser(List<IParser> tokens, List<string> names, MethodInfo info, SentenceAttribute attr)
        {
            Method = info;
            ParametersName = names;
            Parameters = info.GetParameters();
            Tokens = tokens;
            ReturnType = info.ReturnType;
            Attribute = attr;
        }

        internal int Suggest(string input, bool deep, out Suggestion sugg)
        {
            sugg = null;

            int coeff = 0;
            string test = input.ToLower();

            foreach (StaticStringParser token in Tokens.OfType<StaticStringParser>())
            {
                if (token.Reference.Contains(test))
                {
                    sugg = new Suggestion(this, input, SuggestionMatchType.Sentence);
                    return input.Length;
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
                            else // no support for chained variables yet
                            {
                                throw new NotImplementedException("no support for chained variables yet");
                            }
                        }
                        else // during
                        {
                            if (Tokens.Count == i + 1) // last token
                            {
                                bool ok = token.TryParse(test, out o);

                                if (ok)
                                {
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
                            else // no support for chained variables yet
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
                        throw new NotImplementedException("no support for chained variables yet");
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

        public override string ToString()
        {
            return Full;
        }
    }
}
