// <copyright file="MdetNameDictionary.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Sources
{
    using System.Collections.Generic;

    public sealed class MdetNameDictionary : NameDictionary, ISourceNameDictionary
    {
        private const string IdName = "Src";
        private const string RaName = "RA";
        private const string DecName = "Dec";
        private const string SnrName = "SNR";

        public MdetNameDictionary(IReadOnlyList<NameEntry> names)
            : base(names)
        {
            IdIndex = this[IdName];
            RaIndex = this[RaName];
            DecIndex = this[DecName];
            SnrIndex = this[SnrName];
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

        public int SnrIndex
        {
            get;
        }
    }
}
