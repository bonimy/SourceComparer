// <copyright file="SourceMatchLists.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Sources
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Wcs;
    using NodeList = System.Collections.Generic.List<
        System.Collections.Generic.LinkedListNode<ISource>>;
    using SourceNode = System.Collections.Generic.LinkedListNode<
        ISource>;

    public class SourceMatchLists
    {
        public SourceMatchLists(
            SourceList primarySourceList,
            SourceList secondarySourceList,
            Angle searchRadius,
            bool verbose)
        {
            var matches = new SourceMatchBuilder(
                primarySourceList,
                secondarySourceList,
                searchRadius,
                verbose);

            PrimaryMatchedSources = new SourceList(
                primarySourceList.NameDictionary,
                matches.PrimaryMatchedSources);

            PrimaryUnmatchedSources = new SourceList(
                primarySourceList.NameDictionary,
                matches.PrimarySources);

            SecondaryMatchedSources = new SourceList(
                secondarySourceList.NameDictionary,
                matches.SecondaryMatchedSources);

            SecondaryUnmatchedSources = new SourceList(
                secondarySourceList.NameDictionary,
                matches.SecondarySources);
        }

        internal SourceMatchLists(
            SourceList primaryMatchedSources,
            SourceList primaryUnmatchedSources,
            SourceList secondaryMatchedSources,
            SourceList secondaryUnmatchedSources)
        {
            PrimaryMatchedSources = primaryMatchedSources;
            PrimaryUnmatchedSources = primaryUnmatchedSources;
            SecondaryMatchedSources = secondaryMatchedSources;
            SecondaryUnmatchedSources = secondaryUnmatchedSources;
        }

        public int MatchCount
        {
            get
            {
                return PrimaryMatchedSources.Count;
            }
        }

        public SourceList PrimaryMatchedSources
        {
            get;
        }

        public SourceList SecondaryMatchedSources
        {
            get;
        }

        public SourceList PrimaryUnmatchedSources
        {
            get;
        }

        public SourceList SecondaryUnmatchedSources
        {
            get;
        }

        public static (
            Angle minRa,
            Angle maxRa,
            Angle minDec,
            Angle maxDec) GetBounds(
            IEnumerable<ISource> sources)
        {
            var minRa = Angle.FromRadians(Double.MaxValue);
            var maxRa = Angle.FromRadians(Double.MinValue);
            var minDec = Angle.FromRadians(Double.MaxValue);
            var maxDec = Angle.FromRadians(Double.MinValue);

            foreach (var source in sources)
            {
                if (minRa > source.RA)
                {
                    minRa = source.RA;
                }

                if (minDec > source.Dec)
                {
                    minDec = source.Dec;
                }

                if (maxRa < source.RA)
                {
                    maxRa = source.RA;
                }

                if (maxDec < source.Dec)
                {
                    maxDec = source.Dec;
                }
            }

            return (

                 minRa,
                //// Angle.FromDegrees(149.36076),
                maxRa,
                minDec,
                //// Angle.FromDegrees(1.4154749),
                maxDec);
        }

        public SourceMatchLists RestrictToBounds()
        {
            var (minRa, maxRa, minDec, maxDec) = GetBounds(PrimaryMatchedSources);
            return RestrictToBounds(minRa, maxRa, minDec, maxDec);
        }

        public SourceMatchLists RestrictToBounds(
            Angle minRa,
            Angle maxRa,
            Angle minDec,
            Angle maxDec)
        {
            var sources1 = new List<ISource>(PrimaryMatchedSources.Count);
            var sources2 = new List<ISource>(SecondaryMatchedSources.Count);
            for (var i = 0; i < PrimaryMatchedSources.Count; i++)
            {
                var source1 = PrimaryMatchedSources[i];
                var source2 = SecondaryMatchedSources[i];

                // Both matching sources must be in bounds to be considered.
                if (InBounds(source1) && InBounds(source2))
                {
                    sources1.Add(source1);
                    sources2.Add(source2);
                }
            }

            var primaryMatched = new SourceList(
                PrimaryMatchedSources.NameDictionary,
                sources1);

            var secondaryMatched = new SourceList(
                SecondaryMatchedSources.NameDictionary,
                sources2);

            var primaryUnmatched = PrimaryUnmatchedSources.Filter(InBounds);

            var secondaryUnmatched = SecondaryUnmatchedSources.Filter(InBounds);

            return new SourceMatchLists(
                primaryMatched,
                primaryUnmatched,
                secondaryMatched,
                secondaryUnmatched);

            bool InBounds(ISource source)
            {
                return
                    source.RA >= minRa &&
                    source.RA <= maxRa &&
                    source.Dec >= minDec &&
                    source.Dec <= maxDec;
            }
        }

        public SourceMatchLists Filter(MatchFilterCallback matchFilter)
        {
            if (matchFilter is null)
            {
                throw new ArgumentNullException(nameof(matchFilter));
            }

            var primaryMatched = new List<ISource>();
            var secondaryMatches = new List<ISource>();

            var primaryUnmatches = new List<ISource>(
                PrimaryUnmatchedSources);

            var secondaryUnmatched = new List<ISource>(
                SecondaryUnmatchedSources);

            for (var i = 0; i < MatchCount; i++)
            {
                var primary = PrimaryMatchedSources[i];
                var secondary = SecondaryMatchedSources[i];

                if (matchFilter(primary, secondary))
                {
                    primaryMatched.Add(primary);
                    secondaryMatches.Add(secondary);
                }
                else
                {
                    primaryUnmatches.Add(primary);
                    secondaryUnmatched.Add(secondary);
                }
            }

            var primaryMatchedSources = new SourceList(
                PrimaryMatchedSources.NameDictionary,
                primaryMatched);

            var secondaryMatchedSources = new SourceList(
                SecondaryMatchedSources.NameDictionary,
                secondaryMatches);

            var primaryUnmatchedSources = new SourceList(
                PrimaryMatchedSources.NameDictionary,
                primaryUnmatches);

            var secondaryUnmatchedSources = new SourceList(
                SecondaryUnmatchedSources.NameDictionary,
                secondaryUnmatched);

            return new SourceMatchLists(
                primaryMatchedSources,
                primaryUnmatchedSources,
                secondaryMatchedSources,
                secondaryUnmatchedSources);
        }

        private class SourceMatchBuilder
        {
            public SourceMatchBuilder(
                IReadOnlyCollection<ISource> primarySourceList,
                IReadOnlyCollection<ISource> secondarySourceList,
                Angle searchRadius,
                bool verbose)
            {
                if (primarySourceList is null)
                {
                    throw new ArgumentNullException(nameof(primarySourceList));
                }

                if (secondarySourceList is null)
                {
                    throw new ArgumentNullException(nameof(secondarySourceList));
                }

                Verbose = verbose;
                SearchRadius = searchRadius;

                PrimarySources = new LinkedList<ISource>(primarySourceList);
                SecondarySources = new LinkedList<ISource>(secondarySourceList);

                PrimaryMatchedSources = new List<ISource>(
                    PrimarySources.Count);

                SecondaryMatchedSources = new List<ISource>(
                    SecondarySources.Count);

                if (Verbose)
                {
                    Console.WriteLine();
                    Console.WriteLine("Calculating source bounds.");
                }

                CreateEquatorialCoordinateComparer();

                if (Verbose)
                {
                    Console.WriteLine("Hashing primary source positions.");
                }

                CreateSourceDictionary();

                if (Verbose)
                {
                    Console.WriteLine("Determining matches to secondary source positions");
                }

                MatchSources();

                if (Verbose)
                {
                    Console.WriteLine("Number of matches:           {0}", PrimaryMatchedSources.Count);
                    Console.WriteLine("Unmatched primary sources:   {0}", PrimarySources.Count);
                    Console.WriteLine("Unmatched secondary sources: {0}", SecondarySources.Count);
                }
            }

            public List<ISource> PrimaryMatchedSources
            {
                get;
            }

            public List<ISource> SecondaryMatchedSources
            {
                get;
            }

            public LinkedList<ISource> PrimarySources
            {
                get;
            }

            public LinkedList<ISource> SecondarySources
            {
                get;
            }

            private IReadOnlyDictionary<Point, NodeList> SourceDictionary
            {
                get;
                set;
            }

            private Angle SearchRadius
            {
                get;
            }

            private bool Verbose
            {
                get;
            }

            private EquatorialCoordinateComparer Comparer
            {
                get;
                set;
            }

            private void CreateSourceDictionary()
            {
                var nodes = SecondarySources;
                var dictionary = new Dictionary<Point, NodeList>(nodes.Count);
                for (var node = nodes.First; node != null; node = node.Next)
                {
                    // Get the pixel coordinate of the current equatorial coordinate.
                    var coordinate = node.Value.EquatorialCoordinate;
                    var pixel = Comparer.GetPixel(coordinate);

                    // Do we already have a collection of sources at this pixel?
                    if (dictionary.TryGetValue(pixel, out var nodeList))
                    {
                        // If so, add this source and keep going.
                        nodeList.Add(node);
                        continue;
                    }

                    // If not, create the collection with this source and add it to the current pixel.
                    nodeList = new List<SourceNode>() { node };
                    dictionary.Add(pixel, nodeList);
                }

                SourceDictionary = dictionary;
            }

            private void MatchSources()
            {
                // Check every source in the secondary collection.
                var nodes = PrimarySources;
                for (var node = nodes.First; node != null;)
                {
                    // Get the source at this node, and the pixel it falls in.
                    var current = node.Value;
                    var coordinate = current.EquatorialCoordinate;
                    var pixel = Comparer.GetPixel(coordinate);
                    var nodeList = GetCandidateMatches(pixel);

                    // Find the source that is closest to the candidate source.
                    var matchedNode = GetNearestSourceNode(current, nodeList);

                    // Did we get a match?
                    if (matchedNode is null)
                    {
                        // No matches here, go to the next node.
                        node = node.Next;
                        continue;
                    }

                    // Add the match.
                    var match = matchedNode.Value;
                    PrimaryMatchedSources.Add(current);
                    SecondaryMatchedSources.Add(match);

                    // Remove the matched source from the pixel collection so it cannot be matched again.
                    var position = Comparer.GetPixel(match.EquatorialCoordinate);
                    nodeList = SourceDictionary[position];
                    nodeList.Remove(matchedNode);

                    // Remove the matched source from the primary list (so we end up with all unmatched sources afterwards).
                    SecondarySources.Remove(matchedNode);

                    // Remove this node from the secondary list and update the position.
                    var temp = node;
                    node = node.Next;
                    PrimarySources.Remove(temp);
                    continue;
                }
            }

            private NodeList GetCandidateMatches(Point pixel)
            {
                // Check just the center first.
                if (SourceDictionary.TryGetValue(pixel, out var result))
                {
                    // add the result to the collection.
                    return result;
                }

                // Get all sources in the 3x3 grid centered on the candidate pixel. These 9 tiles are all within the source list
                var nodeList = new NodeList();
                for (var y = -1; y <= +1; y++)
                {
                    for (var x = -1; x <= +1; x++)
                    {
                        // Are there candidate sources in this pixel?
                        var position = new Point(pixel.X + x, pixel.Y + y);
                        if (SourceDictionary.TryGetValue(position, out result))
                        {
                            // add the result to the collection.
                            nodeList.AddRange(result);
                        }
                    }
                }

                return nodeList;
            }

            private SourceNode GetNearestSourceNode(ISource source, NodeList comparisons)
            {
                // This is an O(N) operation so we want a small node list.
                SourceNode result = null;
                var distance = Double.MaxValue;
                foreach (var comparison in comparisons)
                {
                    var other = comparison.Value;
                    var coordinate0 = source.EquatorialCoordinate;
                    var coordinate1 = other.EquatorialCoordinate;
                    var angle = coordinate0.DistanceTo(coordinate1);
                    var radians = angle.Radians;
                    if (distance <= radians)
                    {
                        continue;
                    }

                    distance = radians;
                    if (distance > SearchRadius.Radians)
                    {
                        continue;
                    }

                    var magSource1 = source as IMagSource;
                    var magSource2 = other as IMagSource;

                    if (magSource1 is null || magSource2 is null)
                    {
                        result = comparison;
                        continue;
                    }

                    var magnitude1 = magSource1.Magnitude1;
                    var magnitude2 = magSource2.Magnitude1;
                    if (magnitude1 - magnitude2 > 1)
                    {
                        continue;
                    }

                    result = comparison;
                }

                return result;
            }

            private void CreateEquatorialCoordinateComparer()
            {
                var minRa = Double.MaxValue;
                var maxRa = Double.MinValue;

                var minDec = Double.MaxValue;
                var maxDec = Double.MinValue;

                foreach (var source in SecondarySources)
                {
                    var ra = source.RA.Radians;
                    var dec = source.Dec.Radians;

                    if (minRa > ra)
                    {
                        minRa = ra;
                    }

                    if (maxRa < ra)
                    {
                        maxRa = ra;
                    }

                    if (minDec > dec)
                    {
                        minDec = dec;
                    }

                    if (maxDec < dec)
                    {
                        maxDec = dec;
                    }
                }

                Comparer = new EquatorialCoordinateComparer(
                    Angle.FromRadians(minRa),
                    Angle.FromRadians(maxRa),
                    Angle.FromRadians(minDec),
                    Angle.FromRadians(maxDec),
                    SearchRadius);
            }
        }
    }
}
