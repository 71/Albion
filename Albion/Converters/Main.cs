using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Albion
{
    public static partial class Converters
    {
        public static object Auto(string w)
        {
            w = w.Trim();
			if (Regex.IsMatch(w, @"^\d+$")) return int.Parse(w);
			else if (Regex.IsMatch(w, @"^\d+.\d+$")) return double.Parse(w);
            else if (Number(w) > 0) return Number(w);
            else return null;
        }
    }
}
