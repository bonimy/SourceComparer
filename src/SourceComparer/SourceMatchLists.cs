// <copyright file="SourceMatchLists.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections.Generic;
using NodeList = System.Collections.Generic.List<System.Collections.Generic.LinkedListNode<SourceComparer.ISource>>;
using SourceNode = System.Collections.Generic.LinkedListNode<SourceComparer.ISource>;

namespace SourceComparer
{
    public class SourceMatchLists
    {
        public IReadOnlyList<ISource> PrimaryMatchedSources
        {
            get;
        }

        public IReadOnlyList<ISource> SecondaryMatchedSources
        {
            get;
        }

        public IReadOnlyCollection<ISource> PrimaryUnmatchedSources
        {
            get;
        }

        public IReadOnlyCollection<ISource> SecondaryUnmatchedSources
        {
            get;
        }

        public SourceMatchLists(
            IReadOnlyCollection<ISource> primarySourceList,
            IReadOnlyCollection<ISource> secondarySourceList,
            Angle searchRadius,
            bool verbose)
        {
            var matches = new SourceMatchBuilder(
                primarySourceList,
                secondarySourceList,
                searchRadius,
                verbose);

            PrimaryMatchedSources = matches.PrimaryMatchedSources;
            SecondaryMatchedSources = matches.SecondaryMatchedSources;

            PrimaryUnmatchedSources = matches.PrimaryUnmatchedSources;
            SecondaryUnmatchedSources = matches.SecondaryUnmatchedSources;
        }

        private class SourceMatchBuilder
        {
            public List<ISource> PrimaryMatchedSources
            {
                get;
            }

            public List<ISource> SecondaryMatchedSources
            {
                get;
            }

            public LinkedList<ISource> PrimaryUnmatchedSources
            {
                get;
            }

            public LinkedList<ISource> SecondaryUnmatchedSources
            {
                get;
            }

            private IReadOnlyDictionary<Position2D, NodeList> PrimarySourceDictionary
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

            public SourceMatchBuilder(
                IReadOnlyCollection<ISource> primarySourceList,
                IReadOnlyCollection<ISource> secondarySourceList,
                Angle searchRadius,
                bool verbose)
            {
                if (primarySourceList == null)
                {
                    throw new ArgumentNullException(nameof(primarySourceList));
                }

                if (secondarySourceList == null)
                {
                    throw new ArgumentNullException(nameof(secondarySourceList));
                }

                Verbose = verbose;
                SearchRadius = searchRadius;

                PrimaryUnmatchedSources = new LinkedList<ISource>(primarySourceList);
                SecondaryUnmatchedSources = new LinkedList<ISource>(secondarySourceList);

                PrimaryMatchedSources = new List<ISource>(PrimaryUnmatchedSources.Count);
                SecondaryMatchedSources = new List<ISource>(SecondaryUnmatchedSources.Count);

                if (Verbose)
                {
                    Console.WriteLine("Calculating source bounds.");
                }

                CreateEquatorialCoordinateComparer();

                if (Verbose)
                {
                    Console.WriteLine("Hashing primary source positions.");
                }

                CreatePrimarySourceDictionary();

                if (Verbose)
                {
                    Console.WriteLine("Determining matches to secondary source positions");
                }

                MatchSources();

                if (Verbose)
                {
                    Console.WriteLine("Number of matches:           {0}", PrimaryMatchedSources.Count);
                    Console.WriteLine("Unmatched primary sources:   {0}", PrimaryUnmatchedSources.Count);
                    Console.WriteLine("Unmatched secondary sources: {0}", SecondaryUnmatchedSources.Count);
                }
            }

            private void CreatePrimarySourceDictionary()
            {
                var nodes = PrimaryUnmatchedSources;
                var dictionary = new Dictionary<Position2D, NodeList>(nodes.Count);
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

                PrimarySourceDictionary = dictionary;
            }

            private void MatchSources()
            {
                // Check every source in the secondary collection.
                var nodes = SecondaryUnmatchedSources;
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
                    if (matchedNode == null)
                    {
                        // No matches here, go to the next node.
                        node = node.Next;
                        continue;
                    }

                    // Add the match.
                    var match = matchedNode.Value;
                    PrimaryMatchedSources.Add(match);
                    SecondaryMatchedSources.Add(current);

                    // Remove the matched source from the pixel collection so it cannot be matched again.
                    var position = Comparer.GetPixel(match.EquatorialCoordinate);
                    nodeList = PrimarySourceDictionary[position];
                    nodeList.Remove(matchedNode);

                    // Remove the matched source from the primary list (so we end up with all unmatched sources afterwards).
                    PrimaryUnmatchedSources.Remove(matchedNode);

                    // Remove this node from the secondary list and update the position.
                    var temp = node;
                    node = node.Next;
                    SecondaryUnmatchedSources.Remove(temp);
                    continue;
                }
            }

            private NodeList GetCandidateMatches(Position2D pixel)
            {
                // Check just the center first.
                if (PrimarySourceDictionary.TryGetValue(pixel, out var result))
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
                        var position = pixel + new Position2D(x, y);
                        if (PrimarySourceDictionary.TryGetValue(position, out result))
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
                    var coordinate0 = source.EquatorialCoordinate;
                    var coordinate1 = comparison.Value.EquatorialCoordinate;
                    var angle = coordinate0.DistanceTo(coordinate1);
                    if (distance > angle.Radians)
                    {
                        distance = angle.Radians;

                        if (distance <= SearchRadius.Radians)
                        {
                            result = comparison;
                        }
                    }
                }

                return result;
            }

            private void CreateEquatorialCoordinateComparer()
            {
                var minRa = Double.MaxValue;
                var maxRa = Double.MinValue;

                var minDec = Double.MaxValue;
                var maxDec = Double.MinValue;

                foreach (var source in PrimaryUnmatchedSources)
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
