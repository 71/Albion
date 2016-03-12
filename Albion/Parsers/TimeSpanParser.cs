using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Albion.Attributes;

namespace Albion.Parsers
{
    [Parser]
    public class TimeSpanParser : TypeParser<TimeSpan>
    {
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
