using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albion.Parsers
{
    public class ArrayParser<T> : TypeParser<T[]>
    {
        public static string[] Separators = new string[] { ";", ",", "and", "or" };

        public override IEnumerable<string> Examples
        {
            get
            {
                yield return "one, two and three";
            }
        }

        protected override bool TryParse(string s, out T[] res)
        {
            IParser parser = Engine.GetParserForParameter(typeof(T));

            string[] items = s.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            T[] parsedItems = new T[items.Length];

            bool success = true;

            for (int i = 0; i < items.Length; i++)
            {
                object obj;
                if (parser.TryParse(items[i], out obj))
                {
                    parsedItems[i] = (T)obj;
                }
                else
                {
                    success = false;
                    break;
                }
            }

            res = parsedItems;
            return success;
        }
    }
}
