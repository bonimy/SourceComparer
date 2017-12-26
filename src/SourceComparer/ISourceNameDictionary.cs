// <copyright file="ISourceNameDictionary.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

namespace SourceComparer
{
    public interface ISourceNameDictionary : INameDictionary
    {
        int IdIndex { get; }

        int RaIndex { get; }

        int DecIndex { get; }
    }
}
