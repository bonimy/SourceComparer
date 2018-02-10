// <copyright file="MdexNameDictionary.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System.Collections.Generic;

namespace SourceComparer
{
    public sealed class MdexNameDictionary : NameDictionary, ISourceNameDictionary
    {
        private const string IdName = "Src";
        private const string RaName = "RA";
        private const string DecName = "Dec";

        private static readonly IReadOnlyList<string> SnrNames = new string[]
        {
            "W1SNR",
            "W2SNR",
            "W3SNR",
            "W4SNR"
        };

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

        public MdexNameDictionary(IReadOnlyList<NameEntry> names) : base(names)
        {
            var snrIndexes = new int[SnrNames.Count];
            for (var i = 0; i < snrIndexes.Length; i++)
            {
                snrIndexes[i] = this[SnrNames[i]];
            }

            IdIndex = this[IdName];
            RaIndex = this[RaName];
            DecIndex = this[DecName];
            SnrIndexes = snrIndexes;
        }
    }
}
