using Albion.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albion
{
    /// <summary>
    /// The kind of scan you wish to make.
    /// A scan can be either <see cref="Normal"/> or <see cref="Deep"/>,
    /// but can be any combinaison of <see cref="Sentence"/>, <see cref="Description"/> and <see cref="ID"/>.
    /// </summary>
    public enum SuggestionMatchType : byte
    {
        /// <summary>
        /// Only scan the the beginning of the sentence,
        /// and stop if it does not match.
        /// </summary>
        Normal = 1,
        /// <summary>
        /// Keep scanning until the end, even if there is no match.
        /// </summary>
        Deep = 2,

        /// <summary>
        /// Scan only sentence templates.
        /// </summary>
        /// <example>"I'm Gr" will match with "I'm {person}"</example>
        Sentence = 4,
        /// <summary>
        /// Scan only descriptions.
        /// </summary>
        Description = 8,
        /// <summary>
        /// Scan only IDs.
        /// </summary>
        ID = 16
    }

    /// <summary>
    /// 
    /// </summary>
    public class Suggestion
    {
        /// <summary>
        /// The description provided by <see cref="SentenceSentenceDescription"/> or <see cref="SentenceBuilder.Description(string)"/>.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// The language of the matched sentence.
        /// </summary>
        public string Language { get; private set; }
        /// <summary>
        /// The ID provided by <see cref="SentenceSentenceID"/> or <see cref="SentenceBuilder.ID(string)"/>.
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// The <see cref="SuggestionMatchType"/> flags given by the user when calling <see cref="Engine.Suggest(string, SuggestionMatchType)"/>.
        /// </summary>
        public SuggestionMatchType MatchType { get; private set; }

        /// <summary>
        /// The string before the match
        /// </summary>
        public string Before { get; private set; }
        /// <summary>
        /// The user input
        /// </summary>
        public string Match { get; private set; }
        /// <summary>
        /// The string after the match
        /// </summary>
        public string After { get; private set; }
        
        internal Suggestion(SentenceParser parser, string before, string after, string input)
        {
            Description = parser.SentenceDescription;
            Language = parser.SentenceLanguage;
            ID = parser.SentenceID;

            MatchType = SuggestionMatchType.Sentence;

            Before = before;
            After = after;
            Match = input;
        }

        /// <summary>
        /// Input is contained in a static parser, or in the sentence
        /// </summary>
        internal Suggestion(SentenceParser parser, string input, SuggestionMatchType type)
        {
            Description = parser.SentenceDescription;
            Language = parser.SentenceLanguage;
            ID = parser.SentenceID;

            MatchType = type;

            string full;

            if (type.HasFlag(SuggestionMatchType.Description))
            {
                full = Description;
            }
            else if (type.HasFlag(SuggestionMatchType.ID))
            {
                full = ID;
            }
            else
            {
                full = parser.Full;
            }

            Before = "";
            Match = "";
            After = "";
            
            while (!full.StartsWith(input.ToLower()) && full.Length > 0)
            {
                Before += full[0];
                full = full.Substring(1);
            }

            Match = input;
            After = full.Substring(input.Length);
        }

        /// <summary>
        /// <see cref="Before"/> + <see cref="Match"/> + <see cref="After"/>.
        /// </summary>
        public override string ToString()
        {
            return Converters.Capitalized(Before + Match + After);
        }
    }
}
