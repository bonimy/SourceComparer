// <copyright file="MdetSource.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections.Generic;

namespace SourceComparer
{
    public sealed class MdetSource : Source, ISnrSource
    {
        private IReadOnlyList<object> Entries
        {
            get;
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

        public MdetSource(
            MdetNameDictionary names,
            IReadOnlyList<string> values) :
            base(names, values)
        {
            var snr = this[Names.SnrIndex];
            if (snr.GetType() != typeof(double))
            {
                throw new ArgumentException("Expected SNR to have type \"double\".");
            }
        }
    }
}
