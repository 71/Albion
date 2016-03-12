using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Albion
{
    public class Sentence
    {
        internal Sentence(MethodInfo m, SentenceAttribute a, int i, object b = null)
        {
            var ex = m.DeclaringType.GetTypeInfo().GetCustomAttribute<ExtensionAttribute>();

            Base = b;
            Method = m;
            Variables = Regex.Matches(Template, @"{[\d\w]+}").Cast<Match>().Select(x => x.Value.Substring(1, x.Value.Length - 2));
            Template = a.Sentence[i];

            Language = a.Language == "" ? ex.Language : a.Language;
            SentenceID = a.ID;
            ExtensionID = ex.ID;
            Description = a.Description;
        }

        internal object Base { get; private set; }
        internal MethodInfo Method { get; private set; }
        internal IEnumerable<string> Variables { get; private set; }

        public string Template { get; private set; }
        public string Language { get; private set; }
        public string SentenceID { get; private set; }
        public string ExtensionID { get; private set; }
        public string Description { get; private set; }

        public override string ToString()
        {
            return this.Template;
        }
    }
}
