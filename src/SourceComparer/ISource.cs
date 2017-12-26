// <copyright file="ISource.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System.Collections.Generic;

namespace SourceComparer
{
    public interface ISource : IReadOnlyList<object>
    {
        ISourceNameDictionary Names { get; }

        int Id { get; }

        Angle RA { get; }

        Angle Dec { get; }

        EquatorialCoordinate EquatorialCoordinate { get; }
    }
}
