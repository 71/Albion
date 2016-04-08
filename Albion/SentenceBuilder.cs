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
    /// <summary>
    /// 
    /// </summary>
    public class SentenceBuilder
    {
        private Engine engine;

        internal string id;
        internal string description;
        internal string language;
        internal Dictionary<string, IDictionary<string, SentenceBuilderParameter>> sentences;
        internal Dictionary<string, string> sentenceLanguages;

        internal SentenceBuilder(Engine e, string lang)
        {
            engine = e;
            language = lang;
            description = "";
            id = "";
            sentences = new Dictionary<string, IDictionary<string, SentenceBuilderParameter>>();
            sentenceLanguages = new Dictionary<string, string>();
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

        /// <summary>
        /// Set the handler of this sentence to <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler">The <see cref="Action{T}"/> invoked when the provided sentence has a match</param>
        public void Handler(Action<dynamic> handler)
        {
            foreach (var sentence in sentences)
            {
                var parsed = SentenceParser.ParseSentence(sentence.Key, sentence.Value);

                if (parsed != null)
                    engine.Register(new SentenceParser(parsed.Item1, parsed.Item2, handler, parsed.Item3, sentence.Key, language, description, id));
            }
        }

        /// <summary>
        /// Set the handler of this sentence to <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler">The <see cref="Func{T, TResult}"/> invoked when the provided sentence has a match</param>
        public void Handler(Func<dynamic, object> handler)
        {
            foreach (var sentence in sentences)
            {
                var parsed = SentenceParser.ParseSentence(sentence.Key, sentence.Value);

                if (parsed != null)
                    engine.Register(new SentenceParser(parsed.Item1, parsed.Item2, handler, parsed.Item3, sentence.Key, language, description, id));
            }
        }

        /// <summary>
        /// Set the description of this sentence to <paramref name="descr"/>.
        /// </summary>
        /// <param name="descr">The description, used by <see cref="Engine.Suggest(string)"/></param>
        /// <returns>this</returns>
        public SentenceBuilder Description(string descr)
        {
            description = descr;
            return this;
        }

        /// <summary>
        /// Set the ID of this sentence to <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The ID, used by <see cref="Engine.Suggest(string)"/></param>
        /// <returns>this</returns>
        public SentenceBuilder ID(string id)
        {
            this.id = id;
            return this;
        }

        /// <summary>
        /// Add a sentence template to the <see cref="Engine"/>.
        /// </summary>
        /// <param name="lang">The language of the sentence</param>
        /// <param name="sentence">The sentence template, of format "Text {varname}"</param>
        /// <param name="parameters">The different variables in the sentence</param>
        /// <returns>this</returns>
        public SentenceBuilder Sentence(string lang, string sentence, params Expression<Func<SentenceBuilderParameter, SentenceBuilderParameter>>[] parameters)
        {
            sentenceLanguages.Add(sentence, lang);
            sentences.Add(sentence, GetParameters(parameters).ToDictionary(x => x.Key, x => x.Value));
            return this;
        }

        /// <summary>
        /// Add a sentence template to the <see cref="Engine"/>, with the default language provided when calling <see cref="Engine.Build(string)"/>,
        /// or <see cref="Engine.Language"/> if no language was passed.
        /// </summary>
        /// <param name="sentence">The sentence template, of format "Text {varname}"</param>
        /// <param name="parameters">The different variables in the sentence</param>
        /// <returns>this</returns>
        public SentenceBuilder Sentence(string sentence, params Expression<Func<SentenceBuilderParameter, SentenceBuilderParameter>>[] parameters)
        {
            return Sentence(language, sentence, parameters);
        }

        /// <summary>
        /// A chained type that defines a parameter: its type, and optionally, custom examples, and custom parser.
        /// </summary>
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

            /// <summary>
            /// Define custom examples for this parameter. Corresponds to <see cref="ParserAttribute.Examples"/>.
            /// </summary>
            /// <param name="examples">Examples used in <see cref="Engine.Suggest(string)"/></param>
            /// <returns>this</returns>
            public SentenceBuilderParameter CustomExamples(params string[] examples)
            {
                _examples.AddRange(examples);
                return this;
            }

            /// <summary>
            /// Specifies the type of this parameter.
            /// </summary>
            /// <param name="type"></param>
            /// <returns>this</returns>
            public SentenceBuilderParameter IsType(Type type)
            {
                _type = type;
                return this;
            }

            /// <summary>
            /// Specifies the type of this parameter.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns>this</returns>
            public SentenceBuilderParameter IsType<T>()
            {
                _type = typeof(T);
                return this;
            }

            /// <summary>
            /// Define a custom parser for this parameter. Corresponds to <see cref="ParserAttribute.CustomParser"/>.
            /// </summary>
            /// <param name="parser">A custom <see cref="IParser"/></param>
            /// <returns>this</returns>
            public SentenceBuilderParameter UseParser(IParser parser)
            {
                _parser = parser;
                return this;
            }

            /// <summary>
            /// Define a custom parser for this parameter. Corresponds to <see cref="ParserAttribute.CustomParser"/>.
            /// The parser will be created.
            /// </summary>
            /// <returns>this</returns>
            public SentenceBuilderParameter UseParser<TParser>() where TParser : IParser, new()
            {
                _parser = new TParser();
                return this;
            }
        }
    }
}
