using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Albion
{
    /// <summary>
    /// Dictionary used as <see langword="dynamic"/> by the
    /// <see cref="SentenceBuilder.Handler(Action{object})"/> method.
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
        /// Fails to set a value.
        /// </summary>
        /// <returns>false</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value) => false;
    }
}
