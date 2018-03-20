// <copyright file="MdexNameDictionary.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Sources
{
    using System;
    using System.Collections.Generic;

    public sealed class MdexNameDictionary : NameDictionary, ISourceNameDictionary
    {
        private const string IdName = "Src";
        private const string RaName = "RA";
        private const string DecName = "Dec";

        private static readonly IReadOnlyList<string> SnrNames =
            Array.AsReadOnly(new string[]
        {
            "W1SNR",
            "W2SNR",
            "W3SNR",
            "W4SNR"
        });

        private static readonly IReadOnlyList<string> MagNames =
            Array.AsReadOnly(new string[]
        {
            "w1mag",
            "w2mag",
            "w3mag",
            "w4mag"
        });

        public MdexNameDictionary(IReadOnlyList<NameEntry> names)
            : base(names)
        {
            var snrIndexes = new int[SnrNames.Count];
            for (var i = 0; i < snrIndexes.Length; i++)
            {
                snrIndexes[i] = this[SnrNames[i]];
            }

            var magIndexes = new int[MagNames.Count];
            for (var i = 0; i < magIndexes.Length; i++)
            {
                magIndexes[i] = this[MagNames[i]];
            }

            IdIndex = this[IdName];
            RaIndex = this[RaName];
            DecIndex = this[DecName];
            SnrIndexes = Array.AsReadOnly(snrIndexes);
            MagIndexes = Array.AsReadOnly(magIndexes);
        }

        public int IdIndex
        {
            get;
        }

        public int RaIndex
        {
            get;
        }

        public int DecIndex
        {
            get;
        }

        public IReadOnlyList<int> SnrIndexes
        {
            get;
        }

        public IReadOnlyList<int> MagIndexes
        {
            get;
        }
    }
}
