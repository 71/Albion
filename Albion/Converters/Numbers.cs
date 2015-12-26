using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Albion
{
    public static partial class Converters
    {
        private static int Digit(string w)
        {
            w = w.Replace("fort", "fourt").Replace("nint", "ninet");
            List<string> s = w.Split(new string[1] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();

            List<string> i = new List<string>() { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

            List<string> ii = new List<string>() { "o", "ten", "twenty", "thirty", "fourty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            List<string> exc = new List<string>() { "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };

            if (exc.IndexOf(w) >= 0) return exc.IndexOf(w) + 10;
            else if (s.Count > 2 || s.Count < 1) return -1;
            else if (s.Count == 1)
            {
                if (ii.IndexOf(s[0]) > -1) s.Add("zero");
                else if (i.IndexOf(s[0]) > -1) s.Insert(0, "o");
                else return -1;
            }

            int iii = Int32.Parse(ii.IndexOf(s[0]).ToString() + i.IndexOf(s[1]).ToString());

            return iii;
        }

        /// <summary>
        /// Converts a string to an integer (up to 999 999).
        /// </summary>
        /// <param name="s">A string that'll be filtered.</param>
        /// <returns>null if incorrect format, or the int corresponding</returns>
        public static int Number(string s)
        {
            // extract the number
            s = Regex.Replace(s, @"[^a-zA-Z\s]", " ").ToLower();
            s = Regex.Replace(s, " +", " ");
            List<string> keeps = new List<string>()
                { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
                  "ten", "twenty", "thirty", "fourty", "fifty", "sixty", "seventy", "eighty", "ninety",
                  "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen",
                  "hundred", "thousand" };
            string[] splitted = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string keep in splitted)
                if (keeps.IndexOf(keep) < 0) s = Regex.Replace(s, String.Format(@"[^\w]{0}[^\w]|^{0}|{0}$", keep), " ");

            splitted = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (splitted.Length == 0) return -1;
            if (splitted.Count(x => x == "zero") > 0)
                if (splitted.Count(x => x == "zero") > 1) return -1;
                else if (splitted.Length > 1) return -1;
            if (splitted.Count(x => x == "hundred") > 2 || splitted.Count(x => x == "thousand") > 1) return -1;

            MatchCollection m = Regex.Matches(s, @"(?<ht>.*?)hundred(?=.*thousand)|(?<th>.*?)thousand|(?<hu>.*?)hundred|(?<in>.+?)$");

            var ght = m.Cast<Match>().Select(x => x.Groups["ht"]).FirstOrDefault(y => y.Value != "");
            string _ht = (ght == null) ? "" : ght.Value.Trim();
            var gth = m.Cast<Match>().Select(x => x.Groups["th"]).FirstOrDefault(y => y.Value != "");
            string _th = (gth == null) ? "" : gth.Value.Trim();
            var ghu = m.Cast<Match>().Select(x => x.Groups["hu"]).FirstOrDefault(y => y.Value != "");
            string _hu = (ghu == null) ? "" : ghu.Value.Trim();
            var gin = m.Cast<Match>().Select(x => x.Groups["in"]).FirstOrDefault(y => y.Value != "");
            string _in = (gin == null) ? "" : gin.Value.Trim();

            int hundredthousands = Digit(_ht);
            hundredthousands = (hundredthousands == -1) ? (Regex.IsMatch(s, @"(?<ht>.*?)hundred(?=.*thousand)") ? 1 : 0) : hundredthousands;
            int thousands = Digit(_th);
            thousands = (thousands == -1) ? (Regex.IsMatch(s, "thousand") ? 1 : 0) : thousands;
            int hundreds = Digit(_hu);
            hundreds = (hundreds == -1) ? (Regex.IsMatch(s, @"hundred") ? 1 : 0) : hundreds;
            int numbers = Digit(_in);
            numbers = (numbers == -1) ? 0 : numbers;

            return (hundredthousands * 100000) + (thousands * 1000) + (hundreds * 100) + (numbers) * (s.Contains("minus") ? -1 : 1);
        }

        private static string Digit(int i)
        {
            List<string> ls1 = new List<string>() { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

            List<string> ls2 = new List<string>() { "zero", "ten", "twenty", "thirty", "fourty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            List<string> exc = new List<string>() { "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };

            if (i > 99 || i < 0) return null;
            else if (i < 10) return ls1[i];
            else if (i % 10 == 0) return ls2[i / 10];
            else if (exc.Count > i - 10) return exc[i - 10];
            else
            {
                int i1 = Int32.Parse(i.ToString().Substring(0, 1));
                int i2 = Int32.Parse(i.ToString().Substring(1, 1));
                return ls2[i1] + "-" + ls1[i2];
            }
        }

        public static string Number(int i)
        {
            if (i > 999999) return null;
            string s = i.ToString("000000");
            string digits = Digit(Int32.Parse(s.Substring(4)));
            string hundred = (s.Substring(3, 1) == "0") ? " " : Digit(Int32.Parse(s.Substring(3, 1)));
            string thousand = (s.Substring(1, 2) == "00") ? " " : Digit(Int32.Parse(s.Substring(1, 2)));
            string hundredthousands = (s.Substring(0, 1) == "0") ? " " : Digit(Int32.Parse(s.Substring(0, 1)));
            string finale = ((hundredthousands == " ") ? "" : hundredthousands + " hundred ")
                          + ((thousand == " ") ? "" : thousand + " thousand ")
                          + ((hundred == " ") ? "" : hundred + " hundred ")
                          + ((digits == "zero") ? "" : digits);
            return Regex.Replace(finale, " +", " ");
        }

        /// <summary>
        /// Convert each number in a sentence to an int
        /// </summary>
        /// <param name="s">"Fifteen minutes"</param>
        /// <returns>"15 minutes"</returns>
        public static string StringToInt(string s)
        {
            s = s.Replace('-', ' ');
            List<string> keeps = new List<string>()
                { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
                  "ten", "twenty", "thirty", "fourty", "fifty", "sixty", "seventy", "eighty", "ninety",
                  "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen",
                  "hundred", "thousand" };

            string i = "";
            string o = "";

            foreach (string w in s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (keeps.IndexOf(w) >= 0)
                    o += w + " ";
                else if (o != "")
                {
                    i += Number(o).ToString() + " " + w + " ";
                    o = "";
                }
                else
                    i += w + " ";
            }

            if (o != "") i += o + " ";
            return i.Trim();
        }

        /// <summary>
        /// Convert each int in a string to a number
        /// </summary>
        /// <param name="s">"15 minutes"</param>
        /// <returns>"Fifteen minutes"</returns>
        public static string IntToString(string s)
        {
            string i = Regex.Replace(s, "\\d+", delegate(Match m) { return Number(int.Parse(m.Value)); });
            return i[0] + i.Substring(1);
        }
    }
}
