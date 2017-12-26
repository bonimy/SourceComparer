// <copyright file="Program.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.IO;

namespace SourceComparer
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var startTime = DateTime.Now;

            var commandSwitches = new CommandSwitches(args, out var status);
            if (status != 0)
            {
                return status;
            }

            if (commandSwitches.CompareMode)
            {
                status = SourceComparisonMode.Run(commandSwitches);
            }
            else
            {
                Console.WriteLine("Mode not set (-draw, -compare, or -filter expected).");
                status = 1;
            }

            Console.WriteLine("Elapsed time: {0}", DateTime.Now - startTime);

            // Explicitly flush the standard output stream (for file log).
            Console.Out.Close();

            return status;
        }

        public static string[] ReadAllLinesSafe(string path, bool verbose)
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
    }
}
