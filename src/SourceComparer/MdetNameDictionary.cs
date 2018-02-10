// <copyright file="MdetNameDictionary.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System.Collections.Generic;

namespace SourceComparer
{
    public sealed class MdetNameDictionary : NameDictionary, ISourceNameDictionary
    {
        private const string IdName = "Src";
        private const string RaName = "RA";
        private const string DecName = "Dec";
        private const string SnrName = "SNR";

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

        public MdetNameDictionary(IReadOnlyList<NameEntry> names) : base(names)
        {
            IdIndex = this[IdName];
            RaIndex = this[RaName];
            DecIndex = this[DecName];
            SnrIndex = this[SnrName];
        }
    }
}
