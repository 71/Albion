using Albion.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Albion.Parsers
{

#pragma warning disable 1591
    public interface IParser
    {
        int Coeff { get; }
        Type ParseTo { get; }
        IEnumerable<string> Examples { get; }

        bool TryParse(string s, out object res);
        string RandomExample();
    }
#pragma warning restore 1591

    /// <summary>
    /// A base class for parsers.
    /// If you want your custom to be registered as a default parser, add the <see cref="TypeParserAttribute"/> attribute to it.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to which to parse</typeparam>
    public abstract class TypeParser<T> : IParser
    {
        private static Random ExampleChooser = new Random();

        /// <summary>
        /// The <see cref="Type"/> to which to parse.
        /// </summary>
        public Type ParseTo { get { return typeof(T); } }
        /// <summary>
        /// A list of strings that replace the variables in brackets when using <see cref="Engine.Suggest(string)"/>.
        /// It should contain at least one string.
        /// </summary>
        public abstract IEnumerable<string> Examples { get; }
        /// <summary>
        /// A number indicating how complex the match is. Used when calculating how relevant a suggestion is.
        /// </summary>
        /// <example>A <see cref="string"/> is the easiest type to parse, and has a coeff of 1.</example>
        /// <example>An <see cref="int"/> is harder to parse, and has a coeff of 100.</example>
        public abstract int Coeff { get; }

        /// <summary>
        /// The publicly exposed <see cref="TryParse(string, out T)"/> method.
        /// </summary>
        /// <param name="s">The input string to parse</param>
        /// <param name="res">The parsed object, if successful</param>
        /// <returns>Whether or not the parsing was successful</returns>
        public bool TryParse(string s, out object res)
        {
            T r = default(T);
            bool b = TryParse(s, out r);
            res = r;
            return b;
        }

        /// <summary>
        /// Method used to convert a <see cref="string"/> to a <typeparamref name="T"/> object.
        /// </summary>
        /// <param name="s">The input string to parse</param>
        /// <param name="res">The parsed object, if successful</param>
        /// <returns>Whether or not the parsing was successful</returns>
        protected abstract bool TryParse(string s, out T res);

        /// <summary>
        /// Return a pseudo-random string in <see cref="Examples"/>.
        /// </summary>
        /// <returns>A pseudo-random string in <see cref="Examples"/></returns>
        public string RandomExample()
        {
            return Examples.ElementAt(ExampleChooser.Next(Examples.Count()));
        }
    }
}
