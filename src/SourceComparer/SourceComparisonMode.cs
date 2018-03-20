// <copyright file="SourceComparisonMode.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace SourceComparer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Helper;
    using Sources;
    using Wcs;
    using static SourceComparer.Program;
    using static System.Math;

    public class SourceComparisonMode : IProcess
    {
        public SourceComparisonMode(CommandSwitches commandSwitches)
        {
            CommandSwitches = commandSwitches ??
                throw new ArgumentNullException(nameof(commandSwitches));

            // This information is already printed to the output stream.
            Multithreaded = CommandSwitches.Multithreaded;
            Verbose = CommandSwitches.Verbose;
            TimeStamp = CommandSwitches.TimeStamp;
            LogPath = CommandSwitches.LogPath;

            Console.WriteLine();
            Console.WriteLine("Source Comparison Mode:");

            FilteredPath = ReadFilteredPath();
            UnfilteredPath = ReadUnfilteredPath();
            SpitzerPath = ReadSpitzerPath();
            OutputPath = ReadOutputPath();
            SearchRadius = ReadSearchRadius();
            Color1 = ReadColor1();
            Color2 = ReadColor2();
            Color3 = ReadColor3();
        }

        public CommandSwitches CommandSwitches
        {
            get;
        }

        public bool Multithreaded
        {
            get;
        }

        public bool Verbose
        {
            get;
        }

        public bool TimeStamp
        {
            get;
        }

        public string LogPath
        {
            get;
        }

        public string FilteredPath
        {
            get;
        }

        public string UnfilteredPath
        {
            get;
        }

        public string SpitzerPath
        {
            get;
        }

        public string OutputPath
        {
            get;
        }

        public Angle SearchRadius
        {
            get;
        }

        public string Color1
        {
            get;
        }

        public string Color2
        {
            get;
        }

        public string Color3
        {
            get;
        }

        private int Status
        {
            get;
            set;
        }

        private SourceList Filtered
        {
            get;
            set;
        }

        private SourceList Unfiltered
        {
            get;
            set;
        }

        private SourceList Spitzer
        {
            get;
            set;
        }

        private SourceComparisonDivider FilteredDivided
        {
            get;
            set;
        }

        private SourceComparisonDivider UnfilteredDivided
        {
            get;
            set;
        }

        public int Run()
        {
            if (ReadSourceLists() != 0)
            {
                return Status;
            }

            if (FilterSourceLists() != 0)
            {
                return Status;
            }

            var filteredMatchLists = new SourceMatchLists(
                Filtered,
                Spitzer,
                SearchRadius,
                Verbose);

            var unfilteredMatchLists = new SourceMatchLists(
                Unfiltered,
                Spitzer,
                SearchRadius,
                Verbose);

            var filteredCompleteness = GetFractionDictionary(
                filteredMatchLists.SecondaryMatchedSources,
                Spitzer);

            var unfilteredCompleteness = GetFractionDictionary(
                unfilteredMatchLists.SecondaryMatchedSources,
                Spitzer);

            var filteredReliability = GetFractionDictionary(
                filteredMatchLists.PrimaryMatchedSources,
                Filtered);

            var unfilteredReliability = GetFractionDictionary(
                unfilteredMatchLists.PrimaryMatchedSources,
                Unfiltered);

            File.WriteAllText(
                "filtered-completeness.txt",
                GetCompletenessTable(filteredCompleteness));

            File.WriteAllText(
                "unfiltered-completeness.txt",
                GetCompletenessTable(unfilteredCompleteness));

            File.WriteAllText(
                "filtered-reliability.txt",
                GetCompletenessTable(filteredReliability));

            File.WriteAllText(
                "unfiltered-reliability.txt",
                GetCompletenessTable(unfilteredReliability));

            return Status;
        }

        public int CompareFilteredAndUnfiltered()
        {
            if (ReadSourceLists() != 0)
            {
                return Status;
            }

            if (FilterSourceLists() != 0)
            {
                return Status;
            }

            var filtered = new SourceMatchLists(
                Filtered,
                Spitzer,
                SearchRadius,
                Verbose);

            var unfiltered = new SourceMatchLists(
                Unfiltered,
                Spitzer,
                SearchRadius,
                Verbose);

            var matchFiltered = filtered.PrimaryMatchedSources;
            var matchUnfiltered = unfiltered.PrimaryMatchedSources;

            var mixed = new SourceMatchLists(
                matchFiltered,
                matchUnfiltered,
                SearchRadius * 4,
                Verbose);

            var lines = StringizePositions(filtered);
            File.WriteAllLines("filtered.txt", lines);

            lines = StringizePositions(unfiltered);
            File.WriteAllLines("unfiltered.txt", lines);

            return Status;
        }

        void IDisposable.Dispose()
        {
        }

        private static string GetCompletenessTable(
            IReadOnlyDictionary<double, Fraction> completeness)
        {
            var sb = new StringBuilder();
            sb.Append("Mag\tMatch\tTotal\tFraction");
            sb.Append(Environment.NewLine);
            foreach (var kvp in completeness)
            {
                sb.Append(kvp.Key.ToString("00.00"));
                sb.Append("\t");
                sb.Append(kvp.Value.ToString());
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        private static IReadOnlyDictionary<double, Fraction> GetFractionDictionary(
            SourceList matchedSources,
            SourceList allSources)
        {
            var brightest = GetBrightestMagSource(allSources);
            var result = new Dictionary<double, Fraction>();
            for (var mag = 19.0; mag >= brightest; mag -= 0.25)
            {
                var matches = matchedSources.Filter(IsInBin);
                var total = allSources.Filter(IsInBin);
                var fraction = new Fraction(
                    matches.Count,
                    total.Count);

                result.Add(mag, fraction);

                bool IsInBin(ISource source)
                {
                    var magSource = source as IMagSource;

                    var sourceMag = magSource.Magnitude1;
                    return

                        sourceMag <= mag + 0.125 &&
                        true; // sourceMag > mag - 0.125;
                }
            }

            return result;
        }

        private static double GetBrightestMagSource(
            IEnumerable<ISource> sourceList)
        {
            var result = Double.MaxValue;
            foreach (var source in sourceList)
            {
                var spitzerSource = source as IMagSource;
                if (result > spitzerSource.Magnitude1)
                {
                    result = spitzerSource.Magnitude1;
                }
            }

            return result;
        }

        private static bool IsInRadius(
            ISource primary,
            ISource secondary,
            double radius)
        {
            var x1 = primary.EquatorialCoordinate;
            var x2 = secondary.EquatorialCoordinate;

            return x1.DistanceTo(x2).Arcseconds <= radius;
        }

        private string ReadFilteredPath()
        {
            if (!CommandSwitches.FilteredPathSet)
            {
                Console.WriteLine("Filtered mdex path not set.");
                Status = 1;
            }

            var result = CommandSwitches.FilteredPath;
            Console.WriteLine("Filtered mdex path: {0}", result);
            return result;
        }

        private string ReadUnfilteredPath()
        {
            if (!CommandSwitches.UnfilteredPathSet)
            {
                Console.WriteLine("Unfiltered mdex path not set.");
                Status = 1;
                return null;
            }

            var result = CommandSwitches.UnfilteredPath;
            Console.WriteLine("Unfiltered mdex path: {0}", result);
            return result;
        }

        private string ReadSpitzerPath()
        {
            if (!CommandSwitches.SpitzerPathSet)
            {
                Console.WriteLine("Spitzer path not set.");
                Status = 1;
                return null;
            }

            var result = CommandSwitches.SpitzerPath;
            Console.WriteLine("Spitzer path: {0}", result);
            return result;
        }

        private string ReadOutputPath()
        {
            if (!CommandSwitches.OutputPathSet)
            {
                Console.WriteLine("Matched source list path not set.");
                Status = 1;
                return null;
            }

            var result = CommandSwitches.OutputPath;
            Console.WriteLine("Matched source list path: {0}", result);
            return result;
        }

        private Angle ReadSearchRadius()
        {
            if (!CommandSwitches.SearchRadiusArcsecSet)
            {
                Console.WriteLine("Search radius not set.");
                Status = 1;
                return Angle.FromRadians(0);
            }

            var arcsec = CommandSwitches.SearchRadiusArcsec;
            Console.WriteLine("Search radius: {0} asec", arcsec);
            return Angle.FromArcseconds(arcsec);
        }

        private string ReadColor1()
        {
            if (!CommandSwitches.Color1Set)
            {
                return "red";
            }

            return CommandSwitches.Color1;
        }

        private string ReadColor2()
        {
            if (!CommandSwitches.Color2Set)
            {
                return "green";
            }

            return CommandSwitches.Color2;
        }

        private string ReadColor3()
        {
            if (!CommandSwitches.Color3Set)
            {
                return "yellow";
            }

            return CommandSwitches.Color3;
        }

        private int WriteMatches(double snrCutOff)
        {
            string[] lines = null; // StringizeMatchCount(subLists);

            var name = GetName();
            var path = String.Format(OutputPath, name);
            var header = Path.GetFileNameWithoutExtension(path);
            var contents = new List<string>(lines.Length + 1)
            {
                header
            };

            contents.AddRange(lines);

            if (Verbose)
            {
                Console.WriteLine("Writing to {0}", path);
            }

            File.WriteAllLines(path, contents);

            return Status;

            string GetName()
            {
                if (Double.IsNaN(snrCutOff))
                {
                    return "-all-snr.txt";
                }

                return String.Format("-snr-gte{0:0}.txt", snrCutOff);
            }
        }

        private int WriteAnomalies(
            SourceMatchLists sourceMatchLists)
        {
            var anomalies = sourceMatchLists.Filter(Filter);
            var lines = StringizePositions(anomalies);
            var path = String.Format(OutputPath, "-anomalies.txt");
            File.WriteAllLines(path, lines);

            var text = WriteRegions(anomalies.PrimaryMatchedSources, Color1);
            path = String.Format(OutputPath, "-anomalies-wise.reg");
            File.WriteAllText(path, text);

            text = WriteRegions(anomalies.SecondaryMatchedSources, Color2);
            path = String.Format(OutputPath, "-anomalies-spitzer.reg");
            File.WriteAllText(path, text);

            return Status;

            bool Filter(ISource primary, ISource secondary)
            {
                if (IsInRadius(primary, secondary, 5))
                {
                    return false;
                }

                var snrSource = primary as ISnrSource;
                return snrSource.SignalToNoise >= 20;
            }
        }

        private int ReadSourceLists()
        {
            if (Status != 0)
            {
                return Status;
            }

            Filtered = CreateSourceList(FilteredPath);
            if (Status != 0)
            {
                return Status;
            }

            Unfiltered = CreateSourceList(UnfilteredPath);
            if (Status != 0)
            {
                return Status;
            }

            Spitzer = CreateSourceList(SpitzerPath);
            if (Status != 0)
            {
                return Status;
            }

            return Status;
        }

        private SourceList CreateSourceList(string path)
        {
            var lines = ReadAllLinesSafe(path, Verbose);
            if (lines is null)
            {
                Status = -1;
                return null;
            }

            if (Verbose)
            {
                Console.WriteLine("Generating source table.");
            }

            try
            {
                return new SourceList(lines, Multithreaded, Verbose);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private int FilterSourceLists()
        {
            if (FilterSpitzerMag() != 0)
            {
                return Status;
            }

            /*
            if (FilterMdexSources() != 0)
            {
                return Status;
            }
            */

            if (FilterBounds() != 0)
            {
                return Status;
            }

            if (Verbose)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "Sources remaining in filtered mdex list:  {0}",
                    Filtered.Count);

                Console.WriteLine(
                    "Sources remaining in unfiltered mdex list:  {0}",
                    Unfiltered.Count);

                Console.WriteLine(
                    "Sources remaining in spitzer list: {0}",
                    Spitzer.Count);
            }

            return Status;
        }

        private int FilterSpitzerMag()
        {
            if (Verbose)
            {
                Console.WriteLine();
                Console.WriteLine("Setting spitzer source cutoff to mag <= 19");
            }

            Spitzer = Spitzer.Filter(IsInMag);

            if (Verbose)
            {
                Console.Write("Spitzer reduced to {0} sources.", Spitzer.Count);
            }

            return Status;

            bool IsInMag(ISource source)
            {
                var magSource = source as IMagSource;
                return magSource.Magnitude1 <= (19 + 0.125);
            }
        }

        private int FilterMdexSources()
        {
            if (Verbose)
            {
                Console.WriteLine();
                Console.WriteLine("Setting mdex source cutoff to snr >= 20");
            }

            Filtered = Filtered.Filter(IsInSnrRange);
            Unfiltered = Unfiltered.Filter(IsInSnrRange);

            if (Verbose)
            {
                Console.WriteLine(
                    "Filtered mdex reduced to {0} sources",
                    Filtered.Count);

                Console.WriteLine(
                    "Unfiltered mdex reduced to {0} sources",
                    Unfiltered.Count);
            }

            return Status;

            bool IsInSnrRange(ISource source)
            {
                var snrSource = source as ISnrSource;
                return snrSource.SignalToNoise >= 20;
            }
        }

        private int FilterBounds()
        {
            if (Verbose)
            {
                Console.WriteLine();
                Console.WriteLine("Calculating intersection of regions");
            }

            var bounds1 = SourceMatchLists.GetBounds(Filtered);
            var bounds2 = SourceMatchLists.GetBounds(Spitzer);

            var minRa = Angle.FromRadians(
                Max(bounds1.minRa.Radians, bounds2.minRa.Radians));

            var maxRa = Angle.FromRadians(
                Min(bounds1.maxRa.Radians, bounds2.maxRa.Radians));

            var minDec = Angle.FromRadians(
                Max(bounds1.minDec.Radians, bounds2.minDec.Radians));

            var maxDec = Angle.FromRadians(
                Min(bounds1.maxDec.Radians, bounds2.maxDec.Radians));

            if (Verbose)
            {
                Console.WriteLine("Min RA={0}", minRa);
                Console.WriteLine("Max RA={0}", maxRa);
                Console.WriteLine("Min Dec={0}", minDec);
                Console.WriteLine("Max Dec={0}", maxDec);

                Console.WriteLine();
                Console.WriteLine(
                    "Filtering source lists to bounded region.");
            }

            Filtered = Filtered.Filter(InBounds);
            Unfiltered = Unfiltered.Filter(InBounds);
            Spitzer = Spitzer.Filter(InBounds);

            return Status;

            bool InBounds(ISource source)
            {
                return
                    source.RA >= minRa &&
                    source.RA <= maxRa &&
                    source.Dec >= minDec &&
                    source.Dec <= maxDec;
            }
        }

        private string[] StringizeMatchCount(
            SourceMatchLists[] searches)
        {
            var lines = new string[searches.Length + 1];
            lines[0] = "radius\ttotal matches\tnew matches";
            if (Multithreaded)
            {
                Parallel.For(0, searches.Length, WriteLine);
            }
            else
            {
                for (var i = 0; i < searches.Length; i++)
                {
                    WriteLine(i);
                }
            }

            return lines;

            void WriteLine(int index)
            {
                var radius = 0.1 + (index / 10.0);
                var count = searches[index].MatchCount;

                var sb = new StringBuilder();
                sb.Append(radius);
                sb.Append('\t');
                sb.Append(count);
                sb.Append('\t');
                if (index == 0)
                {
                    sb.Append(count);
                }
                else
                {
                    var diff = count - searches[index - 1].MatchCount;
                    sb.Append(diff);
                }

                lines[index + 1] = sb.ToString();
            }
        }

        private string[] StringizePositions(
            SourceMatchLists matches)
        {
            var lines = new string[matches.MatchCount + 1];
            lines[0] = "WISE ID\tWISE RA\tWISE Dec\tSpit. ID\tW1 Mag\t[3.6] Mag\tWISE SNR";
            if (Multithreaded)
            {
                Parallel.For(0, matches.MatchCount, WriteLine);
            }
            else
            {
                for (var i = 0; i < matches.MatchCount; i++)
                {
                    WriteLine(i);
                }
            }

            return lines;

            void WriteLine(int index)
            {
                var source = matches.PrimaryMatchedSources[index];
                var wise = source as MdexSource;
                var spit = source as SpitzerSource;

                var wiseID = wise.Id;
                var wiseRa = wise.RA.Degrees;
                var wiseDec = wise.Dec.Degrees;
                var spitId = spit.Id;
                var wiseW1Mag = (double)wise[wise.Names["w1mag"]];
                var spitMag = spit.Magnitude1;
                var wiseSnr = wise.SignalToNoise;

                var sb = new StringBuilder();
                sb.Append(wiseID.ToString("000000"));
                sb.Append('\t');
                sb.Append(wiseRa.ToString("000.00000"));
                sb.Append('\t');
                sb.Append(wiseDec.ToString("000.00000"));
                sb.Append('\t');
                sb.Append(spitId.ToString("000000"));
                sb.Append('\t');
                sb.Append(wiseW1Mag.ToString("000.00000"));
                sb.Append('\t');
                sb.Append(spitMag.ToString("000.00000"));
                sb.Append('\t');
                sb.Append(wiseSnr.ToString("00.0000"));

                lines[index + 1] = sb.ToString();
            }
        }

        private string WriteRegions(
            IEnumerable<ISource> sources,
            string color)
        {
            var sb = new StringBuilder();

            // Write the global properties for the region file.
            sb.Append("# Region file format: DS9 version 4.1 ");
            sb.Append("global ");
            sb.Append("color = ");
            sb.Append(color);
            sb.Append(' ');
            sb.Append("dashlist = 8 3 ");
            sb.Append("width = 1 ");
            sb.Append("font = \"helvetica 10 normal roman\" ");
            sb.Append("select = 1 ");
            sb.Append("highlite = 1 ");
            sb.Append("dash = 0 ");
            sb.Append("fixed= 0 ");
            sb.Append("edit = 1 ");
            sb.Append("move = 1 ");
            sb.Append("delete = 1 ");
            sb.Append("include = 1 ");
            sb.Append("source = 1");
            sb.AppendLine();

            // coordinate system.
            sb.Append("fk5");
            sb.AppendLine();

            // Draw circle for each source.
            foreach (var source in sources)
            {
                sb.Append("circle(");
                sb.Append(source.RA.Degrees);
                sb.Append(',');
                sb.Append(source.Dec.Degrees);
                sb.Append(",5\") # color=");
                sb.Append(color);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string StringizeBins(
            IReadOnlyDictionary<double, double> bins)
        {
            var sb = new StringBuilder();
            foreach (var kvp in bins)
            {
                sb.Append(kvp.Key);
                sb.Append('\t');
                sb.Append(kvp.Value);
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        private IReadOnlyDictionary<double, double> GetReliabilityBins(
            SourceMatchLists search,
            double startSnr,
            double endSnr,
            double width)
        {
            var primaryMatched = search.PrimaryMatchedSources;
            var primaryUnmatched = search.PrimaryUnmatchedSources;

            var matched = primaryMatched.CreateBins(startSnr, width);
            var unmatched = primaryUnmatched.CreateBins(startSnr, width);

            var bins = new Dictionary<double, double>();
            for (var snr = startSnr; snr <= endSnr; snr += width)
            {
                var matchedCount = GetSize(matched);
                var unmatchedCount = GetSize(unmatched);
                var total = matchedCount + unmatchedCount;
                var reliability = (double)matchedCount / total;
                if (Double.IsNaN(reliability))
                {
                    reliability = -1;
                }

                bins.Add(snr, reliability);

                int GetSize(IReadOnlyDictionary<double, IReadOnlyCollection<ISource>> dictionary)
                {
                    if (dictionary.TryGetValue(snr, out var list))
                    {
                        return list.Count;
                    }

                    return 0;
                }
            }

            return bins;
        }

        private struct Fraction : IEquatable<Fraction>
        {
            public Fraction(int top, int bottom)
            {
                Top = top;
                Bottom = bottom;
            }

            public int Top
            {
                get;
            }

            public int Bottom
            {
                get;
            }

            public double Ratio
            {
                get
                {
                    return Top / (double)Bottom;
                }
            }

            public static bool operator ==(Fraction left, Fraction right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Fraction left, Fraction right)
            {
                return !(left == right);
            }

            public bool Equals(Fraction other)
            {
                return
                    Top.Equals(other.Top) &&
                    Bottom.Equals(other.Bottom);
            }

            public override bool Equals(object obj)
            {
                if (obj is Fraction other)
                {
                    return Equals(other);
                }

                return false;
            }

            public override int GetHashCode()
            {
                return Top ^ Bottom;
            }

            public override string ToString()
            {
                return String.Format(
                    "{0}\t{1}\t{2:0.00000}",
                    Top,
                    Bottom,
                    Ratio);
            }
        }
    }
}
