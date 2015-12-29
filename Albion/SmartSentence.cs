using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Albion
{
    internal class SmartSentence : IEnumerable<SmartSentence.Token>
    {
        private Stack<Token> Tokens { get; set; }
        public Sentence Sentence { get; set; }
        public int VariablesCount { get { return Tokens.Count(x => x.Type == TokenType.Variable); } }

        public SmartSentence(Sentence sentence)
        {
            Sentence = sentence;
            Tokens = new Stack<Token>();

            // no need to check for incomplete {}'s, the engine takes care of that.

            Dictionary<string, ParameterInfo> types = Sentence.Method.GetParameters()
                .ToDictionary(x => x.Name, x => x);
            
            using (StringReader sr = new StringReader(sentence.Template))
            {
                string cs = "";
                string vs = null;

                char c;

                while ((c = (char)sr.Read()) < char.MaxValue)
                {
                    if (c == '{')
                    {
                        vs = "";

                        if (cs != "")
                        {
                            Tokens.Push(new Token(cs));
                            cs = "";
                        }
                    }
                    else if (c == '}')
                    {
                        Tokens.Push(new Token(types[vs]));
                        vs = null;
                    }
                    else if (vs != null)
                    {
                        vs += c;
                    }
                    else
                    {
                        cs += c;
                    }
                }

                if (cs != "")
                {
                    Tokens.Push(new Token(cs));
                }
            }
        }

        public IEnumerator<Token> GetEnumerator()
        {
            return Tokens.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public enum TokenType
        {
            String = 0,
            Variable = 1
        }

        public class Token
        {
            public TokenType Type { get; set; }
            public string Value { get; set; }
            private ParameterInfo Infos { get; set; }

            public Token(ParameterInfo info)
            {
                Infos = info;
                Type = TokenType.Variable;
                Value = info.Name;
            }

            public Token(string str)
            {
                Type = TokenType.String;
                Value = str;
            }

            public object Convert(string input)
            {
                var conv = Engine.Converters.FirstOrDefault(x => x.Type == Infos.ParameterType && x.CanConvert(input));

                if (conv == null)
                {
                    return null;
                }
                else
                {
                    return conv.Convert(input);
                }
            }

            public override string ToString()
            {
                return Value;
            }
        }
    }
}
