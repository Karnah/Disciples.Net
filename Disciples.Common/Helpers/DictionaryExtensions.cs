using System.Collections.Generic;

namespace Disciples.Common.Helpers
{
    /// <summary>
    /// Набор методов для работы со словарём.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Добавить в словарь значение, если оно еще не было добавлено.
        /// </summary>
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                return false;

            dictionary.Add(key, value);
            return true;
        }
    }
}