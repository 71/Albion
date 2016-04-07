using Albion.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Albion
{
    public class SentenceBuilder
    {
        private Engine engine;

        internal string description;
        internal string language;
        internal Dictionary<string, IDictionary<string, SentenceBuilderParameter>> sentences;

        internal SentenceBuilder(Engine e, string lang)
        {
            engine = e;
            language = lang;
            sentences = new Dictionary<string, IDictionary<string, SentenceBuilderParameter>>();
        }

        private static IEnumerable<KeyValuePair<string, SentenceBuilderParameter>> GetParameters(Expression<Func<SentenceBuilderParameter, SentenceBuilderParameter>>[] exprs)
        {
            foreach (var expr in exprs)
            {
                SentenceBuilderParameter par = (expr.Body is ConstantExpression)
                    ? (SentenceBuilderParameter)(expr.Body as ConstantExpression).Value
                    : (SentenceBuilderParameter)expr.Compile()(new SentenceBuilderParameter());

                yield return new KeyValuePair<string, SentenceBuilderParameter>(expr.Parameters[0].Name, par);
            }
        }

        public SentenceBuilder Description(string descr)
        {
            description = descr;
            return this;
        }

        public void Handler(Action<dynamic> handler)
        {
            foreach (var sentence in sentences)
            {
                var parsed = SentenceParser.ParseSentence(sentence.Key, sentence.Value);

                if (parsed != null)
                    engine.Register(new SentenceParser(parsed.Item1, parsed.Item2, handler, parsed.Item3, sentence.Key, language, description));
            }
        }

        public void Handler(Func<dynamic, object> handler)
        {
            foreach (var sentence in sentences)
            {
                var parsed = SentenceParser.ParseSentence(sentence.Key, sentence.Value);

                if (parsed != null)
                    engine.Register(new SentenceParser(parsed.Item1, parsed.Item2, handler, parsed.Item3, sentence.Key, language, description));
            }
        }

        public SentenceBuilder Sentence(string sentence, params Expression<Func<SentenceBuilderParameter, SentenceBuilderParameter>>[] parameters)
        {
            sentences.Add(sentence, GetParameters(parameters).ToDictionary(x => x.Key, x => x.Value));
            return this;
        }

        public class SentenceBuilderParameter
        {
            internal IParser Parser
            {
                get
                {
                    return _parser == null
                        ? Engine.GetParserForParameter(this.Type)
                        : _parser;
                }
            }

            internal Type Type
            {
                get
                {
                    if (_type == null)
                        throw new ArgumentNullException("No type has been specified.");
                    else
                        return _type;
                }
            }

            internal bool HasExamples { get { return _examples.Count > 0; } }

            internal IEnumerable<string> Examples
            {
                get
                {
                    return _examples;
                }
            }

            private Type _type;
            private IParser _parser;
            private List<string> _examples;

            internal SentenceBuilderParameter()
            {
                _examples = new List<string>();
            }

            public SentenceBuilderParameter CustomExample(params string[] examples)
            {
                _examples.AddRange(examples);
                return this;
            }

            public SentenceBuilderParameter IsType(Type type)
            {
                _type = type;
                return this;
            }

            public SentenceBuilderParameter IsType<T>()
            {
                _type = typeof(T);
                return this;
            }

            public SentenceBuilderParameter UseParser(IParser parser)
            {
                _parser = parser;
                return this;
            }

            public SentenceBuilderParameter UseParser<TParser>() where TParser : IParser, new()
            {
                _parser = new TParser();
                return this;
            }
        }
    }
}
