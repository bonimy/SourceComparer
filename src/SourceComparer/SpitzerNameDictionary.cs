// <copyright file="SpitzerNameDictionary.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections.Generic;

namespace SourceComparer
{
    public sealed class SpitzerNameDictionary : NameDictionary, ISourceNameDictionary
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
            if (TryGetValue(IdName, out var idIndex))
            {
                IdIndex = idIndex;
            }
            else
            {
                throw new ArgumentException();
            }

            if (TryGetValue(RaName, out var raIndex))
            {
                RaIndex = raIndex;
            }
            else
            {
                throw new ArgumentException();
            }

            if (TryGetValue(DecName, out var decIndex))
            {
                DecIndex = decIndex;
            }
            else
            {
                throw new ArgumentException();
            }

            if (TryGetValue(Flux1Name, out var flux1Index))
            {
                Flux1Index = flux1Index;
            }
            else
            {
                throw new ArgumentException();
            }

            if (TryGetValue(Flux2Name, out var flux2Index))
            {
                Flux2Index = flux2Index;
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
