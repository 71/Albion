using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Albion.Parsers
{
    [PhraseParser]
    public class StringParser : TypeParser<string>
    {
        public override int Coeff { get { return 1; } }

        public override IEnumerable<string> Examples
        {
            get
            {
                yield return "world";
                yield return "eat";
                yield return "shower";
            }
        }

        protected override bool TryParse(string s, out string res)
        {
            res = s;
            return s.Length > 0;
        }
    }
}
