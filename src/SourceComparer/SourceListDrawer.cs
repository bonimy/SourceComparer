// <copyright file="SourceListDrawer.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace SourceComparer
{
    public class SourceListDrawer : IReadOnlyDictionary<int, ISource>
    {
        private IReadOnlyDictionary<int, ISource> Pixels
        {
            get;
        }

        public int Count
        {
            get
            {
                return Pixels.Count;
            }
        }

        public IEnumerable<int> Keys
        {
            get
            {
                return Pixels.Keys;
            }
        }

        public IEnumerable<ISource> Values
        {
            get
            {
                return Pixels.Values;
            }
        }

        public ISource this[int index]
        {
            get
            {
                return Pixels[index];
            }
        }

        public int Width
        {
            get
            {
                return Comparer.PixelWidth;
            }
        }

        public int Height
        {
            get
            {
                return Comparer.PixelHeight;
            }
        }

        public SourceList SourceList
        {
            get;
        }

        public EquatorialCoordinateComparer Comparer
        {
            get;
        }

        public SourceListDrawer(
            SourceList sourceList,
            EquatorialCoordinateComparer comparer,
            bool verbose)
        {
            SourceList = sourceList ??
                throw new ArgumentNullException(nameof(sourceList));

            Comparer = comparer ??
                throw new ArgumentNullException(nameof(comparer));

            var duplicates = 0;
            var pixels = new Dictionary<int, ISource>(sourceList.Count);
            for (var i = 0; i < SourceList.Count; i++)
            {
                var source = SourceList[i];
                var coordinate = source.EquatorialCoordinate;

                var point = comparer.GetPixel(coordinate);
                var index = GetIndex(point.X, point.Y);
                if (pixels.TryGetValue(index, out var duplicate))
                {
                    if (verbose)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Warning: Duplicate source found.");
                        Console.WriteLine(
                            "Source={0}; {1}",
                            source.Id,
                            coordinate);

                        Console.WriteLine(
                            "Match ={0}; {1}",
                            duplicate.Id,
                            coordinate);
                    }

                    duplicates++;
                    continue;
                }

                pixels.Add(index, source);
            }

            if (verbose && duplicates > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Number of duplicate sources: {0}", duplicates);
            }

            Pixels = pixels;
        }

        public bool ContainsKey(int key)
        {
            return Pixels.ContainsKey(key);
        }

        public bool TryGetValue(int key, out ISource value)
        {
            return Pixels.TryGetValue(key, out value);
        }

        public int GetIndex(int x, int y)
        {
            if (x < 0 || x >= Width)
            {
                return -1;
            }

            if (y < 0 || y >= Height)
            {
                return -1;
            }

            return (y * Width) + x;
        }

        public (int x, int y) GetCoordinates(int index)
        {
            if (index < 0 || index >= Width * Height)
            {
                return (-1, -1);
            }

            return (index % Width, index / Width);
        }

        public ISource GetSource(int x, int y)
        {
            return this[GetIndex(x, y)];
        }

        public SourceListDrawer MatchTo(
            SourceListDrawer other,
            int pixelRadius,
            bool verbose)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (!Comparer.Equals(other.Comparer))
            {
                throw new ArgumentException();
            }

            var searcher = new PixelMatcher(
                this,
                other,
                Comparer,
                pixelRadius,
                verbose);

            var filterList = SourceList.Filter(searcher.SharesPixel);

            if (verbose && searcher.Duplicates > 0)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "Number of duplicate sources: {0}",
                    searcher.Duplicates);
            }

            return new SourceListDrawer(filterList, Comparer, false);
        }

        public IEnumerator<KeyValuePair<int, ISource>> GetEnumerator()
        {
            return Pixels.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class PixelMatcher
        {
            public SourceListDrawer Primary
            {
                get;
            }

            public SourceListDrawer Secondary
            {
                get;
            }

            public EquatorialCoordinateComparer Comparer
            {
                get;
            }

            public int PixelRadius
            {
                get;
            }

            public bool Verbose
            {
                get;
            }

            public int Duplicates
            {
                get;
                private set;
            }

            private bool[] Matched
            {
                get;
            }

            private ISource Duplicate
            {
                get;
                set;
            }

            public PixelMatcher(
                SourceListDrawer primary,
                SourceListDrawer secondary,
                EquatorialCoordinateComparer comparer,
                int pixelRadius,
                bool verbose)
            {
                Primary = primary ??
                    throw new ArgumentNullException(nameof(primary));

                Secondary = secondary ??
                    throw new ArgumentNullException(nameof(secondary));

                Comparer = Secondary.Comparer;

                PixelRadius = pixelRadius;
                Verbose = verbose;
                Duplicates = 0;

                Matched = new bool[Secondary.SourceList.Count];
            }

            public bool SharesPixel(ISource source)
            {
                // Convert pixel point according to other pixel coordinate converter.
                var coordinate = source.EquatorialCoordinate;
                var point = Comparer.GetPixel(coordinate);

                Duplicate = null;
                if (!GetSource(point.X, point.Y))
                {
                    return false;
                }

                CheckForDuplicate(source);
                return true;
            }

            private void CheckForDuplicate(ISource source)
            {
                if (Duplicate is null)
                {
                    return;
                }

                Duplicates++;
                if (!Verbose)
                {
                    return;
                }

                var coordinate = source.EquatorialCoordinate;
                Console.WriteLine();
                Console.WriteLine("Warning: Duplicate source found.");
                Console.WriteLine(
                    "Source={0}; {1}",
                    source.Id,
                    coordinate);

                Console.WriteLine(
                    "Match ={0}; {1}",
                    Duplicate.Id,
                    coordinate);
            }

            private bool GetSource(int centerX, int centerY)
            {
                // Search by growing out of pixel radius to ensure closest matches.
                for (var radius = 0; radius <= PixelRadius; radius++)
                {
                    if (SearchInRadius(centerX, centerY, radius))
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool SearchInRadius(int centerX, int centerY, int radius)
            {
                for (var relativeY = -radius; relativeY <= +radius; relativeY++)
                {
                    var index = Secondary.GetIndex(centerX, centerY + relativeY);
                    if (index == -1)
                    {
                        continue;
                    }

                    if (SearchLine(centerX, relativeY, index, radius))
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool SearchLine(
                int centerX,
                int relativeY,
                int index,
                int radius)
            {
                for (var relativeX = -radius; relativeX <= +radius; relativeX++)
                {
                    var absoluteX = centerX + relativeX;
                    if (absoluteX < 0 || absoluteX >= Comparer.PixelWidth)
                    {
                        return false;
                    }

                    // Confine to circular radius.
                    var x2 = relativeX * relativeX;
                    var y2 = relativeY * relativeY;
                    var r2 = radius * radius;
                    if (x2 + y2 > r2)
                    {
                        continue;
                    }

                    if (CheckPixel(index + relativeX))
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool CheckPixel(int index)
            {
                if (!Secondary.TryGetValue(index, out var result))
                {
                    return false;
                }

                var matchIndex = result.Id - 1;

                // Don't match to a source that was already matched.
                if (Matched[matchIndex])
                {
                    Duplicate = result;
                    return false;
                }

                return Matched[matchIndex] = true;
            }
        }
    }
}
