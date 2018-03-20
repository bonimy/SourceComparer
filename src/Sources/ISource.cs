// <copyright file="ISource.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Sources
{
    using System.Collections.Generic;
    using Wcs;

    public interface ISource : IReadOnlyList<object>
    {
        ISourceNameDictionary Names { get; }

        int Id { get; }

        Angle RA { get; }

        Angle Dec { get; }

        EquatorialCoordinate EquatorialCoordinate { get; }
    }
}
