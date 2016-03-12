using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albion
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class SentenceAttribute : Attribute
    {
        public SentenceAttribute(params string[] formattedSentences)
        {
            sentences = formattedSentences;
        }

        protected string[] sentences;
        public string[] Sentences { get { return this.sentences; } }

        protected string id = "";
        public string ID { get { return this.id; } set { this.id = value; } }

        protected string descr = "";
        public string Description { get { return this.descr; } set { this.descr = value; } }

        protected string lang = "en";
        public string Language { get { return this.lang; } set { this.lang = value; } }
    }
}
