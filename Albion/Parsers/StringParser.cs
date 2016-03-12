using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Albion.Attributes;

namespace Albion.Parsers
{
    [Parser]
    public class StringParser : TypeParser<string>
    {
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
