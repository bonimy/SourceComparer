// <copyright file="NameDictionary.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections.Generic;

namespace SourceComparer
{
    public class NameDictionary : INameDictionary
    {
        private IReadOnlyDictionary<string, int> Base
        {
            get;
        }

        public IReadOnlyList<string> Keys
        {
            get;
        }

        IEnumerable<string> IReadOnlyDictionary<string, int>.Keys
        {
            get
            {
                return Keys;
            }
        }

        public IEnumerable<int> Values
        {
            get
            {
                return Base.Values;
            }
        }

        public int Count
        {
            get
            {
                return Base.Count;
            }
        }

        public IReadOnlyList<NameEntry> Entries
        {
            get;
        }

        public int this[string key]
        {
            get
            {
                return Base[key];
            }
        }

        public NameDictionary(IReadOnlyList<NameEntry> names)
        {
            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            var keys = new string[names.Count];
            var dictionary = new Dictionary<string, int>(names.Count, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < names.Count; i++)
            {
                var name = names[i];
                var key = name.Name;
                keys[i] = key;
                dictionary.Add(key, i);
            }

            Entries = names;
            Keys = keys;
            Base = dictionary;
        }

        public bool ContainsKey(string key)
        {
            return Base.ContainsKey(key);
        }

        public bool TryGetValue(string key, out int value)
        {
            return Base.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
        {
            return Base.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
