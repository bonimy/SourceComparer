using System;
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

        public SourceListDrawer(SourceList sourceList, EquatorialCoordinateComparer comparer, bool verbose)
        {
            SourceList = sourceList ?? throw new ArgumentNullException(nameof(sourceList));
            Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

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
                        Console.WriteLine("Source={0}; {1}", source.Id, coordinate);
                        Console.WriteLine("Match ={0}; {1}", duplicate.Id, coordinate);
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

        public SourceListDrawer MatchTo(SourceListDrawer other, int pixelRadius, bool verbose)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (!Comparer.Equals(other.Comparer))
            {
                throw new ArgumentException();
            }

            var duplicates = 0;
            var matched = new bool[other.SourceList.Count];
            var filterList = SourceList.Filter(SharesPixel);

            if (verbose && duplicates > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Number of duplicate sources: {0}", duplicates);
            }

            return new SourceListDrawer(filterList, Comparer, false);

            bool SharesPixel(ISource source)
            {
                ISource match = null;

                var coordinate = source.EquatorialCoordinate;

                // Convert pixel point according to other pixel coordinate converter.
                var point = other.Comparer.GetPixel(coordinate);

                ISource duplicate = null;
                if (GetSource(point.X, point.Y))
                {
                    if (duplicate != null)
                    {
                        duplicates++;
                        if (verbose)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Warning: Duplicate source found.");
                            Console.WriteLine("Source={0}; {1}", source.Id, coordinate);
                            Console.WriteLine("Match ={0}; {1}", duplicate.Id, coordinate);
                        }
                    }

                    return true;
                }

                return false;

                bool GetSource(int centerX, int centerY)
                {
                    // Search by growing out of pixel radius to ensure closest matches.
                    for (var radius = 0; radius <= pixelRadius; radius++)
                    {
                        if (SearchInRadius(centerX, centerY, radius))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool SearchInRadius(int centerX, int centerY, int radius)
                {
                    for (var relativeY = -radius; relativeY <= +radius; relativeY++)
                    {
                        var index = other.GetIndex(centerX, centerY + relativeY);
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

                bool SearchLine(int centerX, int relativeY, int index, int radius)
                {
                    for (var relativeX = -radius; relativeX <= +radius; relativeX++)
                    {
                        var absoluteX = centerX + relativeX;
                        if (absoluteX < 0 || absoluteX >= other.Comparer.PixelWidth)
                        {
                            return false;
                        }

                        // Confine to circular radius.
                        if ((relativeX * relativeX) + (relativeY * relativeY) > radius * radius)
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

                bool CheckPixel(int index)
                {
                    if (other.TryGetValue(index, out var result))
                    {
                        var matchIndex = result.Id - 1;

                        // Don't match to a source that was already matched.
                        if (matched[matchIndex])
                        {
                            duplicate = result;
                            return false;
                        }

                        match = result;
                        return matched[matchIndex] = true;
                    }

                    return false;
                }
            }
        }

        public IEnumerator<KeyValuePair<int, ISource>> GetEnumerator()
        {
            return Pixels.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
