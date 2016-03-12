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
    public class DateTimeParser : TypeParser<DateTime>
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

        protected override bool TryParse(string s, out DateTime res)
        {
            res = Converters.Date(s);
            return true;
        }
    }
}
