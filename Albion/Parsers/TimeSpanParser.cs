using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Albion.Parsers
{
    [PhraseParser]
    public class TimeSpanParser : TypeParser<TimeSpan>
    {
        public override int Coeff { get { return 100; } }

        public override IEnumerable<string> Examples
        {
            get
            {
                yield return "2 days";
                yield return "a week and twenty-three days";
            }
        }

        protected override bool TryParse(string s, out TimeSpan res)
        {
            res = Converters.In(s);
            return true;
        }
    }
}
