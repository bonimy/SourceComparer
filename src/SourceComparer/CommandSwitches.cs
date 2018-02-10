// <copyright file="CommandSwitches.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SourceComparer
{
    public class CommandSwitches
    {
        private static readonly IReadOnlyDictionary<string, string> ArgNames = new Dictionary<string, string>()
            {
                { "-draw"          , "Draw mode"               },
                { "-compare"       , "Source comparison mode"  },
                { "-filter"        , "Filter mode"             },
                { "-mt"            , "Multi-thread mode"       },
                { "-multithreaded" , "Multi-thread mode"       },
                { "-v"             , "Verbosity"               },
                { "-verbose"       , "Verbosity"               },
                { "-ts"            , "Time-stamp"              },
                { "-timestamp"     , "Time-stamp"              },
                { "-logpath"       , "Console log output path" },
                { "-input"         , "Input source path"       },
                { "-primary"       , "Primary source path"     },
                { "-secondary"     , "Secondary source path"   },
                { "-output"        , "Output path"             },
                { "-center_ra"     , "Center RA"               },
                { "-center_dec"    , "Center dec"              },
                { "-pixel_width"   , "Pixel width"             },
                { "-pixel_height"  , "Pixel height"            },
                { "-asecperpix_x"  , "Arcsec per pixel (X)"    },
                { "-asecperpix_y"  , "Arcsec per pixel (Y)"    },
                { "-rot_deg"       , "Rotation (degrees)"      },
                { "-searchrad_asec", "Search radius (arcsec)"  },
                { "-snrlist"       , "SNR range list"          },
            };

        private IReadOnlyDictionary<string, Action> Commands
        {
            get;
        }

        private IReadOnlyDictionary<string, Func<bool>> HashParsedArgs
        {
            get;
        }

        public IReadOnlyList<string> Args
        {
            get;
        }

        private int ArgIndex
        {
            get;
            set;
        }

        public int Status
        {
            get;
            private set;
        }

        public bool DrawMode
        {
            get;
            private set;
        }

        public bool DrawModeSet
        {
            get;
            private set;
        }

        public bool CompareMode
        {
            get;
            private set;
        }

        public bool CompareModeSet
        {
            get;
            private set;
        }

        public bool FilterMode
        {
            get;
            private set;
        }

        public bool FilterModeSet
        {
            get;
            private set;
        }

        public bool Multithreaded
        {
            get;
            private set;
        }

        public bool MultithreadedSet
        {
            get;
            private set;
        }

        public bool Verbose
        {
            get;
            private set;
        }

        public bool VerboseSet
        {
            get;
            private set;
        }

        public bool TimeStamp
        {
            get;
            private set;
        }

        public bool TimeStampSet
        {
            get;
            private set;
        }

        public string LogPath
        {
            get;
            private set;
        }

        public bool LogPathSet
        {
            get;
            private set;
        }

        public string InputPath
        {
            get;
            private set;
        }

        public bool InputPathSet
        {
            get;
            private set;
        }

        public string OutputPath
        {
            get;
            private set;
        }

        public bool OutputPathSet
        {
            get;
            private set;
        }

        public double CenterRa
        {
            get;
            private set;
        }

        public bool CenterRaSet
        {
            get;
            private set;
        }

        public double CenterDec
        {
            get;
            private set;
        }

        public bool CenterDecSet
        {
            get;
            private set;
        }

        public int PixelWidth
        {
            get;
            private set;
        }

        public bool PixelWidthSet
        {
            get;
            private set;
        }

        public int PixelHeight
        {
            get;
            private set;
        }

        public bool PixelHeightSet
        {
            get;
            private set;
        }

        public double ArcsecPerPixelX
        {
            get;
            private set;
        }

        public bool ArcsecPerPixelXSet
        {
            get;
            private set;
        }

        public double ArcsecPerPixelY
        {
            get;
            private set;
        }

        public bool ArcsecPerPixelYSet
        {
            get;
            private set;
        }

        public double RotationDegrees
        {
            get;
            private set;
        }

        public bool RotationDegreesSet
        {
            get;
            private set;
        }

        public IReadOnlyList<double> SnrRange
        {
            get;
            private set;
        }

        public bool SnrRangeSet
        {
            get;
            private set;
        }

        public string PrimaryPath
        {
            get;
            private set;
        }

        public bool PrimaryPathSet
        {
            get;
            private set;
        }

        public string SecondaryPath
        {
            get;
            private set;
        }

        public bool SecondaryPathSet
        {
            get;
            private set;
        }

        public double SearchRadiusArcsec
        {
            get;
            private set;
        }

        public bool SearchRadiusArcsecSet
        {
            get;
            private set;
        }

        public CommandSwitches(IReadOnlyList<string> args, out int status)
        {
            Args = args ??
                throw new ArgumentNullException(nameof(args));

            Commands = InitializeCommandDictionary();
            HashParsedArgs = InitializeHashParsedArgs();
            Status = status = 0;

            // We don't know where we're outputting our stream to yet, so let's create a buffer.
            var sb = new StringBuilder();

            // Save the current output stream.
            var stdout = Console.Out;

            using (var stringWriter = new StringWriter(sb))
            {
                // We're going to split the buffer line-by-line so define what its NewLine string should be.
                stringWriter.NewLine = "\n";

                // Set the new buffer stream.
                Console.SetOut(stringWriter);

                // Parse each arg in order.
                for (ArgIndex = 0; ArgIndex < Args.Count; ArgIndex++)
                {
                    // Update the status each time.
                    Status = ParseArg(Args[ArgIndex]);
                    if (Status != 0)
                    {
                        break;
                    }
                }

                // Do this to ensure all stream writes are complete.
                stringWriter.Flush();
            }

            // Go back to the original output stream to use as a base before setting the new output stream.
            Console.SetOut(stdout);
            SetOutputStream();

            // Write the buffer to the output stream line-by-line.
            var lines = sb.ToString().Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                Console.WriteLine(lines[i]);
            }

            // Terminate if nonzero status;
            if (Status != 0)
            {
                status = Status;
                return;
            }

            // Print some basic info to the console.
            if (Verbose)
            {
                PrintBasics();
            }

            status = Status;
        }

        private int SetOutputStream()
        {
            // Save the current output stream.
            var stdout = Console.Out;

            // Did we specify a log path to write to?
            if (LogPathSet)
            {
                FileStream stream;
                try
                {
                    // Try to open the stream.
                    stream = new FileStream(LogPath, FileMode.Create);
                }
                catch (IOException ex)
                {
                    // Catch any I/O error and give a non-zero exit status.
                    Console.WriteLine(ex.Message);
                    return -1;
                }

                // Set output stream to console and log file.
                var fileWriter = new StreamWriter(stream);
                stdout = new CollectionTextWriter(new TextWriter[] { Console.Out, fileWriter });
            }

            // Are we time stamping our messages?
            if (TimeStamp)
            {
                stdout = new TimeStampTextWriter(stdout);
            }

            // Set the new output stream.
            if (Console.Out != stdout)
            {
                Console.SetOut(stdout);
            }

            return Status;
        }

        private int ParseArg(string arg)
        {
            // Check if this is a recognized command.
            if (!Commands.TryGetValue(arg, out var parseCommand))
            {
                // Note: Should this return a nonzero status?
                Console.Write("Did not recognize arg: {0}", arg);
                return 0;
            }

            // Check that we haven't already set this arg.
            if (IsArgSet(arg))
            {
                return 0;
            }

            // Parse the arg.
            parseCommand();

            // Return whatever the status was.
            return Status;
        }

        private bool IsArgSet(string arg)
        {
            var isSet = HashParsedArgs[arg]();
            var name = ArgNames[arg];
            if (isSet)
            {
                // Should this cause a nonzero status?
                Console.WriteLine("{0} is already set.", name);
            }

            return isSet;
        }

        private void PrintBasics()
        {
            PrintVerboseState();
            PrintTimeStampState();
            PrintMultithreadState();
            PrintLogPathState();
        }

        private void PrintVerboseState()
        {
            if (Verbose)
            {
                Console.WriteLine("Using verbosity.");
            }
        }

        private void PrintTimeStampState()
        {
            if (TimeStamp)
            {
                Console.WriteLine("Adding timestamps to output.");
            }
        }

        private void PrintMultithreadState()
        {
            if (Multithreaded)
            {
                Console.WriteLine("Multi-thread mode enabled.");
            }
            else
            {
                Console.WriteLine("Single-thread mode.");
            }
        }

        private void PrintLogPathState()
        {
            if (LogPathSet)
            {
                Console.WriteLine("Output log path: {0}", LogPath);
            }
        }

        private void PrintInputPathState()
        {
            if (InputPathSet)
            {
                Console.WriteLine("Input path: {0}", InputPath);
            }
            else
            {
                Console.WriteLine("Input path not provided.");
                Status = 1;
            }
        }

        private void PrintOutputPathState()
        {
            if (OutputPathSet)
            {
                Console.WriteLine("Output image path: {0}", OutputPath);
            }
            else
            {
                Console.WriteLine("Output image path not provided.");
                Status = 1;
            }
        }

        private void PrintCenterCoordinatesState()
        {
            if (CenterRaSet)
            {
                Console.WriteLine("Center RA: {0}", CenterRa);
            }
            else
            {
                Console.WriteLine("Center RA not set.");
                Status = 1;
            }

            if (CenterDecSet)
            {
                Console.WriteLine("Center Dec: {0}", CenterDec);
            }
            else
            {
                Console.WriteLine("Center Dec not set.");
                Status = 1;
            }
        }

        private void PrintPixelSizeState()
        {
            if (PixelWidthSet)
            {
                Console.WriteLine("Image width (pixels): {0}", PixelWidth);
            }
            else
            {
                Console.WriteLine("Image width not set.");
                Status = 1;
            }

            if (PixelHeightSet)
            {
                Console.WriteLine("Image height (pixels): {0}", PixelHeight);
            }
            else
            {
                Console.Write("Image height not set.");
                Status = 1;
            }
        }

        private void PrintPixelResolutionState()
        {
            if (ArcsecPerPixelXSet)
            {
                Console.WriteLine("Arcsec per pixel (X): {0}", ArcsecPerPixelX);
            }
            else
            {
                Console.WriteLine("Arcsec per pixel (X) not set.");
                Status = 1;
            }

            if (ArcsecPerPixelYSet)
            {
                Console.WriteLine("Arcsec per pixel (Y): {0}", ArcsecPerPixelY);
            }
            else
            {
                Console.WriteLine("Arcsec per pixel (Y) not set.");
                Status = 1;
            }
        }

        private void PrintRotationAngleState()
        {
            if (RotationDegreesSet)
            {
                Console.WriteLine("Rotation (degrees): {0}", RotationDegrees);
            }
            else
            {
                Console.WriteLine("Rotation not set, assuming zero.");
            }
        }

        private void PrintSnrRangeState()
        {
            if (SnrRangeSet)
            {
                if (SnrRange.Count != 0)
                {
                    var sb = new StringBuilder(SnrRange[0].ToString("0.0E0"));
                    for (var i = 1; i < SnrRange.Count; i++)
                    {
                        sb.Append(' ');
                        sb.Append(SnrRange[i].ToString("0.0E0"));
                    }

                    Console.WriteLine("SNR range: {0}", sb.ToString());
                    return;
                }
            }

            Console.WriteLine("SNR range not set. Will take best guess.");
        }

        private void PrintDrawMode()
        {
            Console.WriteLine("Draw mode:");
            PrintBasics();

            PrintInputPathState();
            PrintOutputPathState();
            PrintCenterCoordinatesState();
            PrintPixelSizeState();
            PrintPixelResolutionState();
            PrintRotationAngleState();
            PrintSnrRangeState();
        }

        private string NextString()
        {
            if (Status != 0)
            {
                return null;
            }

            if (ArgIndex + 1 >= Args.Count)
            {
                Console.WriteLine("Could not get value (end of list.)");
                Status = 2;
                return null;
            }

            return Args[++ArgIndex];
        }

        private double ParseDouble()
        {
            var text = NextString();
            if (Status != 0)
            {
                return Double.NaN;
            }

            if (Double.TryParse(text, out var result))
            {
                return result;
            }

            Status = 1;
            return Double.NaN;
        }

        private int ParseInt()
        {
            var text = NextString();
            if (Status != 0)
            {
                return 0;
            }

            if (Int32.TryParse(text, out var result))
            {
                return result;
            }

            Status = 1;
            return 0;
        }

        private IReadOnlyList<double> ParseDoubleList()
        {
            var text = NextString();
            if (Status != 0)
            {
                return null;
            }

            var values = text.Split(',');
            var result = new double[values.Length];
            for (var i = 0; i < result.Length; i++)
            {
                if (!Double.TryParse(values[i], out var value))
                {
                    Status = 1;
                    return null;
                }

                result[i] = value;
            }

            return result;
        }

        private void SetDrawMode()
        {
            if (IsArgSet("-compare") ||
                IsArgSet("-filter"))
            {
                return;
            }

            DrawMode = true;
            DrawModeSet = true;
        }

        private void SetCompareMode()
        {
            if (IsArgSet("-draw") ||
                IsArgSet("-filter"))
            {
                return;
            }

            CompareMode = true;
            CompareModeSet = true;
        }

        private void SetFilterMode()
        {
            if (IsArgSet("-draw") ||
                IsArgSet("-compare"))
            {
                return;
            }

            FilterMode = true;
            FilterModeSet = true;
        }

        private void SetMultithreaded()
        {
            Multithreaded = true;
            MultithreadedSet = true;
        }

        private void SetVerbose()
        {
            Verbose = true;
            VerboseSet = true;
        }

        private void SetTimeStamp()
        {
            TimeStamp = true;
            TimeStampSet = true;
        }

        private void SetLogPath()
        {
            LogPath = NextString();
            LogPathSet = true;
        }

        private void SetInputPath()
        {
            InputPath = NextString();
            InputPathSet = true;
        }

        private void SetOutputPath()
        {
            OutputPath = NextString();
            OutputPathSet = true;
        }

        private void SetCenterRa()
        {
            CenterRa = ParseDouble();
            CenterRaSet = true;
        }

        private void SetCenterDec()
        {
            CenterDec = ParseDouble();
            CenterDecSet = true;
        }

        private void SetPixelWidth()
        {
            PixelWidth = ParseInt();
            PixelWidthSet = true;
        }

        private void SetPixelHeight()
        {
            PixelHeight = ParseInt();
            PixelHeightSet = true;
        }

        private void SetArcsecPerPixelX()
        {
            ArcsecPerPixelX = ParseDouble();
            ArcsecPerPixelXSet = true;
        }

        private void SetArcsecPerPixelY()
        {
            ArcsecPerPixelY = ParseDouble();
            ArcsecPerPixelYSet = true;
        }

        private void SetRotationDegrees()
        {
            RotationDegrees = ParseDouble();
            RotationDegreesSet = true;
        }

        private void SetSnrRange()
        {
            SnrRange = ParseDoubleList();
            SnrRangeSet = true;
        }

        private void SetPrimaryPath()
        {
            PrimaryPath = NextString();
            PrimaryPathSet = true;
        }

        private void SetSecondaryPath()
        {
            SecondaryPath = NextString();
            SecondaryPathSet = true;
        }

        private void SetSeacrhRadiusArcsec()
        {
            SearchRadiusArcsec = ParseDouble();
            SearchRadiusArcsecSet = true;
        }

        private IReadOnlyDictionary<string, Action> InitializeCommandDictionary()
        {
            return new Dictionary<string, Action>()
            {
                { "-draw"          , SetDrawMode           },
                { "-compare"       , SetCompareMode        },
                { "-filter"        , SetFilterMode         },
                { "-mt"            , SetMultithreaded      },
                { "-multithreaded" , SetMultithreaded      },
                { "-v"             , SetVerbose            },
                { "-verbose"       , SetVerbose            },
                { "-ts"            , SetTimeStamp          },
                { "-timestamp"     , SetTimeStamp          },
                { "-logpath"       , SetLogPath            },
                { "-input"         , SetInputPath          },
                { "-primary"       , SetPrimaryPath        },
                { "-secondary"     , SetSecondaryPath      },
                { "-output"        , SetOutputPath         },
                { "-center_ra"     , SetCenterRa           },
                { "-center_dec"    , SetCenterDec          },
                { "-pixel_width"   , SetPixelWidth         },
                { "-pixel_height"  , SetPixelHeight        },
                { "-asecperpix_x"  , SetArcsecPerPixelX    },
                { "-asecperpix_y"  , SetArcsecPerPixelY    },
                { "-rot_deg"       , SetRotationDegrees    },
                { "-searchrad_asec", SetSeacrhRadiusArcsec },
                { "-snrlist"       , SetSnrRange           },
            };
        }

        private IReadOnlyDictionary<string, Func<bool>> InitializeHashParsedArgs()
        {
            return new Dictionary<string, Func<bool>>()
            {
                { "-draw"          , () => DrawModeSet          },
                { "-compare"       , () => CompareModeSet       },
                { "-filter"        , () => FilterModeSet        },
                { "-mt"            , () => MultithreadedSet     },
                { "-multithreaded" , () => MultithreadedSet     },
                { "-v"             , () => VerboseSet           },
                { "-verbose"       , () => VerboseSet           },
                { "-ts"            , () => TimeStampSet         },
                { "-timestamp"     , () => TimeStampSet         },
                { "-logpath"       , () => LogPathSet           },
                { "-input"         , () => InputPathSet         },
                { "-primary"       , () => PrimaryPathSet       },
                { "-secondary"     , () => SecondaryPathSet     },
                { "-output"        , () => OutputPathSet        },
                { "-center_ra"     , () => CenterRaSet          },
                { "-center_dec"    , () => CenterDecSet         },
                { "-pixel_width"   , () => PixelWidthSet        },
                { "-pixel_height"  , () => PixelHeightSet       },
                { "-asecperpix_x"  , () => ArcsecPerPixelXSet   },
                { "-asecperpix_y"  , () => ArcsecPerPixelYSet   },
                { "-rot_deg"       , () => RotationDegreesSet   },
                { "-searchrad_asec", () => SearchRadiusArcsecSet},
                { "-snrlist"       , () => SnrRangeSet          },
            };
        }
    }
}
