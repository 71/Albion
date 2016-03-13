using Albion.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albion
{
    public enum SuggestionMatchType : byte
    {
        Normal = 1,
        Deep = 2,

        Sentence = 4,
        Description = 8,
        ID = 16
    }

    public class Suggestion
    {
        public SentenceAttribute SentenceAttr { get; private set; }
        public SuggestionMatchType MatchType { get; private set; }

        public string Before { get; private set; }
        public string Match { get; private set; }
        public string After { get; private set; }

        internal Suggestion(SentenceParser parser, string before, string after, string input)
        {
            SentenceAttr = parser.Attribute;
            MatchType = SuggestionMatchType.Sentence;

            Before = before;
            After = after;
            Match = input;
        }

        /// <summary>
        /// The input is contained in a static parser, or in the sentence
        /// </summary>
        internal Suggestion(SentenceParser parser, string input, SuggestionMatchType type)
        {
            SentenceAttr = parser.Attribute;
            MatchType = type;

            string full = type == SuggestionMatchType.Description
                ? parser.Attribute.Description
                : parser.Full;

            Before = "";
            Match = "";
            After = "";
            
            while (!full.StartsWith(input.ToLower()))
            {
                Before += full[0];
                full = full.Substring(1);
            }

            Match = input;
            After = full.Substring(input.Length);
        }

        public override string ToString()
        {
            return Converters.Capitalized(Before + Match + After);
        }
    }
}
