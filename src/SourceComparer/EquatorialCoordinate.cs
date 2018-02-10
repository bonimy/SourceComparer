// <copyright file="EquatorialCoordinate.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using static System.Math;

namespace SourceComparer
{
    public struct EquatorialCoordinate : IEquatable<EquatorialCoordinate>
    {
        public static readonly EquatorialCoordinate Empty = new EquatorialCoordinate();

        public Angle RA
        {
            get;
        }

        public Angle Dec
        {
            get;
        }

        public EquatorialCoordinate(Angle ra, Angle dec)
        {
            RA = ra;
            Dec = dec;
        }

        public Angle DistanceTo(EquatorialCoordinate other)
        {
            // https://en.wikipedia.org/wiki/Great-circle_distance
            var deltaDec = Dec.Radians - other.Dec.Radians;
            var deltaRA = RA.Radians - other.RA.Radians;

            var left = Sin(deltaDec / 2);
            left *= left;

            var right = Sin(deltaRA / 2);
            right *= right;
            right *= Cos(Dec.Radians);
            right *= Cos(other.Dec.Radians);

            var separation = Sqrt(left + right);
            var result = 2 * Asin(separation);
            return Angle.FromRadians(result);
        }

        public override bool Equals(object obj)
        {
            if (obj is EquatorialCoordinate value)
            {
                return value == this;
            }

            return false;
        }

        public bool Equals(EquatorialCoordinate obj)
        {
            return
                RA.Equals(obj.RA) &&
                Dec.Equals(obj.Dec);
        }

        public override int GetHashCode()
        {
            return RA.GetHashCode() ^ Dec.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("RA={0}, Dec={1}", RA, Dec);
        }

        public static bool operator ==(
            EquatorialCoordinate left,
            EquatorialCoordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            EquatorialCoordinate left,
            EquatorialCoordinate right)
        {
            return !(left == right);
        }
    }
}
