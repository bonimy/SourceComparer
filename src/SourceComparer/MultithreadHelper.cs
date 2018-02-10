// <copyright file="MultithreadHelper.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia.
// </copyright>

using System;
using System.Threading.Tasks;

namespace SourceComparer
{
    public static class MultithreadHelper
    {
        public static void For(
            bool multithreaded,
            int fromInclusive,
            int toExclusive,
            Action<int> body)
        {
            if (multithreaded)
            {
                var result = Parallel.For(
                    fromInclusive,
                    toExclusive,
                    body);

                if (!result.IsCompleted)
                {
                    throw new TaskCanceledException();
                }
            }
            else
            {
                for (var i = fromInclusive; i < toExclusive; i++)
                {
                    body(i);
                }
            }
        }
    }
}
