// <copyright file="MdexSource.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections.Generic;

namespace SourceComparer
{
    public sealed class MdexSource : Source, ISnrSource
    {
        private IReadOnlyList<object> Entries
        {
            get;
        }

        public new MdexNameDictionary Names
        {
            get
            {
                return (MdexNameDictionary)base.Names;
            }
        }

        ISourceNameDictionary ISource.Names
        {
            get
            {
                return Names;
            }
        }

        public double W1Snr
        {
            get
            {
                return (double)this[Names.SnrIndexes[0]];
            }
        }

        public double W2Snr
        {
            get
            {
                return (double)this[Names.SnrIndexes[1]];
            }
        }

        public double W3Snr
        {
            get
            {
                return (double)this[Names.SnrIndexes[2]];
            }
        }

        public double W4Snr
        {
            get
            {
                return (double)this[Names.SnrIndexes[3]];
            }
        }

        public double SignalToNoise
        {
            get
            {
                var w1 = Double.IsNaN(W1Snr) ? 0 : W1Snr;
                var w2 = Double.IsNaN(W2Snr) ? 0 : W2Snr;
                return Math.Sqrt((w1 * w1) + (w2 * w2));
            }
        }

        public MdexSource(
            MdexNameDictionary names,
            IReadOnlyList<string> values) :
            base(names, values)
        {
            var typeChecks = new Dictionary<int, Type>()
            {
                { Names.SnrIndexes[0], typeof(double) },
                { Names.SnrIndexes[1], typeof(double) },
                { Names.SnrIndexes[2], typeof(double) },
                { Names.SnrIndexes[3], typeof(double) },
            };

            foreach (var kvp in typeChecks)
            {
                var value = this[kvp.Key];
                if (value != null && value.GetType() != kvp.Value)
                {
                    throw new ArgumentException();
                }
            }
        }
    }
}
