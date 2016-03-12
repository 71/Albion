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
                yield return "the 21th of October 2016";
                yield return "tomorrow";
                yield return "in a week and twenty-three days";
            }
        }

        protected override bool TryParse(string s, out TimeSpan res)
        {
            res = Converters.In(s);
            return true;
        }
    }
}
