// <copyright file="Angle.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using static System.Math;

namespace SourceComparer
{
    public struct Angle :
        IEquatable<Angle>,
        IComparable<Angle>,
        IComparable,
        IFormattable
    {
        public const double RadiansPerDegree = PI / 180.0;

        public const double DegreesPerRadian = 180.0 / PI;

        public const double ArcminutesPerDegree = 60.0;

        public const double ArcsecondsPerDegree = 3600.0;

        public const double DegreesPerArcminute =
            1 / ArcminutesPerDegree;

        public const double DegreesPerArcsecond =
            1 / ArcsecondsPerDegree;

        public const double ArcminutesPerRadian =
            ArcminutesPerDegree * DegreesPerRadian;

        public const double RadiansPerArcminute =
            RadiansPerDegree * DegreesPerArcminute;

        public const double ArcsecondsPerRadian =
            ArcsecondsPerDegree * DegreesPerRadian;

        public const double RadiansPerArcsecond =
            RadiansPerDegree * DegreesPerArcsecond;

        public double Radians
        {
            get;
        }

        public double Degrees
        {
            get
            {
                return Radians * DegreesPerRadian;
            }
        }

        public double Arcminutes
        {
            get
            {
                return Radians * ArcminutesPerRadian;
            }
        }

        public double Arcseconds
        {
            get
            {
                return Radians * ArcsecondsPerRadian;
            }
        }

        private Angle(double radians)
        {
            if (Double.IsNaN(radians))
            {
                throw new ArgumentException();
            }

            if (Double.IsInfinity(radians))
            {
                throw new ArgumentException();
            }

            Radians = radians;
        }

        public Angle KeepInRange(Angle minAngle)
        {
            var min = minAngle.Radians;
            var max = min + (2 * PI);
            var radians = Radians;

            while (radians <= min)
            {
                radians += PI;
            }

            while (radians > max)
            {
                radians -= PI;
            }

            return new Angle(radians);
        }

        public static Angle FromRadians(double value)
        {
            return new Angle(value);
        }

        public static Angle FromDegrees(double value)
        {
            return new Angle(value * RadiansPerDegree);
        }

        public static Angle FromArcminutes(double value)
        {
            return new Angle(value * RadiansPerArcminute);
        }

        public static Angle FromArcseconds(double value)
        {
            return new Angle(value * RadiansPerArcsecond);
        }

        public int CompareTo(object other)
        {
            if (other is Angle value)
            {
                return CompareTo(value);
            }

            return Radians.CompareTo(other);
        }

        public int CompareTo(Angle other)
        {
            return Radians.CompareTo(other.Radians);
        }

        public bool Equals(Angle other)
        {
            return Radians.Equals(other.Radians);
        }

        public override bool Equals(object obj)
        {
            if (obj is Angle value)
            {
                return Equals(value);
            }

            return Radians.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Radians.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format(
                "{0:0.000}°",
                Degrees);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return Degrees.ToString(format, provider);
        }

        public static bool operator ==(Angle left, Angle right)
        {
            return left.Radians == right.Radians;
        }

        public static bool operator !=(Angle left, Angle right)
        {
            return left.Radians != right.Radians;
        }

        public static bool operator >(Angle left, Angle right)
        {
            return left.Radians > right.Radians;
        }

        public static bool operator <(Angle left, Angle right)
        {
            return left.Radians < right.Radians;
        }

        public static bool operator >=(Angle left, Angle right)
        {
            return left.Radians >= right.Radians;
        }

        public static bool operator <=(Angle left, Angle right)
        {
            return left.Radians <= right.Radians;
        }

        public static Angle operator +(Angle value)
        {
            return FromRadians(+value.Radians);
        }

        public static Angle operator -(Angle value)
        {
            return FromRadians(-value.Radians);
        }

        public static Angle operator +(Angle left, Angle right)
        {
            return FromRadians(left.Radians + right.Radians);
        }

        public static Angle operator -(Angle left, Angle right)
        {
            return FromRadians(left.Radians - right.Radians);
        }

        public static Angle operator *(Angle left, double right)
        {
            return FromRadians(left.Radians * right);
        }

        public static Angle operator /(Angle left, double right)
        {
            return FromRadians(left.Radians / right);
        }

        public static Angle operator *(double left, Angle right)
        {
            return FromRadians(left * right.Radians);
        }
    }
}
