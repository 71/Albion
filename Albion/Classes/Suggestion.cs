using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Albion
{
    public class Suggestion
    {
        internal Suggestion(string input, Sentence s)
        {
            string tested = s.Template;
            if (tested.Contains("{") && tested.Contains("}") && input.IndexOf(tested.Substring(0, tested.IndexOf('{'))) >= 0)
            {
                string i = "(" + Regex.Replace(tested.ToLower(), @"{[\d\w]+}", "|").Trim('|') + @")|([^\1])";
                input = Regex.Replace(input, i, "$1|", RegexOptions.IgnoreCase);
                input = Regex.Replace(input, @"\|+", "\\?");
                if (input == "\\?" || input == "") return;
            }

            var param = s.Method.GetParameters();
            tested = Regex.Replace(tested, @"{([\d\w]+)}", (m) =>
            {
                var p = param.FirstOrDefault(x => x.Name.ToLower().Equals(m.Groups[1].Value.ToLower()));
                return p == null ? "?" : String.Format("[{0}]", p.ParameterType.Name);
            });
            var r = Regex.Match(tested, "(.*?)(" + input + ")(.*)", RegexOptions.IgnoreCase);
            Full = r.Groups[1].Value + r.Groups[2].Value + r.Groups[3].Value;
            Result = new string[] { r.Groups[1].Value, r.Groups[2].Value, r.Groups[3].Value };
            Matched = r.Groups[2].Value;
            Input = input;
            Description = s.Attr.Description;
            Sentence = s.Template;
        }

        public string Full { get; private set; }
        public string[] Result { get; private set; }
        public string Matched { get; private set; }
        public string Input { get; private set; }
        public string Description { get; private set; }
        public string Sentence { get; private set; }

        public override string ToString()
        {
            return this.Full;
        }
    }
}
