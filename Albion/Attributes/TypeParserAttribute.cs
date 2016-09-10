using System;

namespace Albion
{
    /// <summary>
    /// Indicates that this class can parse phrases of a sentence.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class PhraseParserAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new <see cref="PhraseParserAttribute"/>.
        /// </summary>
        public PhraseParserAttribute() { }
    }
}
