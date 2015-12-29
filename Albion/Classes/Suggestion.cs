using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Albion
{
    public class Suggestion
    {
        public bool Matched { get; set; }

        public string Before { get; set; }
        public string Match { get; set; }
        public string After { get; set; }

        public Sentence Sentence { get; set; }
        public string Input { get; set; }

        internal Suggestion(string input, SmartSentence s)
        {
            Matched = false;
            Before = "";
            Match = "";
            After = "";
            Sentence = s.Sentence;
            Input = input;

            int i = 0;
            while (i < input.Length)
            {
                string match = "";

                for (int o = i; o < input.Length; o++)
                {
                    if ()
                    {

                    }
                    else if (char.ToLower(input[o]) == )
                    {
                        match += input[o];
                    }
                    else
                    {

                    }
                }
            }
            using (StringReader sr = new StringReader(input))
            {
                char c;

                for (int i = 0; i < input.Length; i++)
                {

                }
                while ((c = (char)sr.Read()) != char.MaxValue)
                {

                }
            }
        }

        internal Suggestion(string input, Sentence s)
        {
            string tested = s.Template.ToLower();
            bool cont = false;

            int i = 0;

            // Attempt to find the beginning of the match
            for (; i < s.Template.Length; i++)
            {
                if (char.ToLower(input[0]) == tested[i])
                {
                    cont = true;
                    break;
                }
            }

            if (!cont)
            {
                Matched = false;
                return;
            }
            else
            {
                for (; i < s.Template.Length; i++)
                {
                    char c = s.Template[i];

                    if (char.ToLower(input[0]) == c)
                    {
                        cont = true;
                        break;
                    }
                }
            }


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

        public override string ToString()
        {
            return this.Matched ? this.Before + "{" + this.Match + "}" + this.After : "[No match]";
        }

        public string Format(string format)
        {
            return string.Format(format, Before, Match, After);
        }
    }
}
