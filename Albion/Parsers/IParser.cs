using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albion.Parsers
{
    public interface IParser
    {
        int Coeff { get; }
        Type ParseTo { get; }
        IEnumerable<string> Examples { get; }

        bool TryParse(string s, out object res);
        string RandomExample();
    }

    public abstract class TypeParser<T> : IParser
    {
        private static Random ExampleChooser = new Random();

        public Type ParseTo { get { return typeof(T); } }
        public abstract IEnumerable<string> Examples { get; }
        public abstract int Coeff { get; }

        public bool TryParse(string s, out object res)
        {
            T r = default(T);
            bool b = TryParse(s, out r);
            res = r;
            return b;
        }

        protected abstract bool TryParse(string s, out T res);

        public string RandomExample()
        {
            return Examples.ElementAt(ExampleChooser.Next(Examples.Count()));
        }
    }
}
