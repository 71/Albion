using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albion.Attributes
{
    /// <summary>
    /// Indicates that this class can parse strings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ParserAttribute : Attribute
    {
        public ParserAttribute()
        {

        }
    }
}
