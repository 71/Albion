using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Albion
{
    /// <summary>
    /// Attribute used by <see cref="Engine.Register(Type[])"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class SentenceAttribute : Attribute
    {
        /// <summary>
        /// Create a new <see cref="SentenceAttribute"/> attribute.
        /// </summary>
        /// <param name="formattedSentences">An array of string, of format "Text {varname}"</param>
        public SentenceAttribute(params string[] formattedSentences)
        {
            if (formattedSentences.Length == 0)
                throw new EmptySentenceAttributeException("At least one string must be given to this attribute.");
            sentences = formattedSentences;
            
            foreach (string s in formattedSentences)
            {
                byte op = 0;
                foreach (char c in s)
                {
                    if (op == 0) // normal text
                    {
                        if (c == '{') op = 1;
                        else if (c == '}') throw new FormatException("Invalid format in sentence '" + s + "'.");
                    }
                    else if (op == 1) // variable name ; must no contain characters other than [\d\w_]
                    {
                        if (c == '}') op = 0;
                        else if (char.IsLetterOrDigit(c) || c == '_') continue;
                        else throw new FormatException("Invalid format in sentence '" + s + "'.");
                    }
                }

                if (op == 1)
                    throw new FormatException("Invalid format in sentence '" + s + "'.");
            }
        }

        /// <summary>
        /// Strings passed to <see cref="SentenceAttribute(string[])"/>.
        /// </summary>
        public string[] Sentences { get { return this.sentences; } }
        private string[] sentences;

        /// <summary>
        /// ID of these sentences
        /// </summary>
        public string ID { get { return this.id; } set { this.id = value; } }
        private string id = "";

        /// <summary>
        /// Description of these sentences in <see cref="Language"/>. Default: ""
        /// </summary>
        public string Description { get { return this.descr; } set { this.descr = value; } }
        private string descr = "";

        /// <summary>
        /// Language of the sentence. Default: "en"
        /// </summary>
        public string Language { get { return this.lang; } set { this.lang = value; } }
        private string lang = "en";
    }

    /// <summary>
    /// Indicates that no sentence template string was given
    /// </summary>
    public class EmptySentenceAttributeException : Exception
    {
        internal EmptySentenceAttributeException() : base() { }
        internal EmptySentenceAttributeException(string message) : base(message) { }
    }
}
