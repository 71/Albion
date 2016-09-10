using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Albion.Parsers
{
    [PhraseParser]
    public class StaticStringParser : TypeParser<string>
    {
        public override int Coeff { get { return 10000; } }

        public string Reference { get; private set; }
        public override IEnumerable<string> Examples { get { yield return Reference; } }
        protected override bool TryParse(string s, out string res) { throw new NotImplementedException(); }

        public StaticStringParser(string s)
        {
            Reference = s.ToLower();
        }

        public int MatchLength(string s)
        {
            s = s.ToLower();
            return s.ToLower().StartsWith(Reference) ? Reference.Length : Reference.StartsWith(s) ? s.Length : 0;
        }
    }
}
