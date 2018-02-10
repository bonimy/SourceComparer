// <copyright file="SpitzerNameDictionary.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System.Collections.Generic;

namespace SourceComparer
{
    public sealed class SpitzerNameDictionary : NameDictionary,
        ISourceNameDictionary
    {
        private const string IdName = "Id";
        private const string RaName = "RA";
        private const string DecName = "Dec";
        private const string Flux1Name = "flux_c1_4";
        private const string Flux2Name = "flux_c2_4";

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

        public int Flux1Index
        {
            get;
        }

        public int Flux2Index
        {
            get;
        }

        public SpitzerNameDictionary(IReadOnlyList<NameEntry> names) : base(names)
        {
            IdIndex = this[IdName];
            RaIndex = this[RaName];
            DecIndex = this[DecName];
            Flux1Index = this[Flux1Name];
            Flux2Index = this[Flux2Name];
        }
    }
}
