using Albion.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Albion.Parsers
{

#pragma warning disable 1591
    public class StringEnumParser : TypeParser<string>
    {
        private string[] possibilities;

        public override int Coeff { get { return 1000; } }
        public bool CaseSensitive { get; set; }
        public bool UseRegex { get; set; }
        public bool Trim { get; set; }

        #region Constructors
        public StringEnumParser(string poss1, string poss2, string poss3, string poss4, string poss5, string poss6, string poss7, string poss8, string poss9, string poss10)
        {
            this.possibilities = new string[] { poss1, poss2, poss3, poss4, poss5, poss6, poss7, poss8, poss9, poss10 };

            CaseSensitive = false;
            UseRegex = true;
            Trim = true;
        }

        public StringEnumParser(string poss1, string poss2, string poss3, string poss4, string poss5, string poss6, string poss7, string poss8, string poss9)
        {
            this.possibilities = new string[] { poss1, poss2, poss3, poss4, poss5, poss6, poss7, poss8, poss9 };

            CaseSensitive = false;
            UseRegex = true;
            Trim = true;
        }

        public StringEnumParser(string poss1, string poss2, string poss3, string poss4, string poss5, string poss6, string poss7, string poss8)
        {
            this.possibilities = new string[] { poss1, poss2, poss3, poss4, poss5, poss6, poss7, poss8};

            CaseSensitive = false;
            UseRegex = true;
            Trim = true;
        }

        public StringEnumParser(string poss1, string poss2, string poss3, string poss4, string poss5, string poss6, string poss7)
        {
            this.possibilities = new string[] { poss1, poss2, poss3, poss4, poss5, poss6, poss7 };

            CaseSensitive = false;
            UseRegex = true;
            Trim = true;
        }

        public StringEnumParser(string poss1, string poss2, string poss3, string poss4, string poss5, string poss6)
        {
            this.possibilities = new string[] { poss1, poss2, poss3, poss4, poss5, poss6 };

            CaseSensitive = false;
            UseRegex = true;
            Trim = true;
        }

        public StringEnumParser(string poss1, string poss2, string poss3, string poss4, string poss5)
        {
            this.possibilities = new string[] { poss1, poss2, poss3, poss4, poss5 };

            CaseSensitive = false;
            UseRegex = true;
            Trim = true;
        }

        public StringEnumParser(string poss1, string poss2, string poss3, string poss4)
        {
            this.possibilities = new string[] { poss1, poss2, poss3, poss4 };

            CaseSensitive = false;
            UseRegex = true;
            Trim = true;
        }

        public StringEnumParser(string poss1, string poss2, string poss3)
        {
            this.possibilities = new string[] { poss1, poss2, poss3 };

            CaseSensitive = false;
            UseRegex = true;
            Trim = true;
        }

        public StringEnumParser(string poss1, string poss2)
        {
            this.possibilities = new string[] { poss1, poss2 };

            CaseSensitive = false;
            UseRegex = true;
            Trim = true;
        }

        public StringEnumParser(params string[] possibilities)
        {
            this.possibilities = possibilities;

            CaseSensitive = false;
            UseRegex = true;
            Trim = true;
        }

        public StringEnumParser(bool casesensitive, bool useregex, bool trim, params string[] possibilities)
        {
            this.possibilities = possibilities;

            CaseSensitive = casesensitive;
            UseRegex = useregex;
            Trim = trim;
        }
        #endregion

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
