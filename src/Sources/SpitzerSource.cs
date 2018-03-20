// <copyright file="SpitzerSource.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Sources
{
    using System.Collections.Generic;
    using static System.Math;

    public sealed class SpitzerSource : Source, IMagSource
    {
        public SpitzerSource(
            SpitzerNameDictionary names,
            IReadOnlyList<string> values)
            : base(names, values)
        {
        }

        public new SpitzerNameDictionary Names
        {
            get
            {
                return (SpitzerNameDictionary)base.Names;
            }
        }

        public double Flux1
        {
            get
            {
                return (double)this[Names.Flux1Index];
            }
        }

        public double Magnitude1
        {
            get
            {
                return -2.5 * Log10(Flux1 / 2.809E8);
            }
        }

        public double Flux2
        {
            get
            {
                return (double)this[Names.Flux2Index];
            }
        }

        public double Magnitude2
        {
            get
            {
                return -2.5 * Log10(Flux1 / 1.797E8);
            }
        }
    }
}
