using Albion.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Albion.Parsers
{
    public class StringEnumParser : TypeParser<string>
    {
        private string[] possibilities;

        public override int Coeff { get { return 1000; } }
        public bool CaseSensitive { get; set; }
        public bool UseRegex { get; set; }
        public bool Trim { get; set; }

        public StringEnumParser(bool casesensitive, bool useregex, bool trim, params string[] possibilities)
        {
            this.possibilities = possibilities;

            CaseSensitive = casesensitive;
            UseRegex = useregex;
            Trim = trim;
        }

        public override IEnumerable<string> Examples
        {
            get
            {
                return possibilities;
            }
        }

        protected override bool TryParse(string s, out string res)
        {
            res = Trim ? s.Trim() : s;

            if (!CaseSensitive)
                s = s.ToLower();

            if (UseRegex && CaseSensitive)
            {
                foreach (string poss in possibilities)
                    if (Regex.IsMatch(s, poss))
                        return true;
            }
            else if (UseRegex)
            {
                foreach (string poss in possibilities)
                    if (Regex.IsMatch(s, poss, RegexOptions.IgnoreCase))
                        return true;
            }
            else if (CaseSensitive)
            {
                foreach (string poss in possibilities)
                    if (poss == s)
                        return true;
            }
            else
            {
                foreach (string poss in possibilities)
                    if (poss.ToLower() == s)
                        return true;
            }
            return false;
        }
    }
}
