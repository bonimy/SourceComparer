// <copyright file="Column.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Sources
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class Column : IReadOnlyList<object>
    {
        public Column(NameEntry name, IReadOnlyList<object> values)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }

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

        public int Count
        {
            get
            {
                return Values.Count;
            }
        }

        private IReadOnlyList<object> Values
        {
            get;
        }

        public object this[int index]
        {
            get
            {
                return Values[index];
            }
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
