// <copyright file="NameEntry.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Sources
{
    using System;
    using System.ComponentModel;

    public class NameEntry :
        IEquatable<NameEntry>,
        IComparable<NameEntry>,
        IComparable
    {
        public NameEntry(
            string name,
            ColumnFormat format,
            Unit units,
            string nullSpecifier)
        {
            Name = name ??
                throw new ArgumentNullException(nameof(name));

            if (!Enum.IsDefined(typeof(ColumnFormat), format))
            {
                throw new InvalidEnumArgumentException(
                    nameof(format),
                    (int)format,
                    typeof(ColumnFormat));
            }

            Format = format;

            if (!Enum.IsDefined(typeof(Unit), units))
            {
                throw new InvalidEnumArgumentException(
                    nameof(units),
                    (int)units,
                    typeof(Unit));
            }

            Units = units;

            if (Format == ColumnFormat.Double && Units == Unit.ModifiedJulianDate)
            {
                Format = ColumnFormat.ModifiedJulianDate;
            }

            NullSpecifier = nullSpecifier;
        }

        public string Name
        {
            get;
        }

        public ColumnFormat Format
        {
            get;
        }

        public Unit Units
        {
            get;
        }

        public string NullSpecifier
        {
            get;
        }

        public int CompareTo(NameEntry other)
        {
            if (other is null)
            {
                return +1;
            }

            return StringComparer.OrdinalIgnoreCase.Compare(Name, other.Name);
        }

        public int CompareTo(object obj)
        {
            if (obj is NameEntry value)
            {
                return CompareTo(value);
            }

            return +1;
        }

        public bool Equals(NameEntry other)
        {
            if (other is null)
            {
                return false;
            }

            return StringComparer.OrdinalIgnoreCase.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (obj is NameEntry value)
            {
                return Equals(value);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
