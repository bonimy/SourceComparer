// <copyright file="Program.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace SourceComparer
{
    using System;
    using System.IO;
    using Helper;

    internal static class Program
    {
        public static string[] ReadAllLinesSafe(
            string path,
            bool verbose)
        {
            try
            {
                if (verbose)
                {
                    Console.WriteLine();
                    Console.WriteLine("Opening file \"{0}\"", path);
                }

                return File.ReadAllLines(path);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private static int Main(string[] args)
        {
            var startTime = DateTime.Now;
            using (var process = GetProcess(args, out var status))
            {
                if (status != 0)
                {
                    return status;
                }

                status = process.Run();

                var elapsedTime = DateTime.Now - startTime;
                Console.WriteLine("Elapsed time: {0}", elapsedTime);

                // Explicitly flush the standard output stream (for file log).
                Console.Out.Close();

                return status;
            }
        }

        private static IProcess GetProcess(string[] args, out int status)
        {
            var commandSwitches = new CommandSwitches(args, out status);
            if (status != 0)
            {
                return null;
            }

            if (commandSwitches.CompareMode)
            {
                return new SourceComparisonMode(commandSwitches);
            }

            Console.WriteLine(
                "Mode not set (-draw, -compare, or -filter expected).");

            status = 1;
            return null;
        }
    }
}
