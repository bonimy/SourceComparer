// <copyright file="INameDictionary.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Sources
{
    using System.Collections.Generic;

    public interface INameDictionary : IReadOnlyDictionary<string, int>
    {
        new IReadOnlyList<string> Keys { get; }

        IReadOnlyList<NameEntry> Entries { get; }
    }
}
