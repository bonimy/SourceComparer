// <copyright file="MultithreadHelper.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Helper
{
    using System;
    using System.Threading.Tasks;

    public static class MultithreadHelper
    {
        public static void For(
            bool multithreaded,
            int fromInclusive,
            int toExclusive,
            Action<int> body)
        {
            if (body is null)
            {
                throw new ArgumentNullException(nameof(body));
            }

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
