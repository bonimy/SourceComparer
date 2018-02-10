// <copyright file="SourceComparisonCallback.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia.
// </copyright>

namespace SourceComparer
{
    public delegate bool SourceComparisonCallback(
        ISource source,
        SourceList other);
}
