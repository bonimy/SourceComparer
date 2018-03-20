// <copyright file="SourceListComparer.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Sources
{
    using System;
    using Wcs;

    public class SourceListComparer
    {
        public SourceListComparer(
            SourceList mdex,
            SourceList spitzer,
            float snrCutOff,
            Angle searchRadius,
            bool verbose)
        {
            Mdex = mdex ??
                throw new ArgumentNullException(nameof(mdex));

            Spitzer = spitzer ??
                throw new ArgumentNullException(nameof(spitzer));

            SourceMatchLists = new SourceMatchLists(
                Mdex,
                Spitzer,
                searchRadius,
                verbose);
        }

        public SourceList Mdex
        {
            get;
        }

        public SourceList Spitzer
        {
            get;
        }

        public SourceMatchLists SourceMatchLists
        {
            get;
        }
    }
}
