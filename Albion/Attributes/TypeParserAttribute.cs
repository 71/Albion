using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albion.Attributes
{
    /// <summary>
    /// Indicates that this class can parse phrases of a sentence.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TypeParserAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new <see cref="TypeParserAttribute"/>.
        /// </summary>
        public TypeParserAttribute() { }
    }
}
