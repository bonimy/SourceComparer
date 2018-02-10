// <copyright file="SourceComparisonMode.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using static SourceComparer.Program;

namespace SourceComparer
{
    internal static class SourceComparisonMode
    {
        private static int Status
        {
            get;
            set;
        }

        public static bool Multithreaded
        {
            get;
            private set;
        }

        public static bool Verbose
        {
            get;
            private set;
        }

        public static bool TimeStamp
        {
            get;
            private set;
        }

        public static string LogPath
        {
            get;
            private set;
        }

        public static string PrimaryPath
        {
            get;
            private set;
        }

        public static string SecondaryPath
        {
            get;
            private set;
        }

        public static string OutputPath
        {
            get;
            private set;
        }

        public static Angle SearchRadius
        {
            get;
            private set;
        }

        public static int Run(CommandSwitches commandSwitches)
        {
            if (SetSwitches(commandSwitches) != 0)
            {
                return Status;
            }

            var primary = CreateSourceList(PrimaryPath);
            if (Status != 0)
            {
                return Status;
            }

            var secondary = CreateSourceList(SecondaryPath);
            if (Status != 0)
            {
                return Status;
            }

            if (Verbose)
            {
                Console.WriteLine();
                Console.WriteLine("Setting spitzer source cutoff to mag <= 19");
            }

            secondary = secondary.Filter(source => ((SpitzerSource)source).Magnitude1 <= 19);

            if (Verbose)
            {
                Console.Write("Spitzer reduced to {0} sources.", secondary.Count);

                Console.WriteLine();
                Console.WriteLine("Calculating intersection of regions");
            }

            var bounds1 = SourceMatchLists.GetBounds(primary);
            var bounds2 = SourceMatchLists.GetBounds(secondary);

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
                Console.WriteLine("Filtering source lists to bounded region.");
            }

            primary = primary.Filter(InBounds);
            secondary = secondary.Filter(InBounds);

            primary = primary.Filter(IsInSnrRange);

            if (Verbose)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "Sources remaining in primary list:  {0}",
                    primary.Count);

                Console.WriteLine(
                    "Sources remaining in secondary list: {0}",
                    secondary.Count);
            }

            var searches = new SourceMatchLists[100];
            var total1 = new List<ISource>();
            var total2 = new List<ISource>();
            var count = new List<int>();
            for (var i = 0; i < searches.Length; i++)
            {
                var radius = 0.1 + (i / 10.0);

                if (Verbose)
                {
                    Console.WriteLine();
                    Console.WriteLine("Using {0:0.0}\" search radius.", radius);
                }

                searches[i] = new SourceMatchLists(
                    primary,
                    secondary,
                    Angle.FromArcseconds(radius),
                    Verbose);

                primary = searches[i].PrimaryUnmatchedSources;
                secondary = searches[i].SecondaryUnmatchedSources;

                total1.AddRange(searches[i].PrimaryMatchedSources);
                total2.AddRange(searches[i].SecondaryMatchedSources);

                if (Verbose)
                {
                    Console.WriteLine();
                    Console.WriteLine("Total matches: {0}", total1.Count);
                }
            }

            var result = new SourceMatchLists(
                new SourceList(primary.NameDictionary, total1),
                searches[searches.Length - 1].PrimaryUnmatchedSources,
                new SourceList(secondary.NameDictionary, total2),
                searches[searches.Length - 1].SecondaryUnmatchedSources);

            //WriteReliability(result);

            File.WriteAllLines("counts-20.txt", StringizeMatchCount(searches));
            File.WriteAllLines("data-20.txt", StringizePositions(result));

            return Status;

            bool InBounds(ISource source)
            {
                return
                    source.RA >= minRa &&
                    source.RA <= maxRa &&
                    source.Dec >= minDec &&
                    source.Dec <= maxDec;
            }

            bool IsInSnrRange(ISource source)
            {
                var snrSource = source as ISnrSource;
                var snr = snrSource.SignalToNoise;
                return snr <= 20.1;
            }
        }

        private static string[] StringizeMatchCount(SourceMatchLists[] searches)
        {
            var lines = new string[searches.Length + 1];
            lines[0] = "radius\tmatches";
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
                lines[index] = sb.ToString();
            }
        }

        private static string[] StringizePositions(SourceMatchLists matches)
        {
            var lines = new string[matches.MatchCount + 1];
            lines[0] = "WISE ID\tΔRA\tΔDec\tDistance\tW1 SNR\tSNR\tch1 mag";
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
                var wise = matches.PrimaryMatchedSources[index] as MdexSource;
                var spit = matches.SecondaryMatchedSources[index] as SpitzerSource;

                var id = wise.Id;

                var dRa = (spit.RA - wise.RA).Arcseconds;
                var ddec = (spit.Dec - wise.Dec).Arcseconds;
                var radius = Sqrt((dRa * dRa) + (ddec * ddec));

                var w1Snr = wise.W1Snr;
                var w2Snr = wise.W2Snr;
                var snr = Sqrt((w1Snr * w1Snr) + (w2Snr * w2Snr));

                var mag = spit.Magnitude1;

                var sb = new StringBuilder();
                sb.Append(id);
                sb.Append('\t');
                sb.Append(dRa);
                sb.Append('\t');
                sb.Append(ddec);
                sb.Append('\t');
                sb.Append(radius);
                sb.Append('\t');
                sb.Append(w1Snr);
                sb.Append('\t');
                sb.Append(snr);
                sb.Append('\t');
                sb.Append(mag);

                lines[index + 1] = sb.ToString();
            }
        }

        private static void WriteReliability(SourceMatchLists bounded)
        {
            var reliability2to10 = GetReliabilityBins(bounded, 2.4, 9.4, 0.2);
            var reliability10to80 = GetReliabilityBins(bounded, 10, 80, 1);

            var unmatched = bounded.PrimaryUnmatchedSources.CreateBins(2.4, 0.2);

            File.WriteAllText("regions.reg", WriteRegions(unmatched[5]));

            var sb = new StringBuilder(StringizeBins(reliability2to10));
            sb.Append(StringizeBins(reliability10to80));
            File.WriteAllText("reliability.txt", sb.ToString());
        }

        private static string WriteRegions(IEnumerable<ISource> sources)
        {
            var sb = new StringBuilder();

            // Write the global properties for the region file.
            sb.Append("# Region file format: DS9 version 4.1 ");
            sb.Append("global ");
            sb.Append("color = green ");
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

            // Draw 20 arcsec circle for each source.
            foreach (var source in sources)
            {
                sb.Append("circle(");
                sb.Append(source.RA);
                sb.Append(',');
                sb.Append(source.Dec);
                sb.Append(",5\") # color=green");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string StringizeBins(
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

        private static IReadOnlyDictionary<double, double> GetReliabilityBins(
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

        private static SourceList CreateSourceList(string path)
        {
            var lines = ReadAllLinesSafe(path, Verbose);
            if (lines == null)
            {
                Status = -1;
                return null;
            }

            try
            {
                if (Verbose)
                {
                    Console.WriteLine("Generating source table.");
                }

                return new SourceList(lines, Multithreaded, Verbose);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private static int SetSwitches(CommandSwitches commandSwitches)
        {
            if (commandSwitches == null)
            {
                throw new ArgumentNullException(nameof(commandSwitches));
            }

            // This information is already printed to the output stream.
            Multithreaded = commandSwitches.Multithreaded;
            Verbose = commandSwitches.Verbose;
            TimeStamp = commandSwitches.TimeStamp;
            LogPath = commandSwitches.LogPath;

            Console.WriteLine();
            Console.WriteLine("Draw mode:");

            if (commandSwitches.PrimaryPathSet)
            {
                PrimaryPath = commandSwitches.PrimaryPath;
                Console.WriteLine("Primary source path: {0}", PrimaryPath);
            }
            else
            {
                Console.WriteLine("Primary path not set.");
                Status = 1;
            }

            if (commandSwitches.SecondaryPathSet)
            {
                SecondaryPath = commandSwitches.SecondaryPath;
                Console.WriteLine("Secondary source path: {0}", SecondaryPath);
            }
            else
            {
                Console.WriteLine("Secondary source path not set.");
                Status = 1;
            }

            if (commandSwitches.OutputPathSet)
            {
                OutputPath = commandSwitches.OutputPath;
                Console.WriteLine("Matched source list path: {0}", OutputPath);
            }
            else
            {
                Console.WriteLine("Matched source list path not set.");
                Status = 1;
            }

            if (commandSwitches.SearchRadiusArcsecSet)
            {
                SearchRadius = Angle.FromArcseconds(commandSwitches.SearchRadiusArcsec);
                Console.WriteLine("Search radius: {0} asec", SearchRadius.Arcseconds);
            }
            else
            {
                Console.WriteLine("Search radius not set.");
                Status = 1;
            }

            return Status;
        }
    }
}
