using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albion
{
    /// <summary>
    /// Dictionary used as dynamic by the <see cref="SentenceBuilder.Handler(Action{dynamic})"/> method.
    /// </summary>
    public class DynamicDictionary : DynamicObject
    {
        private readonly Dictionary<string, object> dictionary;

        internal DynamicDictionary(Dictionary<string, object> dictionary)
        {
            this.dictionary = dictionary;
        }

        /// <summary>
        /// Get a member of the sentence.
        /// </summary>
        /// <returns>Result of <see cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return dictionary.TryGetValue(binder.Name, out result);
        }

        /// <summary>
        /// Fail to set a value.
        /// </summary>
        /// <returns>false</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return false;
        }
    }
}
