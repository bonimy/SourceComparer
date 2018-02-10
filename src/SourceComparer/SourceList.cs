// <copyright file="SourceList.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

// Redo source output to be an SNR inequality instead of SNR bin.
// Output ds9 region files for source matches with SNR > 20 and distance > 5".

namespace SourceComparer
{
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

        public SourceList(
            string[] lines,
            bool multithreaded,
            bool verbose)
        {
            var parser = new SourceListParser(
                lines,
                multithreaded,
                verbose);

            NameDictionary = parser.NameDictionary;
            Sources = parser.Sources;
        }

        internal SourceList(
            INameDictionary nameList,
            IEnumerable<ISource> sources)
        {
            NameDictionary = nameList ??
                throw new ArgumentNullException(nameof(nameList));

            Sources = new List<ISource>(sources);
        }

        public Column GetColumn(string name)
        {
            var entries = NameDictionary.Entries;
            var index = NameDictionary[name];
            var entry = entries[index];

            return GetColumn(entry);
        }

        public Column GetColumn(NameEntry entry)
        {
            if (entry is null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            var name = entry.Name;
            var index = NameDictionary[name];
            var values = new object[Count];

            for (var i = 0; i < values.Length; i++)
            {
                var source = this[i];
                values[i] = source[index];
            }

            return new Column(entry, values);
        }

        public SourceList Filter(SourceFilterCallback filter)
        {
            if (filter is null)
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

            return new SourceList(NameDictionary, sources);
        }

        public SourceList FilterBy(
            SourceList other,
            SourceComparisonCallback comparison)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (comparison is null)
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

            return new SourceList(NameDictionary, sources);
        }

        public IReadOnlyDictionary<double, IReadOnlyCollection<ISource>> CreateBins(
            double startSnr,
            double width)
        {
            var comparer = new BinComparer(startSnr, width / 2);
            var dictionary = new Dictionary<double, IReadOnlyCollection<ISource>>(
                Count,
                comparer);

            foreach (var source in this)
            {
                var snrSource = source as ISnrSource;
                var snr = snrSource.SignalToNoise;
                var center = comparer.GetBinCenter(snr);
                if (dictionary.TryGetValue(center, out var list))
                {
                    var collection = list as ICollection<ISnrSource>;
                    collection.Add(snrSource);
                    continue;
                }

                list = new List<ISnrSource>() { snrSource };
                dictionary.Add(center, list);
            }

            return dictionary;
        }

        public IEnumerator<ISource> GetEnumerator()
        {
            return Sources.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class SourceListParser
        {
            private static readonly IReadOnlyDictionary<string, ColumnFormat> ColumnFormatDictionary = new Dictionary<string, ColumnFormat>(
                StringComparer.OrdinalIgnoreCase)
            {
                { "i", ColumnFormat.Integer },
                { "int", ColumnFormat.Integer },
                { "r", ColumnFormat.Double },
                { "double", ColumnFormat.Double },
                { "c", ColumnFormat.String },
                { "char", ColumnFormat.String },
            };

            private static readonly IReadOnlyDictionary<string, Unit> UnitDictionary = new Dictionary<string, Unit>(
                StringComparer.OrdinalIgnoreCase)
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

            private bool Verbose
            {
                get;
            }

            private bool Multithreaded
            {
                get;
            }

            private string[] Lines
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

                        if (line.StartsWith("\\"))
                        {
                            continue;
                        }

                        if (Verbose)
                        {
                            Console.WriteLine(
                                "Start line for column entry info: {0}",
                                i + 1);
                        }

                        return i;
                    }

                    throw new ArgumentException("Could not get start line.", nameof(Lines));
                }
            }

            private int LineNumber
            {
                get;
                set;
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

            public SourceListParser(
                string[] lines,
                bool multithreaded,
                bool verbose)
            {
                // Assign input parameters.
                Lines = lines ??
                    throw new ArgumentNullException(nameof(lines));

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

                var sources = new ISource[Lines.Length - LineNumber];
                MultithreadHelper.For(
                    Multithreaded,
                    0,
                    sources.Length,
                    Iteration);

                Sources = sources;

                if (verbose)
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        "Total rows: {0}",
                        Sources.Count);
                }

                void Iteration(int index)
                {
                    sources[index] = ReadSource(Lines[index + LineNumber]);
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

                MultithreadHelper.For(
                    Multithreaded,
                    0,
                    names.Length,
                    Iteration);

                return names;

                void Iteration(int index)
                {
                    var start = Indexes[index] + 1;
                    var end = Math.Min(
                        Indexes[index + 1],
                        line.Length);

                    var length = end - start;

                    var text = line.Substring(start, length);
                    names[index] = text.Trim();
                }
            }

            private IReadOnlyList<ColumnFormat> GetFormats(string line)
            {
                var names = GetNames(line);

                var formats = new ColumnFormat[names.Count];
                MultithreadHelper.For(
                    Multithreaded,
                    0,
                    formats.Length,
                    Iteration);

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
                MultithreadHelper.For(
                    Multithreaded,
                    0,
                    units.Length,
                    Iteration);

                return units;

                void Iteration(int index)
                {
                    units[index] = GetUnitFormat(names[index]);
                }
            }

            private INameDictionary CreateNameDictionary()
            {
                var entries = new NameEntry[Names.Count];
                MultithreadHelper.For(
                    Multithreaded,
                    0,
                    entries.Length,
                    Iteration);

                if (Verbose)
                {
                    Console.WriteLine(
                        "Number of column entries: {0}",
                        entries.Length);

                    Console.WriteLine();
                    Console.WriteLine(
                        "Begin reading rows: Line {0}",
                        LineNumber + 1);
                }

                return CreateNameDictionary(entries);

                void Iteration(int index)
                {
                    var name = Names[index];
                    var format = Formats[index];
                    var unit = Units[index];
                    var nullSpecifier = NullSpecifiers[index];

                    entries[index] = CreateNameEntry(
                        name,
                        format,
                        unit,
                        nullSpecifier);
                }
            }

            private NameEntry CreateNameEntry(
                string name,
                ColumnFormat format,
                Unit unit,
                string nullSpecifier)
            {
                if (unit == Unit.Degrees || unit == Unit.ArcSeconds)
                {
                    format = ColumnFormat.Angle;
                }

                return new NameEntry(
                    name,
                    format,
                    unit,
                    nullSpecifier);
            }

            private INameDictionary CreateNameDictionary(NameEntry[] entries)
            {
                // HACK: fix this
                if (entries.Length == 8)
                {
                    return new MdetNameDictionary(entries);
                }

                if (entries.Length == 10)
                {
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
                        throw new ArgumentException(
                            "Name dictionary is of an unrecognized type.");
                }
            }

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
        }
    }
}
