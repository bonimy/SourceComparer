using System;
using System.Collections.Generic;

namespace SourceComparer
{
    public class BinComparer : IEqualityComparer<double>
    {
        private double Zero
        {
            get;
        }

        public double Center
        {
            get;
        }

        public double Size
        {
            get;
        }

        public double Width
        {
            get;
        }

        public BinComparer(double value, double size)
        {
            if (Double.IsNaN(value))
            {
                throw new ArgumentException();
            }

            if (Double.IsInfinity(value))
            {
                throw new ArgumentException();
            }

            if (Double.IsNaN(size))
            {
                throw new ArgumentException();
            }

            if (Double.IsInfinity(size))
            {
                throw new ArgumentException();
            }

            Center = value;
            Size = size;
            Width = Size * 2;
            Zero = Center - Size;
        }

        public int GetBinIndex(double value)
        {
            if (Double.IsNaN(value))
            {
                throw new ArgumentException();
            }

            if (Double.IsInfinity(value))
            {
                throw new ArgumentException();
            }

            return (int)((value - Zero) / Width);
        }

        public double GetBinCenter(double value)
        {
            var index = GetBinIndex(value);

            return (index * Width) + Center;
        }

        public bool Equals(double x, double y)
        {
            var left = GetBinIndex(x);
            var right = GetBinIndex(y);

            return left == right;
        }

        public int GetHashCode(double value)
        {
            return GetBinIndex(value).GetHashCode();
        }

        public override string ToString()
        {
            return String.Format(
                "{0}: ±{1}",
                Center,
                Size);
        }
    }
}
