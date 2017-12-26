// <copyright file="SpitzerSource.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System.Collections.Generic;

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

        public SpitzerSource(SpitzerNameDictionary names, IReadOnlyList<string> values) : base(names, values)
        {
        }
    }
}
