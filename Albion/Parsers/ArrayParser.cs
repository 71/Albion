using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albion.Parsers
{
    public class ArrayParser<T> : TypeParser<T[]>
    {
        public override IEnumerable<string> Examples
        {
            get
            {
                yield return "one, two and three";
            }
        }

        protected override bool TryParse(string s, out T[] res)
        {
            throw new NotImplementedException();
        }
    }
}
