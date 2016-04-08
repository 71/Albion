using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Albion.Parsers
{
    internal static partial class Converters
    {
        /// <summary>
        /// Returns a string formatted using String.Format(), and with an s in plural words.
        /// </summary>
        /// <param name="s">The string to format, where things are singular</param>
        /// <param name="args">The arguments to format</param>
        public static string Plural(this string s, params object[] args)
        {
            s = String.Format(s, args) + " ";
            s = Regex.Replace(s, @"(?<=(?:[02-9]\d*?|11) \w+?)y", "ies");
            s = Regex.Replace(s, @"(?<=(?:[02-9]\d*?|11) \w+?[^sx])(?=[^\w])", "s");
            s = s.Replace("daies", "days");

            string[] s1 = new string[] { "man", "woman" };
            string[] s2 = new string[] { "men", "women" };
            for (int i = 0; i < s1.Length; i++)
                s = Regex.Replace(s, @"(?<=\d*[^1] )" + s1[i] + "s", s2[i]);

            return s.Substring(0, s.Length - 1);
        }

        /// <summary>
        /// Returns a string in which the first character is capitalized.
        /// </summary>
        public static string Capitalized(this string s)
        {
            return s.Substring(0, 1).ToUpper() + s.Substring(1);
        }
    }
}
