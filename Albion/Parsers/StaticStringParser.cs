using Albion.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Albion.Parsers
{
    [Parser]
    public class StaticStringParser : TypeParser<string>
    {
        public string Reference { get; private set; }
        public override IEnumerable<string> Examples { get { yield return Reference; } }
        protected override bool TryParse(string s, out string res) { throw new NotImplementedException(); }

        public StaticStringParser(string s)
        {
            Reference = s.ToLower();
        }

        public int MatchLength(string s)
        {
            return s.ToLower().StartsWith(Reference) ? Reference.Length : 0;
        }
    }
}
