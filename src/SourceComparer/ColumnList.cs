// <copyright file="ColumnList.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SourceComparer
{
    public class ColumnList : IReadOnlyList<Column>
    {
        private IReadOnlyList<Column> Columns
        {
            get;
        }

        public int Count
        {
            get
            {
                return Columns.Count;
            }
        }

        public SourceList SourceList
        {
            get;
        }

        public INameDictionary NameDictionary
        {
            get
            {
                return SourceList.NameDictionary;
            }
        }

        public Column this[int index]
        {
            get
            {
                return Columns[index];
            }
        }

        public ColumnList(SourceList sourceList, bool multithreaded, bool verbose)
        {
            SourceList = sourceList ??
                throw new ArgumentNullException(nameof(sourceList));

            var keys = NameDictionary.Keys;
            var verboseOutputPadding = 0;
            if (verbose)
            {
                foreach (var key in keys)
                {
                    if (verboseOutputPadding < key.Length)
                    {
                        verboseOutputPadding = key.Length;
                    }
                }

                verboseOutputPadding += 2;
            }

            var columns = new Column[NameDictionary.Count];
            MultithreadHelper.For(
                multithreaded,
                0,
                columns.Length,
                Iteration);

            Columns = columns;

            void Iteration(int index)
            {
                var name = NameDictionary.Keys[index];
                var entry = NameDictionary.Entries[index];
                var column = SourceList.GetColumn(name);
                columns[index] = column;

                if (verbose)
                {
                    var sb = new StringBuilder("Added column \"");
                    sb.Append(index);
                    sb.Append('"');

                    if (entry.Units != Unit.None)
                    {
                        for (var i = name.Length; i < verboseOutputPadding; i++)
                        {
                            sb.Append(' ');
                        }

                        sb.Append('(');
                        sb.Append(entry.Units);
                        sb.Append(')');
                    }

                    Console.WriteLine(sb);
                }
            }
        }

        public IEnumerator<Column> GetEnumerator()
        {
            return Columns.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
