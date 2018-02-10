// <copyright file="Column.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace SourceComparer
{
    public class Column : IReadOnlyList<object>
    {
        public NameEntry Name
        {
            get;
        }

        public ColumnFormat Format
        {
            get
            {
                return Name.Format;
            }
        }

        public Unit Units
        {
            get
            {
                return Name.Units;
            }
        }

        private IReadOnlyList<object> Values
        {
            get;
        }

        public int Count
        {
            get
            {
                return Values.Count;
            }
        }

        public object this[int index]
        {
            get
            {
                return Values[index];
            }
        }

        public Column(NameEntry name, IReadOnlyList<object> values)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public Column Filter(ColumnFilterCallback filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var values = new List<object>(Count);
            foreach (var source in this)
            {
                if (filter(source))
                {
                    values.Add(source);
                }
            }

            return new Column(Name, values);
        }

        public Column FilterBy(Column other, ColumnComparisonCallback comparison)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (comparison is null)
            {
                throw new ArgumentNullException(nameof(comparison));
            }

            var values = new List<object>(Count);
            foreach (var source in this)
            {
                if (comparison(source, other))
                {
                    values.Add(source);
                }
            }

            return new Column(Name, values);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
