// <copyright file="MdetSource.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Sources
{
    using System;
    using System.Collections.Generic;

    public sealed class MdetSource : Source, ISnrSource
    {
        public MdetSource(
            MdetNameDictionary names,
            IReadOnlyList<string> values)
            : base(names, values)
        {
            var snr = this[Names.SnrIndex];
            if (snr.GetType() != typeof(double))
            {
                throw new ArgumentException("Expected SNR to have type \"double\".");
            }
        }

        public new MdetNameDictionary Names
        {
            get
            {
                return (MdetNameDictionary)base.Names;
            }
        }

        public double SignalToNoise
        {
            get
            {
                return (double)this[Names.SnrIndex];
            }
        }

        private IReadOnlyList<object> Entries
        {
            get;
        }
    }
}
