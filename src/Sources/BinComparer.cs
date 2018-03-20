// <copyright file="BinComparer.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Sources
{
    using System;
    using System.Collections.Generic;

    public class BinComparer : EqualityComparer<double>
    {
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

        private double Zero
        {
            get;
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

        public override bool Equals(double x, double y)
        {
            var left = GetBinIndex(x);
            var right = GetBinIndex(y);

            return left == right;
        }

        public override int GetHashCode(double value)
        {
            return GetBinIndex(value);
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
