// <copyright file="NameDictionary.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace SourceComparer
{
    public class NameDictionary : INameDictionary
    {
        private IReadOnlyDictionary<string, int> Names
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
                return Names.Values;
            }
        }

        public int Count
        {
            get
            {
                return Names.Count;
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
                return Names[key];
            }
        }

        public NameDictionary(IReadOnlyList<NameEntry> entries)
        {
            if (entries is null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            var keys = new string[entries.Count];
            var names = new Dictionary<string, int>(entries.Count, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var name = entry.Name;
                keys[i] = name;
                names.Add(name, i);
            }

            Entries = entries;
            Keys = keys;
            Names = names;
        }

        public bool ContainsKey(string key)
        {
            return Names.ContainsKey(key);
        }

        public bool TryGetValue(string key, out int value)
        {
            return Names.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
        {
            return Names.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
