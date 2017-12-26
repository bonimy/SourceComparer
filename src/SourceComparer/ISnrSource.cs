// <copyright file="ISnrSource.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

namespace SourceComparer
{
    public interface ISnrSource : ISource
    {
        double SignalToNoise { get; }
    }
}
