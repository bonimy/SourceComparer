// <copyright file="SourceList.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SourceComparer
{
    public delegate bool SourceFilter(ISource source);

    public delegate bool SourceComparison(ISource source, SourceList other);

    public sealed class SourceList : IReadOnlyList<ISource>
    {
        public INameDictionary NameDictionary
        {
            get;
        }

        private IReadOnlyList<ISource> Sources
        {
            get;
        }

        public int Count
        {
            get
            {
                return Sources.Count;
            }
        }

        public ISource this[int index]
        {
            get
            {
                return Sources[index];
            }
        }

        public SourceList(string[] lines, bool multithreaded, bool verbose)
        {
            var parser = new SourceListParser(lines, multithreaded, verbose);

            NameDictionary = parser.NameDictionary;
            Sources = parser.Sources;
        }

        private SourceList(INameDictionary nameList, IReadOnlyList<ISource> sources)
        {
            NameDictionary = nameList ?? throw new ArgumentNullException(nameof(nameList));
            Sources = sources ?? throw new ArgumentNullException(nameof(sources));
        }

        public Column GetColumn(string name)
        {
            return GetColumn(NameDictionary.Entries[NameDictionary[name]]);
        }

        public Column GetColumn(NameEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            var name = entry.Name;
            var index = NameDictionary[name];
            var values = new object[Count];
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = this[i][index];
            }

            return new Column(entry, values);
        }

        public SourceList Filter(SourceFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var sources = new List<ISource>(Count);
            for (var i = 0; i < Count; i++)
            {
                var source = this[i];
                if (filter(source))
                {
                    sources.Add(source);
                }
            }

            return new SourceList(NameDictionary, sources.ToArray());
        }

        public SourceList FilterBy(SourceList other, SourceComparison comparison)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (comparison == null)
            {
                throw new ArgumentNullException(nameof(comparison));
            }

            var sources = new List<ISource>(Count);
            for (var i = 0; i < Count; i++)
            {
                var source = this[i];
                if (comparison(source, other))
                {
                    sources.Add(source);
                }
            }

            return new SourceList(NameDictionary, sources.ToArray());
        }

        public IEnumerator<ISource> GetEnumerator()
        {
            return Sources.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class SourceListParser
        {
            private static readonly Dictionary<string, ColumnFormat> ColumnFormatDictionary = new Dictionary<string, ColumnFormat>(StringComparer.OrdinalIgnoreCase)
            {
                { "i", ColumnFormat.Integer },
                { "int", ColumnFormat.Integer },
                { "r", ColumnFormat.Double },
                { "double", ColumnFormat.Double },
                { "c", ColumnFormat.String },
                { "char", ColumnFormat.String },
            };

            private static readonly IReadOnlyDictionary<string, Unit> UnitDictionary = new Dictionary<string, Unit>(StringComparer.OrdinalIgnoreCase)
            {
                { String.Empty, Unit.None },
                { "-", Unit.None },
                { "--", Unit.None },
                { "deg", Unit.Degrees },
                { "degrees", Unit.Degrees },
                { "asec", Unit.ArcSeconds },
                { "arcsec", Unit.ArcSeconds },
                { "pix", Unit.Pixel },
                { "dn", Unit.DimenionslessNumber },
                { "mag", Unit.Magnitude },
                { "MJD", Unit.ModifiedJulianDate },
                { "days", Unit.ModifiedJulianDate },
                { "%", Unit.Percent },
                { "asecpyr", Unit.ArcSecondsPerYear },
                { "ujy", Unit.MicroJansky },
            };

            private static ColumnFormat GetColumnFormat(string text)
            {
                if (ColumnFormatDictionary.TryGetValue(text, out var result))
                {
                    return result;
                }

                throw new ArgumentException("Could not match format: " + text);
            }

            private static Unit GetUnitFormat(string text)
            {
                if (UnitDictionary.TryGetValue(text, out var result))
                {
                    return result;
                }

                throw new ArgumentException("Could not match format: " + text);
            }

            private string[] Lines
            {
                get;
            }

            private bool Multithreaded
            {
                get;
            }

            private int StartLine
            {
                get
                {
                    for (var i = 0; i < Lines.Length; i++)
                    {
                        var line = Lines[i];

                        if (!line.StartsWith("\\"))
                        {
                            if (Verbose)
                            {
                                Console.WriteLine("Start line for column entry info: {0}", i + 1);
                            }

                            return i;
                        }
                    }

                    throw new ArgumentException("Could not get start line.", nameof(Lines));
                }
            }

            private int LineNumber
            {
                get;
                set;
            }

            private bool Verbose
            {
                get;
            }

            public INameDictionary NameDictionary
            {
                get;
            }

            public IReadOnlyList<ISource> Sources
            {
                get;
            }

            private IReadOnlyList<int> Indexes
            {
                get;
            }

            private IReadOnlyList<string> Names
            {
                get;
            }

            private IReadOnlyList<ColumnFormat> Formats
            {
                get;
            }

            private IReadOnlyList<Unit> Units
            {
                get;
            }

            private IReadOnlyList<string> NullSpecifiers
            {
                get;
            }

            public SourceListParser(string[] lines, bool multithreaded, bool verbose)
            {
                // Assign input parameters.
                Lines = lines ?? throw new ArgumentNullException(nameof(lines));
                Verbose = verbose;

                Multithreaded = multithreaded;

                // Get start line for name list.
                LineNumber = StartLine;

                Indexes = GetIndexes(Lines[LineNumber]);
                Names = GetNames(Lines[LineNumber++]);
                Formats = GetFormats(Lines[LineNumber++]);
                Units = GetUnits(Lines[LineNumber++]);

                NullSpecifiers = Lines[LineNumber].StartsWith("|") ?
                    GetNames(Lines[LineNumber++]) :
                    new string[Names.Count];

                NameDictionary = CreateNameDictionary();

                var lastThousdand = 0;

                var count = 0;
                var sources = new ISource[Lines.Length - LineNumber];
                if (Multithreaded)
                {
                    Parallel.For(0, sources.Length, Iteration);
                }
                else
                {
                    for (var i = 0; i < sources.Length; i++)
                    {
                        Iteration(i);
                    }
                }

                Sources = sources;

                if (verbose)
                {
                    Console.WriteLine();
                    Console.WriteLine("Total rows: {0}", Sources.Count);
                }

                void Iteration(int index)
                {
                    sources[index] = ReadSource(Lines[index + LineNumber]);

                    count++;

                    if (verbose && count - lastThousdand > 1000)
                    {
                        lastThousdand = count - (count % 1000);
                        Console.WriteLine("Successfully read {0} rows", lastThousdand);
                    }
                }
            }

            private static IReadOnlyList<int> GetIndexes(string line)
            {
                var list = new List<int>();
                for (var i = 0; i < line.Length; i++)
                {
                    if (line[i] == '|')
                    {
                        list.Add(i);
                    }
                }

                return list;
            }

            private IReadOnlyList<string> GetNames(string line)
            {
                var names = new string[Indexes.Count - 1];

                if (Multithreaded)
                {
                    Parallel.For(0, names.Length, Iteration);
                }
                else
                {
                    for (var i = 0; i < names.Length; i++)
                    {
                        Iteration(i);
                    }
                }

                return names;

                void Iteration(int index)
                {
                    var start = Indexes[index] + 1;
                    var end = Math.Min(Indexes[index + 1], line.Length);
                    var length = end - start;

                    var text = line.Substring(start, length).Trim();
                    names[index] = text;
                }
            }

            private IReadOnlyList<ColumnFormat> GetFormats(string line)
            {
                var names = GetNames(line);

                var formats = new ColumnFormat[names.Count];
                if (Multithreaded)
                {
                    Parallel.For(0, formats.Length, Iteration);
                }
                else
                {
                    for (var i = 0; i < formats.Length; i++)
                    {
                        Iteration(i);
                    }
                }

                return formats;

                void Iteration(int index)
                {
                    formats[index] = GetColumnFormat(names[index]);
                }
            }

            private IReadOnlyList<Unit> GetUnits(string line)
            {
                var names = GetNames(line);

                var units = new Unit[names.Count];
                if (Multithreaded)
                {
                    Parallel.For(0, units.Length, Iteration);
                }
                else
                {
                    for (var i = 0; i < units.Length; i++)
                    {
                        Iteration(i);
                    }
                }

                return units;

                void Iteration(int index)
                {
                    units[index] = GetUnitFormat(names[index]);
                }
            }

            private INameDictionary CreateNameDictionary()
            {
                var entries = new NameEntry[Names.Count];
                if (Multithreaded)
                {
                    Parallel.For(0, entries.Length, Iteration);
                }
                else
                {
                    for (var i = 0; i < entries.Length; i++)
                    {
                        Iteration(i);
                    }
                }

                if (Verbose)
                {
                    Console.WriteLine("Number of column entries: {0}", entries.Length);
                }

                if (Verbose)
                {
                    Console.WriteLine();
                    Console.WriteLine("Begin reading rows: Line {0}", LineNumber + 1);
                }

                // HACK: fix this
                switch (entries.Length)
                {
                case 8:
                return new MdetNameDictionary(entries);

                case 10:
                return new SpitzerNameDictionary(entries);
                }

                // Wphot tables have variable header sizes.
                try
                {
                    // Try to create an mdex dictionary.
                    return new MdexNameDictionary(entries);
                }
                catch
                {
                    // If we fail, default to normal name dictionary.
                    return new NameDictionary(entries);
                }

                void Iteration(int index)
                {
                    var unit = Units[index];
                    var format = Formats[index];

                    if (format == ColumnFormat.Double)
                    {
                        switch (unit)
                        {
                        case Unit.ModifiedJulianDate:
                        entries[index] = new NameEntry(
                            Names[index],
                            ColumnFormat.ModifiedJulianDate,
                            unit,
                            NullSpecifiers[index]);
                        return;

                        case Unit.Degrees:
                        entries[index] = new NameEntry(
                            Names[index],
                            ColumnFormat.Angle,
                            unit,
                            NullSpecifiers[index]);
                        return;

                        case Unit.ArcSeconds:
                        entries[index] = new NameEntry(
                            Names[index],
                            ColumnFormat.Angle,
                            unit,
                            NullSpecifiers[index]);
                        return;
                        }
                    }

                    entries[index] = new NameEntry(
                        Names[index],
                        Formats[index],
                        Units[index],
                        NullSpecifiers[index]);
                }
            }

            private ISource ReadSource(string line)
            {
                var values = GetNames(line);

                switch (NameDictionary)
                {
                case MdetNameDictionary mdetNameDictionary:
                return new MdetSource(mdetNameDictionary, values);

                case MdexNameDictionary mdexNameDictionary:
                return new MdexSource(mdexNameDictionary, values);

                case SpitzerNameDictionary spitzerNameDictionary:
                return new SpitzerSource(spitzerNameDictionary, values);

                default:
                throw new ArgumentException();
                }
            }
        }
    }
}
