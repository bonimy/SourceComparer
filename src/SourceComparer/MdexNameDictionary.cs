// <copyright file="MdexNameDictionary.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
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

            var snrIndexes = new int[SnrNames.Count];
            for (var i = 0; i < snrIndexes.Length; i++)
            {
                if (TryGetValue(SnrNames[i], out var snrIndex))
                {
                    snrIndexes[i] = snrIndex;
                }
                else
                {
                    throw new ArgumentException();
                }
            }

            SnrIndexes = snrIndexes;
        }
    }
}
