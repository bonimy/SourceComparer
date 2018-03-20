// <copyright file="ValueTable.cs" company="Public Domain">
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

    public class ValueTable : IReadOnlyCollection<object>
    {
        public ValueTable(ColumnList columnList)
        {
            ColumnList = columnList ??
                throw new ArgumentNullException(nameof(columnList));
        }

        public ColumnList ColumnList
        {
            get;
        }

        public SourceList SourceList
        {
            get
            {
                return ColumnList.SourceList;
            }
        }

        public INameDictionary NameDictionary
        {
            get
            {
                return ColumnList.NameDictionary;
            }
        }

        public int Count
        {
            get
            {
                return SourceList.Count * ColumnList.Count;
            }
        }

        public IEnumerator<object> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<object>
        {
            public Enumerator(ValueTable valueTable)
            {
                ValueTable = valueTable ??
                    throw new ArgumentNullException(nameof(valueTable));

                NameEnumerator = ValueTable.NameDictionary.Keys.GetEnumerator();

                ColumnIndex = default;
                RowIndex = default;
                ObjectEnumerator = null;
                Current = null;
                InitializeRow = default;
            }

            public int ColumnIndex
            {
                get;
                set;
            }

            public string CurrentName
            {
                get
                {
                    return NameDictionary.Keys[ColumnIndex];
                }
            }

            public NameEntry CurrentNameEntry
            {
                get
                {
                    return NameDictionary.Entries[ColumnIndex];
                }
            }

            public ISource CurrentSource
            {
                get
                {
                    return SourceList[RowIndex];
                }
            }

            public Column CurrentColumn
            {
                get
                {
                    return ColumnList[ColumnIndex];
                }
            }

            public object Current
            {
                get;
                private set;
            }

            private ValueTable ValueTable
            {
                get;
            }

            private INameDictionary NameDictionary
            {
                get
                {
                    return ValueTable.NameDictionary;
                }
            }

            private SourceList SourceList
            {
                get
                {
                    return ValueTable.SourceList;
                }
            }

            private ColumnList ColumnList
            {
                get
                {
                    return ValueTable.ColumnList;
                }
            }

            private IEnumerator<string> NameEnumerator
            {
                get;
            }

            private int RowIndex
            {
                get;
                set;
            }

            private IEnumerator<object> ObjectEnumerator
            {
                get;
                set;
            }

            private bool InitializeRow
            {
                get;
                set;
            }

            public void Reset()
            {
                NameEnumerator.Reset();

                RowIndex = -1;
                ColumnIndex = -1;
                ObjectEnumerator = null;
                Current = null;
                InitializeRow = true;
            }

            public bool MoveNext()
            {
                _MoveNextRow:

                // Is this the start of a new row?
                if (InitializeRow)
                {
                    // Are we out of rows?
                    if (!NameEnumerator.MoveNext())
                    {
                        return false;
                    }

                    // Get the new row.
                    RowIndex++;
                    ColumnIndex = -1;
                    ObjectEnumerator = CurrentSource.GetEnumerator();
                    InitializeRow = false;
                }

                // Is this the last column?
                if (!ObjectEnumerator.MoveNext())
                {
                    // Attempt to get the next row.
                    InitializeRow = true;
                    goto _MoveNextRow;
                }

                Current = ObjectEnumerator.Current;
                ColumnIndex++;
                return true;
            }

            void IDisposable.Dispose()
            {
            }
        }
    }
}
