// <copyright file="Source.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections.Generic;

namespace SourceComparer
{
    public abstract class Source : ISource
    {
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

        protected Source(ISourceNameDictionary names, IReadOnlyList<string> values)
        {
            Names = names ?? throw new ArgumentNullException(nameof(names));

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
                var value = GetValue(values[i], entry.Format, entry.Units, entry.NullSpecifier);
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

        private static object GetValue(string text, ColumnFormat format, Unit unit, string nullSpecifier)
        {
            if (text == nullSpecifier)
            {
                if (format == ColumnFormat.Double)
                {
                    return Double.NaN;
                }

                return null;
            }

            switch (format)
            {
                case ColumnFormat.String:
                    return text;

                case ColumnFormat.Integer:
                    return Int32.Parse(text);

                case ColumnFormat.Double:
                    return Double.Parse(text);

                case ColumnFormat.Angle:
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

                case ColumnFormat.ModifiedJulianDate:
                    var days = Double.Parse(text);
                    var date = JulianDate.FromModifiedJulianDate(days);
                    return (DateTime)date;

                default:
                    return null;
            }
        }

        public IEnumerator<object> GetEnumerator()
        {
            return Entries.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
