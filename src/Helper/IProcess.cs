// <copyright file="IProcess.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Helper
{
    using System;

    public interface IProcess : IDisposable
    {
        int Run();
    }
}
