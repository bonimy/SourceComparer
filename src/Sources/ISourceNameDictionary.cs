// <copyright file="ISourceNameDictionary.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Sources
{
    public interface ISourceNameDictionary : INameDictionary
    {
        int IdIndex { get; }

        int RaIndex { get; }

        int DecIndex { get; }
    }
}
