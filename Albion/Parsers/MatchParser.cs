using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Albion.Parsers
{
#pragma warning disable 1591
    [PhraseParser]
    public class MatchParser : TypeParser<Match>
    {
        public override int Coeff { get { return 50; } }

        public Regex Regex { get; private set; }

        public MatchParser(Regex rgx)
        {
            Regex = rgx;
        }

        public override IEnumerable<string> Examples
        {
            get
            {
                yield return "anything";
            }
        }

        protected override bool TryParse(string s, out Match res)
        {
            res = Regex.Match(s);
            return true;
        }
    }
}
