// <copyright file="Source.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace SourceComparer
{
    public abstract class Source : ISource
    {
        private static readonly IReadOnlyDictionary<ColumnFormat, ParseColumnCallback> ParseDictionary = new Dictionary<ColumnFormat, ParseColumnCallback>()
        {
            { ColumnFormat.String, (text, unit) => text },
            { ColumnFormat.Integer, (text, unit) => Int32.Parse(text) },
            { ColumnFormat.Double, (text, unit) => Double.Parse(text) },
            { ColumnFormat.Angle, ParseAngle },
            { ColumnFormat.ModifiedJulianDate, (text, unit) => ParseDateTime(text) }
        };

        private IReadOnlyList<object> Entries
        {
            get;
        }

        public int Count
        {
            get
            {
                return Entries.Count;
            }
        }

        public ISourceNameDictionary Names
        {
            get;
        }

        public int Id
        {
            get
            {
                return (int)this[Names.IdIndex];
            }
        }

        public Angle RA
        {
            get
            {
                return (Angle)this[Names.RaIndex];
            }
        }

        public Angle Dec
        {
            get
            {
                return (Angle)this[Names.DecIndex];
            }
        }

        public EquatorialCoordinate EquatorialCoordinate
        {
            get
            {
                return new EquatorialCoordinate(RA, Dec);
            }
        }

        public object this[int index]
        {
            get
            {
                return Entries[index];
            }
        }

        protected Source(
            ISourceNameDictionary names,
            IReadOnlyList<string> values)
        {
            Names = names ??
                throw new ArgumentNullException(nameof(names));

            if (values.Count != names.Count)
            {
                throw new ArgumentException();
            }

            var typeChecks = new Dictionary<int, Type>()
            {
                { Names.RaIndex, typeof(Angle) },
                { Names.DecIndex, typeof(Angle) },
                { Names.IdIndex, typeof(int) }
            };

            var result = new object[values.Count];
            for (var i = 0; i < values.Count; i++)
            {
                var entry = names.Entries[i];
                var value = GetValue(
                    values[i],
                    entry.Format,
                    entry.Units,
                    entry.NullSpecifier);

                result[i] = value;
            }

            Entries = result;
            foreach (var kvp in typeChecks)
            {
                if (this[kvp.Key].GetType() != kvp.Value)
                {
                    throw new ArgumentException();
                }
            }
        }

        private static object GetValue(
            string text,
            ColumnFormat format,
            Unit unit,
            string nullSpecifier)
        {
            if (text == nullSpecifier)
            {
                if (format == ColumnFormat.Double)
                {
                    return Double.NaN;
                }

                return null;
            }

            if (ParseDictionary.TryGetValue(format, out var parse))
            {
                return parse(text, unit);
            }

            return null;
        }

        private static object ParseAngle(string text, Unit unit)
        {
            var angle = Double.Parse(text);
            switch (unit)
            {
                case Unit.ArcSeconds:
                    return Angle.FromArcseconds(angle);

                case Unit.Degrees:
                    return Angle.FromDegrees(angle);

                default:
                    return angle;
            }
        }

        public IEnumerator<object> GetEnumerator()
        {
            return Entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return Id.ToString();
        }

        private static DateTime ParseDateTime(string text)
        {
            var days = Double.Parse(text);
            var date = JulianDate.FromModifiedJulianDate(days);
            return date;
        }
    }
}
