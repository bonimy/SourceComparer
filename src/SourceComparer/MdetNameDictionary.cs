// <copyright file="MdetNameDictionary.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
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

            if (TryGetValue(SnrName, out var snrIndex))
            {
                SnrIndex = snrIndex;
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
