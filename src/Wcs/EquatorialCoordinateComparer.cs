// <copyright file="EquatorialCoordinateComparer.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Wcs
{
    using System.Collections.Generic;
    using System.Drawing;

    public class EquatorialCoordinateComparer : IEqualityComparer<EquatorialCoordinate>
    {
        public EquatorialCoordinateComparer(
            Angle minRa,
            Angle maxRa,
            Angle minDec,
            Angle maxDec,
            Angle searchRadius)
        {
            // This dictionary is technically usable for any equatorial coordinates, but it was written for coordinates near the equator and small separation with even smaller search radii.
            MinRa = minRa;
            MaxRa = maxRa;
            MinDec = minDec;
            MaxDec = maxDec;
            SearchRadius = searchRadius;

            // Get equatorial center and range.
            Center = new EquatorialCoordinate(
                (MinRa + MaxRa) / 2,
                (MinDec + MaxDec) / 2);

            RaWidth = MaxRa - MinRa;
            DecHeight = MaxRa - MinRa;

            // Grow search radius slightly to ensure searching only one pixel out.
            RadiansPerPixel = SearchRadius.Radians * 1.01;
            PixelsPerRadian = 1 / RadiansPerPixel;

            // Create image resolution size.
            PixelWidth = (int)(RaWidth.Radians * PixelsPerRadian);
            PixelHeight = (int)(DecHeight.Radians * PixelsPerRadian);
        }

        public Angle MinRa
        {
            get;
        }

        public Angle MaxRa
        {
            get;
        }

        public Angle RaWidth
        {
            get;
        }

        public Angle MinDec
        {
            get;
        }

        public Angle MaxDec
        {
            get;
        }

        public Angle DecHeight
        {
            get;
        }

        public EquatorialCoordinate Center
        {
            get;
        }

        public Angle SearchRadius
        {
            get;
        }

        public double RadiansPerPixel
        {
            get;
        }

        public double PixelsPerRadian
        {
            get;
        }

        public int PixelWidth
        {
            get;
        }

        public int PixelHeight
        {
            get;
        }

        public Point GetPixel(EquatorialCoordinate obj)
        {
            var relRa = obj.RA - MinRa;
            var scaleX = relRa.Radians * PixelsPerRadian;
            var x = (int)scaleX;

            var relDec = obj.Dec - MinDec;
            var scaleY = relDec.Radians * PixelsPerRadian;
            var y = (int)scaleY;

            // ToDo: allow rotation angle.
            return new Point(x, y);
        }

        public EquatorialCoordinate GetEquatorialCoordinate(Point pixel)
        {
            // ToDo: allow rotation angle.
            var relRa = Angle.FromRadians(pixel.X * RadiansPerPixel);
            var ra = relRa + MinRa;

            var relDec = Angle.FromRadians(pixel.Y * RadiansPerPixel);
            var dec = relDec + MinDec;

            return new EquatorialCoordinate(ra, dec);
        }

        public bool Equals(EquatorialCoordinate x, EquatorialCoordinate y)
        {
            var left = GetPixel(x);
            var right = GetPixel(y);
            return left.Equals(right);
        }

        public int GetHashCode(EquatorialCoordinate obj)
        {
            return GetPixel(obj).GetHashCode();
        }
    }
}
