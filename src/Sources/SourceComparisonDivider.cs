// <copyright file="SourceComparisonDivider.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Sources
{
    using System;
    using System.Collections.Generic;
    using Wcs;

    public class SourceComparisonDivider
    {
        public SourceComparisonDivider(
            SourceList mdex,
            SourceList spitzer,
            Angle searchRadius,
            double[] snrCutOffs,
            bool verbose,
            ref int status)
        {
            var helper = new Builder(
                mdex,
                spitzer,
                searchRadius,
                snrCutOffs,
                verbose,
                ref status);

            Mdex = helper.Mdex;
            Spitzer = helper.Spitzer;
            Matches = helper.Matches;
            MatchesBySnrAndRadius = helper.MatchesBySnrAndRadius;
        }

        public SourceList Mdex
        {
            get;
        }

        public SourceList Spitzer
        {
            get;
        }

        public SourceMatchLists Matches
        {
            get;
        }

        public IReadOnlyDictionary<double, IReadOnlyDictionary<Angle, SourceMatchLists>> MatchesBySnrAndRadius
        {
            get;
        }

        public Angle SearchRadius
        {
            get;
        }

        private class Builder
        {
            public Builder(
            SourceList mdex,
            SourceList spitzer,
            Angle searchRadius,
            double[] snrCutOffs,
            bool verbose,
            ref int status)
            {
                Mdex = mdex ??
                    throw new ArgumentNullException(nameof(mdex));

                Spitzer = spitzer ??
                    throw new ArgumentNullException(nameof(spitzer));

                Matches = new SourceMatchLists(
                    mdex,
                    spitzer,
                    searchRadius,
                    verbose);

                Status = status;
                status = WriteMatches(snrCutOffs);
            }

            public SourceList Mdex
            {
                get;
            }

            public SourceList Spitzer
            {
                get;
            }

            public SourceMatchLists Matches
            {
                get;
            }

            public Angle SearchRadius
            {
                get;
            }

            public Dictionary<double, IReadOnlyDictionary<Angle, SourceMatchLists>> MatchesBySnrAndRadius
            {
                get;
                set;
            }

            private bool Verbose
            {
                get;
            }

            private int Status
            {
                get;
                set;
            }

            private static bool IsInRadius(
                ISource primary,
                ISource secondary,
                Angle radius)
            {
                var x1 = primary.EquatorialCoordinate;
                var x2 = secondary.EquatorialCoordinate;

                return x1.DistanceTo(x2) <= radius;
            }

            private int WriteMatches(IEnumerable<double> snrCutOffs)
            {
                if (snrCutOffs == null)
                {
                    throw new ArgumentNullException(nameof(snrCutOffs));
                }

                MatchesBySnrAndRadius = new Dictionary<double, IReadOnlyDictionary<Angle, SourceMatchLists>>();

                foreach (var snrCutOff in snrCutOffs)
                {
                    if (WriteMatches(snrCutOff) != 0)
                    {
                        return Status;
                    }
                }

                return Status;
            }

            private int WriteMatches(double snrCutOff)
            {
                if (Verbose)
                {
                    Console.WriteLine("Dividing matches by search radius.");
                }

                var subLists = new Dictionary<Angle, SourceMatchLists>();
                for (var i = 0; i < 100; i++)
                {
                    var radius = Angle.FromArcseconds(0.1 + (i / 10.0));

                    var matches = Matches.Filter(IsInRadius);
                    subLists.Add(
                        radius,
                        matches);

                    bool IsInRadius(ISource primary, ISource secondary)
                    {
                        var snrSource = primary as ISnrSource;
                        if (snrSource.SignalToNoise < snrCutOff)
                        {
                            return false;
                        }

                        return Builder.IsInRadius(
                            primary,
                            secondary,
                            radius);
                    }
                }

                MatchesBySnrAndRadius.Add(
                    snrCutOff,
                    subLists);

                return Status;
            }
        }
    }
}
