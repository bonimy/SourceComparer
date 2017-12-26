// <copyright file="INameDictionary.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System.Collections.Generic;

namespace SourceComparer
{
    public interface INameDictionary : IReadOnlyDictionary<string, int>
    {
        new IReadOnlyList<string> Keys { get; }

        IReadOnlyList<NameEntry> Entries { get; }
    }
}
