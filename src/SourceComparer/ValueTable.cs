// <copyright file="ValueTable.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections.Generic;

namespace SourceComparer
{
    public class ValueTable : IReadOnlyCollection<object>
    {
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

        public ValueTable(ColumnList columnList)
        {
            ColumnList = columnList ?? throw new ArgumentNullException(nameof(columnList));
        }

        public IEnumerator<object> GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<object>
        {
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

            private int RowIndex
            {
                get;
                set;
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

            private IEnumerator<object> ObjectEnumerator
            {
                get;
                set;
            }

            public object Current
            {
                get;
                private set;
            }

            private bool InitializeRow
            {
                get;
                set;
            }

            public Enumerator(ValueTable valueTable)
            {
                ValueTable = valueTable ?? throw new ArgumentNullException(nameof(valueTable));

                NameEnumerator = ValueTable.NameDictionary.Keys.GetEnumerator();

                ColumnIndex = default(int);
                RowIndex = default(int);
                ObjectEnumerator = null;
                Current = null;
                InitializeRow = default(bool);
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

            public void Dispose()
            {
            }
        }
    }
}
