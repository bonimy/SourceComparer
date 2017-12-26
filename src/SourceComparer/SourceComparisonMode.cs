// <copyright file="SourceComparisonMode.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections.Generic;
using static SourceComparer.Program;

namespace SourceComparer
{
    internal static class SourceComparisonMode
    {
        private static int Status
        {
            get;
            set;
        }

        public static bool Multithreaded
        {
            get;
            private set;
        }

        public static bool Verbose
        {
            get;
            private set;
        }

        public static bool TimeStamp
        {
            get;
            private set;
        }

        public static string LogPath
        {
            get;
            private set;
        }

        public static string PrimaryPath
        {
            get;
            private set;
        }

        public static string SecondaryPath
        {
            get;
            private set;
        }

        public static string OutputPath
        {
            get;
            private set;
        }

        public static Angle SearchRadius
        {
            get;
            private set;
        }

        public static int Run(CommandSwitches commandSwitches)
        {
            if (SetSwitches(commandSwitches) != 0)
            {
                return Status;
            }

            var primary = CreateSourceList(PrimaryPath);
            if (Status != 0)
            {
                return Status;
            }

            var secondary = CreateSourceList(SecondaryPath);
            if (Status != 0)
            {
                return Status;
            }

            var search = new SourceMatchLists(primary, secondary, SearchRadius, Verbose);

            return Status;
        }

        private static SourceList CreateSourceList(string path)
        {
            var lines = ReadAllLinesSafe(path, Verbose);
            if (lines == null)
            {
                Status = -1;
                return null;
            }

            try
            {
                if (Verbose)
                {
                    Console.WriteLine("Generating source table.");
                }

                return new SourceList(lines, Multithreaded, Verbose);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private static int SetSwitches(CommandSwitches commandSwitches)
        {
            if (commandSwitches == null)
            {
                throw new ArgumentNullException(nameof(commandSwitches));
            }

            // This information is already printed to the output stream.
            Multithreaded = commandSwitches.Multithreaded;
            Verbose = commandSwitches.Verbose;
            TimeStamp = commandSwitches.TimeStamp;
            LogPath = commandSwitches.LogPath;

            Console.WriteLine();
            Console.WriteLine("Draw mode:");

            if (commandSwitches.PrimaryPathSet)
            {
                PrimaryPath = commandSwitches.PrimaryPath;
                Console.WriteLine("Primary source path: {0}", PrimaryPath);
            }
            else
            {
                Console.WriteLine("Primary path not set.");
                Status = 1;
            }

            if (commandSwitches.SecondaryPathSet)
            {
                SecondaryPath = commandSwitches.SecondaryPath;
                Console.WriteLine("Secondary source path: {0}", SecondaryPath);
            }
            else
            {
                Console.WriteLine("Secondary source path not set.");
                Status = 1;
            }

            if (commandSwitches.OutputPathSet)
            {
                OutputPath = commandSwitches.OutputPath;
                Console.WriteLine("Matched source list path: {0}", OutputPath);
            }
            else
            {
                Console.WriteLine("Matched source list path not set.");
                Status = 1;
            }

            if (commandSwitches.SearchRadiusArcsecSet)
            {
                SearchRadius = Angle.FromArcseconds(commandSwitches.SearchRadiusArcsec);
                Console.WriteLine("Search radius: {0} asec", SearchRadius.Arcseconds);
            }
            else
            {
                Console.WriteLine("Search radius not set.");
                Status = 1;
            }

            return Status;
        }
    }
}
