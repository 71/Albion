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
        internal Sentence(MethodInfo m, SentenceAttribute a, int i)
        {
            Template = a.Sentence[i];
            Method = m;
            Attr = a;
            ExAttr = (ExtensionAttribute)m.DeclaringType.GetTypeInfo().GetCustomAttribute<ExtensionAttribute>();
            Variables = Regex.Matches(Template, @"{[\d\w]+}").Cast<Match>().Select(x => x.Value.Substring(1, x.Value.Length - 2));
            Base = null;
        }

        internal Sentence(MethodInfo m, SentenceAttribute a, int i, object b)
        {
            Template = a.Sentence[i];
            Method = m;
            Attr = a;
            ExAttr = (ExtensionAttribute)m.DeclaringType.GetTypeInfo().GetCustomAttribute<ExtensionAttribute>();
            Variables = Regex.Matches(Template, @"{[\d\w]+}").Cast<Match>().Select(x => x.Value.Substring(1, x.Value.Length - 2));
            Base = b;
        }

        public string Template { get; private set; }
        public MethodInfo Method { get; private set; }
        public SentenceAttribute Attr { get; private set; }
        public ExtensionAttribute ExAttr { get; private set; }
        public IEnumerable<string> Variables { get; private set; }
        public object Base { get; private set; }

        public override string ToString()
        {
            return this.Template;
        }
    }
}
