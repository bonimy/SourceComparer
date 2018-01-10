// <copyright file="SpitzerSource.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System.Collections.Generic;
using static System.Math;

namespace SourceComparer
{
    public sealed class SpitzerSource : Source
    {
        public new SpitzerNameDictionary Names
        {
            get
            {
                return (SpitzerNameDictionary)base.Names;
            }
        }

        public double Flux1
        {
            get
            {
                return (double)this[Names.Flux1Index];
            }
        }

        public double Magnitude1
        {
            get
            {
                return -2.5 * Log10(Flux1 / 2.809E8);
            }
        }

        public double Flux2
        {
            get
            {
                return (double)this[Names.Flux2Index];
            }
        }

        public double Magnitude2
        {
            get
            {
                return -2.5 * Log10(Flux1 / 1.797E8);
            }
        }

        public SpitzerSource(SpitzerNameDictionary names, IReadOnlyList<string> values) : base(names, values)
        {
        }
    }
}
