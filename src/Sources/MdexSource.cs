// <copyright file="MdexSource.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Sources
{
    using System;
    using System.Collections.Generic;

    public sealed class MdexSource : Source, ISnrSource, IMagSource
    {
        public MdexSource(
            MdexNameDictionary names,
            IReadOnlyList<string> values)
            : base(names, values)
        {
            var typeChecks = new Dictionary<int, Type>()
            {
                { Names.SnrIndexes[0], typeof(double) },
                { Names.SnrIndexes[1], typeof(double) },
                { Names.SnrIndexes[2], typeof(double) },
                { Names.SnrIndexes[3], typeof(double) },
                { Names.MagIndexes[0], typeof(double) },
                { Names.MagIndexes[1], typeof(double) },
                { Names.MagIndexes[2], typeof(double) },
                { Names.MagIndexes[3], typeof(double) },
            };

            foreach (var kvp in typeChecks)
            {
                var value = this[kvp.Key];
                if (value != null && value.GetType() != kvp.Value)
                {
                    throw new ArgumentException();
                }
            }
        }

        public new MdexNameDictionary Names
        {
            get
            {
                return (MdexNameDictionary)base.Names;
            }
        }

        ISourceNameDictionary ISource.Names
        {
            get
            {
                return Names;
            }
        }

        public double W1Snr
        {
            get
            {
                return (double)this[Names.SnrIndexes[0]];
            }
        }

        public double W2Snr
        {
            get
            {
                return (double)this[Names.SnrIndexes[1]];
            }
        }

        public double W3Snr
        {
            get
            {
                return (double)this[Names.SnrIndexes[2]];
            }
        }

        public double W4Snr
        {
            get
            {
                return (double)this[Names.SnrIndexes[3]];
            }
        }

        public double SignalToNoise
        {
            get
            {
                var w1 = Double.IsNaN(W1Snr) ? 0 : W1Snr;
                var w2 = Double.IsNaN(W2Snr) ? 0 : W2Snr;
                return Math.Sqrt((w1 * w1) + (w2 * w2));
            }
        }

        public double Magnitude1
        {
            get
            {
                return (double)this[Names.MagIndexes[0]];
            }
        }

        public double Magnitude2
        {
            get
            {
                return (double)this[Names.MagIndexes[1]];
            }
        }

        public double Magnitude3
        {
            get
            {
                return (double)this[Names.MagIndexes[2]];
            }
        }

        public double Magnitude4
        {
            get
            {
                return (double)this[Names.MagIndexes[3]];
            }
        }

        private IReadOnlyList<object> Entries
        {
            get;
        }
    }
}
