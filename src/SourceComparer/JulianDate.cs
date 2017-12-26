// <copyright file="JulianDate.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;

namespace SourceComparer
{
    public struct JulianDate
    {
        private const double JdFromOa = 2415018.5;
        private const double OaFromJd = -JdFromOa;
        private const double JdFromMjd = 2440000.5;
        private const double MjdFromJd = -JdFromMjd;
        private const double OaFromMjd = OaFromJd - MjdFromJd;
        private const double MjdFromOa = -OaFromMjd;

        private DateTime DateTime
        {
            get;
        }

        private JulianDate(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        public static JulianDate FromJulianDate(double d)
        {
            return DateTime.FromOADate(d - OaFromJd);
        }

        public static JulianDate FromModifiedJulianDate(double d)
        {
            return DateTime.FromOADate(d - OaFromMjd);
        }

        public double ToJulianDate()
        {
            return DateTime.ToOADate() - JdFromOa;
        }

        public double ToModifiedJulianDate()
        {
            return DateTime.ToOADate() - MjdFromOa;
        }

        public static implicit operator JulianDate(DateTime dateTime)
        {
            return new JulianDate(dateTime);
        }

        public static implicit operator DateTime(JulianDate julianDate)
        {
            return julianDate.DateTime;
        }

        public override string ToString()
        {
            return DateTime.ToString();
        }
    }
}
