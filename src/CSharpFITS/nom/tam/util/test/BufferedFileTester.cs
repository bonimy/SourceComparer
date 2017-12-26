// <copyright file="BufferedFileTester.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

namespace nom.tam.util.test
{
    using System;
    using System.IO;
    using nom.tam.util;

    /// <summary>This class provides runs tests of the
    /// BufferedI/O classes: BufferedFile, BufferedDataStream
    /// and BufferedDataOutputStream.  A limited comparison
    /// to the standard I/O classes can also be made.
    /// <p>
    /// Input and output of all primitive scalar and array types is
    /// tested, however input and output of String data is not.
    /// Users may choose to test the BufferedFile class, the
    /// BufferedDataXPUT classes array methods, the BufferedDataXPUT
    /// classes using the methods of DataXput, the traditional
    /// I/O classes, or any combination thereof.
    /// </summary>
    public class BufferedFileTester
    {
        /// <summary>Usage: java nom.tam.util.test.BufferedFileTester file [dim [numIts [flags]]]
        /// where
        /// file 	is the file to be read and written.
        /// dim 	is the dimension of the arrays to be written.
        /// numIts 	is the number of times each array is written.
        /// flags	a string indicating what I/O to test
        /// O  -- test old I/O (RandomAccessFile and standard streams)
        /// R  -- BufferedFile (i.e., random access)
        /// S  -- BufferedDataXPutStream
        /// X  -- BufferedDataXPutStream using standard methods
        /// </summary>
        [STAThread]
        public static void Test(System.String[] args)
        {
            var filename = args[0];
            var dim = 1000;
            if (args.Length > 1)
            {
                dim = System.Int32.Parse(args[1]);
            }
            var numIts = 1;
            if (args.Length > 2)
            {
                numIts = System.Int32.Parse(args[2]);
            }

            System.Console.Out.WriteLine("Allocating arrays.");
            var db = new double[dim];
            var fl = new float[dim];
            var in_Renamed = new int[dim];
            var ln = new long[dim];
            var sh = new short[dim];
            var by = new byte[dim];
            var ch = new char[dim];
            var bl = new bool[dim];

            System.Console.Out.WriteLine("Initializing arrays -- may take a while");
            var sign = 1;
            for (var i = 0; i < dim; i += 1)
            {
                var x = sign * System.Math.Pow(10.0, 20 * SupportClass.Random.NextDouble() - 10);
                db[i] = x;
                fl[i] = (float)x;

                if (System.Math.Abs(x) < 1)
                {
                    x = 1 / x;
                }

                in_Renamed[i] = (int)x;
                ln[i] = (long)x;
                sh[i] = (short)x;
                by[i] = (byte)x;

                //ch[i] = (char) x;
                ch[i] = SupportClass.NextChar();
                bl[i] = x > 0;

                sign = -sign;
            }

            // Ensure special values are tested.

            by[0] = (byte)System.Byte.MinValue;
            by[1] = (byte)System.Byte.MaxValue;
            by[2] = 0;
            ch[0] = System.Char.MinValue;
            ch[1] = System.Char.MaxValue;
            ch[2] = (char)(0);
            sh[0] = System.Int16.MaxValue;
            sh[1] = System.Int16.MinValue;
            sh[0] = 0;
            in_Renamed[0] = System.Int32.MaxValue;
            in_Renamed[1] = System.Int32.MinValue;
            in_Renamed[2] = 0;
            ln[0] = System.Int64.MinValue;
            ln[1] = System.Int64.MaxValue;
            ln[2] = 0;
            fl[0] = System.Single.Epsilon;
            fl[1] = System.Single.MaxValue;
            fl[2] = System.Single.PositiveInfinity;
            fl[3] = System.Single.NegativeInfinity;
            fl[4] = System.Single.NaN;
            fl[5] = 0;
            db[0] = System.Double.MinValue;
            db[1] = System.Double.MaxValue;
            db[2] = System.Double.PositiveInfinity;
            db[3] = System.Double.NegativeInfinity;
            db[4] = System.Double.NaN;
            db[5] = 0;

            var db2 = new double[dim];
            var fl2 = new float[dim];
            var in2 = new int[dim];
            var ln2 = new long[dim];
            var sh2 = new short[dim];
            var by2 = new byte[dim];
            var ch2 = new char[dim];
            var bl2 = new bool[dim];

            var multi = new int[10][][][];
            for (var i2 = 0; i2 < 10; i2++)
            {
                multi[i2] = new int[10][][];
                for (var i3 = 0; i3 < 10; i3++)
                {
                    multi[i2][i3] = new int[10][];
                    for (var i4 = 0; i4 < 10; i4++)
                    {
                        multi[i2][i3][i4] = new int[10];
                    }
                }
            }
            var multi2 = new int[10][][][];
            for (var i5 = 0; i5 < 10; i5++)
            {
                multi2[i5] = new int[10][][];
                for (var i6 = 0; i6 < 10; i6++)
                {
                    multi2[i5][i6] = new int[10][];
                    for (var i7 = 0; i7 < 10; i7++)
                    {
                        multi2[i5][i6][i7] = new int[10];
                    }
                }
            }
            for (var i = 0; i < 10; i += 1)
            {
                multi[i][i][i][i] = i;
            }

            if (args.Length < 4 || args[3].IndexOf((System.Char)'O') >= 0)
            {
                standardFileTest(filename, numIts, in_Renamed, in2);
                standardStreamTest(filename, numIts, in_Renamed, in2);
            }

            if (args.Length < 4 || args[3].IndexOf((System.Char)'X') >= 0)
            {
                buffStreamSimpleTest(filename, numIts, in_Renamed, in2);
            }

            if (args.Length < 4 || args[3].IndexOf((System.Char)'R') >= 0)
            {
                bufferedFileTest(filename, numIts, db, db2, fl, fl2, ln, ln2, in_Renamed, in2, sh, sh2, ch, ch2, by, by2, bl, bl2, multi, multi2);
            }

            if (args.Length < 4 || args[3].IndexOf((System.Char)'S') >= 0)
            {
                bufferedStreamTest(filename, numIts, db, db2, fl, fl2, ln, ln2, in_Renamed, in2, sh, sh2, ch, ch2, by, by2, bl, bl2, multi, multi2);
            }
        }

        public static void standardFileTest(System.String filename, int numIts, int[] in_Renamed, int[] in2)
        {
            System.Console.Out.WriteLine("Standard I/O library: java.io.RandomAccessFile");

            var f = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var dim = in_Renamed.Length;
            resetTime();
            f.Seek(0, SeekOrigin.Begin);
            for (var j = 0; j < numIts; j += 1)
            {
                for (var i = 0; i < dim; i += 1)
                {
                    BinaryWriter temp_BinaryWriter;
                    temp_BinaryWriter = new BinaryWriter(f);
                    temp_BinaryWriter.Write((System.Int32)in_Renamed[i]);

                    //f.Write((System.Int32) in_Renamed[i]);
                }
            }
            System.Console.Out.WriteLine("  RAF Int write: " + (4 * dim * numIts) / (1000 * deltaTime()));
            f.Seek(0, SeekOrigin.Begin);
            resetTime();
            for (var j = 0; j < numIts; j += 1)
            {
                for (var i = 0; i < dim; i += 1)
                {
                    BinaryReader temp_BinaryReader;
                    temp_BinaryReader = new BinaryReader(f);
                    temp_BinaryReader.BaseStream.Position = f.Position;
                    in2[i] = temp_BinaryReader.ReadInt32();

                    //in2[i] = f.ReadInt32();
                }
            }
            System.Console.Out.WriteLine("  RAF Int read:  " + (4 * dim * numIts) / (1000 * deltaTime()));

            lock (f)
            {
                f.Seek(0, SeekOrigin.Begin);
                for (var j = 0; j < numIts; j += 1)
                {
                    for (var i = 0; i < dim; i += 1)
                    {
                        BinaryWriter temp_BinaryWriter;
                        temp_BinaryWriter = new BinaryWriter(f);
                        temp_BinaryWriter.Write((Int32)in_Renamed[i]);

                        //f.Write((Int32)in_Renamed[i]);
                    }
                }
                System.Console.Out.WriteLine("  SyncRAF Int write: " + (4 * dim * numIts) / (1000 * deltaTime()));
                f.Seek(0, SeekOrigin.Begin);
                resetTime();
                for (var j = 0; j < numIts; j += 1)
                {
                    for (var i = 0; i < dim; i += 1)
                    {
                        BinaryReader temp_BinaryReader2;
                        temp_BinaryReader2 = new BinaryReader(f);
                        temp_BinaryReader2.BaseStream.Position = f.Position;
                        in2[i] = temp_BinaryReader2.ReadInt32();

                        //in2[i] = f.ReadInt32();
                    }
                }
            }
            System.Console.Out.WriteLine("  SyncRAF Int read:  " + (4 * dim * numIts) / (1000 * deltaTime()));
        }

        public static void standardStreamTest(System.String filename, int numIts, int[] in_Renamed, int[] in2)
        {
            System.Console.Out.WriteLine("Standard I/O library: java.io.DataXXputStream");
            System.Console.Out.WriteLine("                      layered atop a BufferedXXputStream");

            var f = new BinaryWriter(new BufferedStream(new FileStream(filename, FileMode.Create), 32768));
            resetTime();
            var dim = in_Renamed.Length;
            for (var j = 0; j < numIts; j += 1)
            {
                for (var i = 0; i < dim; i += 1)
                {
                    f.Write(in_Renamed[i]);
                }
            }
            f.Flush();
            f.Close();
            System.Console.Out.WriteLine("  DIS Int write: " + (4 * dim * numIts) / (1000 * deltaTime()));

            var is_Renamed = new BinaryReader(new BufferedStream(new FileStream(filename, FileMode.Open, FileAccess.Read), 32768));
            resetTime();
            for (var j = 0; j < numIts; j += 1)
            {
                for (var i = 0; i < dim; i += 1)
                {
                    in2[i] = is_Renamed.ReadInt32();
                }
            }
            System.Console.Out.WriteLine("  DIS Int read:  " + (4 * dim * numIts) / (1000 * deltaTime()));

            f = new BinaryWriter(new BufferedStream(new FileStream(filename, FileMode.Create), 32768));
            resetTime();
            dim = in_Renamed.Length;
            lock (f)
            {
                for (var j = 0; j < numIts; j += 1)
                {
                    for (var i = 0; i < dim; i += 1)
                    {
                        f.Write(in_Renamed[i]);
                    }
                }
                f.Flush();
                f.Close();
                System.Console.Out.WriteLine("  DIS Int write: " + (4 * dim * numIts) / (1000 * deltaTime()));

                is_Renamed = new BinaryReader(new BufferedStream(new FileStream(filename, FileMode.Open, FileAccess.Read), 32768));
                resetTime();
                for (var j = 0; j < numIts; j += 1)
                {
                    for (var i = 0; i < dim; i += 1)
                    {
                        in2[i] = is_Renamed.ReadInt32();
                    }
                }
            }
            System.Console.Out.WriteLine("  DIS Int read:  " + (4 * dim * numIts) / (1000 * deltaTime()));
        }

        public static void buffStreamSimpleTest(System.String filename, int numIts, int[] in_Renamed, int[] in2)
        {
            System.Console.Out.WriteLine("New libraries:  nom.tam.BufferedDataXXputStream");
            System.Console.Out.WriteLine("                Using non-array I/O");
            var f = new BufferedDataStream(new FileStream(filename, FileMode.Create), 32768);
            resetTime();
            var dim = in_Renamed.Length;
            for (var j = 0; j < numIts; j += 1)
            {
                for (var i = 0; i < dim; i += 1)
                {
                    f.Write(in_Renamed[i]);
                }
            }
            f.Flush();
            f.Close();
            System.Console.Out.WriteLine("  BDS Int write: " + (4 * dim * numIts) / (1000 * deltaTime()));

            var is_Renamed = new BufferedDataStream(new BufferedStream(new FileStream(filename, FileMode.Open, FileAccess.Read), 32768));
            resetTime();
            for (var j = 0; j < numIts; j += 1)
            {
                for (var i = 0; i < dim; i += 1)
                {
                    in2[i] = is_Renamed.ReadInt32();
                }
            }
            System.Console.Out.WriteLine("  BDS Int read:  " + (4 * dim * numIts) / (1000 * deltaTime()));
        }

        public static void bufferedStreamTest(System.String filename, int numIts, double[] db, double[] db2, float[] fl, float[] fl2, long[] ln, long[] ln2, int[] in_Renamed, int[] in2, short[] sh, short[] sh2, char[] ch, char[] ch2, byte[] by, byte[] by2, bool[] bl, bool[] bl2, int[][][][] multi, int[][][][] multi2)
        {
            var dim = db.Length;

            var ds = SupportClass.Random.NextDouble() - 0.5;
            double ds2;
            var fs = (float)(SupportClass.Random.NextDouble() - 0.5);
            float fs2;
            var is_Renamed = (int)(1000000 * (SupportClass.Random.NextDouble() - 500000));
            int is2;
            var ls = (long)(100000000000L * (SupportClass.Random.NextDouble() - 50000000000L));
            long ls2;
            var ss = (short)(60000 * (SupportClass.Random.NextDouble() - 30000));
            short ss2;
            var cs = (char)(60000 * SupportClass.Random.NextDouble());
            char cs2;
            var bs = (byte)(256 * SupportClass.Random.NextDouble() - 128);
            byte bs2;
            var bls = (SupportClass.Random.NextDouble() > 0.5);
            bool bls2;
            System.Console.Out.WriteLine("New libraries: nom.tam.util.BufferedDataXXputStream");
            System.Console.Out.WriteLine("               Using array I/O methods");

            {
                var f = new BufferedDataStream(new FileStream(filename, FileMode.Create));

                resetTime();
                for (var i = 0; i < numIts; i += 1)
                {
                    f.WriteArray(db);
                }

                System.Console.Out.WriteLine("  BDS Dbl write: " + (8 * dim * numIts) / (1000 * deltaTime()));
                for (var i = 0; i < numIts; i += 1)
                {
                    f.WriteArray(fl);
                }

                System.Console.Out.WriteLine("  BDS Flt write: " + (4 * dim * numIts) / (1000 * deltaTime()));
                for (var i = 0; i < numIts; i += 1)
                {
                    f.WriteArray(in_Renamed);
                }

                System.Console.Out.WriteLine("  BDS Int write: " + (4 * dim * numIts) / (1000 * deltaTime()));
                for (var i = 0; i < numIts; i += 1)
                {
                    f.WriteArray(ln);
                }

                System.Console.Out.WriteLine("  BDS Lng write: " + (8 * dim * numIts) / (1000 * deltaTime()));
                for (var i = 0; i < numIts; i += 1)
                {
                    f.WriteArray(sh);
                }

                System.Console.Out.WriteLine("  BDS Sht write: " + (2 * dim * numIts) / (1000 * deltaTime()));
                for (var i = 0; i < numIts; i += 1)
                {
                    f.WriteArray(ch);
                }

                System.Console.Out.WriteLine("  BDS Chr write: " + (2 * dim * numIts) / (1000 * deltaTime()));
                for (var i = 0; i < numIts; i += 1)
                {
                    f.WriteArray((byte[])by);
                }

                System.Console.Out.WriteLine("  BDS Byt write: " + (1 * dim * numIts) / (1000 * deltaTime()));
                for (var i = 0; i < numIts; i += 1)
                {
                    f.WriteArray(bl);
                }

                System.Console.Out.WriteLine("  BDS Boo write: " + (1 * dim * numIts) / (1000 * deltaTime()));

                f.Write((byte)bs);
                f.Write((System.Char)cs);
                f.Write((System.Int16)ss);
                f.Write(is_Renamed);
                f.Write(ls);
                f.Write(fs);
                f.Write(ds);
                f.Write(bls);

                f.WriteArray(multi);
                f.Flush();
                f.Close();
            }

            {
                var f = new BufferedDataStream(new FileStream(filename, FileMode.Open, FileAccess.Read));

                resetTime();
                for (var i = 0; i < numIts; i += 1)
                {
                    f.ReadArray(db2);
                }

                System.Console.Out.WriteLine("  BDS Dbl read:  " + (8 * dim * numIts) / (1000 * deltaTime()));
                for (var i = 0; i < numIts; i += 1)
                {
                    f.ReadArray(fl2);
                }

                System.Console.Out.WriteLine("  BDS Flt read:  " + (4 * dim * numIts) / (1000 * deltaTime()));
                for (var i = 0; i < numIts; i += 1)
                {
                    f.ReadArray(in2);
                }

                System.Console.Out.WriteLine("  BDS Int read:  " + (4 * dim * numIts) / (1000 * deltaTime()));
                for (var i = 0; i < numIts; i += 1)
                {
                    f.ReadArray(ln2);
                }

                System.Console.Out.WriteLine("  BDS Lng read:  " + (8 * dim * numIts) / (1000 * deltaTime()));
                for (var i = 0; i < numIts; i += 1)
                {
                    f.ReadArray(sh2);
                }

                System.Console.Out.WriteLine("  BDS Sht read:  " + (2 * dim * numIts) / (1000 * deltaTime()));
                for (var i = 0; i < numIts; i += 1)
                {
                    f.ReadArray(ch2);
                }

                System.Console.Out.WriteLine("  BDS Chr read:  " + (2 * dim * numIts) / (1000 * deltaTime()));
                for (var i = 0; i < numIts; i += 1)
                {
                    f.ReadArray((byte[])by2);
                }

                System.Console.Out.WriteLine("  BDS Byt read:  " + (1 * dim * numIts) / (1000 * deltaTime()));
                for (var i = 0; i < numIts; i += 1)
                {
                    f.ReadArray(bl2);
                }

                System.Console.Out.WriteLine("  BDS Boo read:  " + (1 * dim * numIts) / (1000 * deltaTime()));

                bs2 = (byte)f.ReadByte();
                cs2 = f.ReadChar();
                ss2 = f.ReadInt16();
                is2 = f.ReadInt32();
                ls2 = f.ReadInt64();
                fs2 = f.ReadSingle();
                ds2 = f.ReadDouble();
                bls2 = f.ReadBoolean();

                for (var i = 0; i < 10; i += 1)
                {
                    multi2[i][i][i][i] = 0;
                }

                // Now read only pieces of the multidimensional array.
                for (var i = 0; i < 5; i += 1)
                {
                    System.Console.Out.WriteLine("Multiread:" + i);

                    // Skip the odd initial indices and
                    // read the evens.
                    //BinaryReader temp_BinaryReader;
                    System.Int64 temp_Int64;

                    //temp_BinaryReader = f;
                    temp_Int64 = f.Position;  //temp_BinaryReader.BaseStream.Position;
                    temp_Int64 = f.Seek(4000) - temp_Int64;  //temp_BinaryReader.BaseStream.Seek(4000, SeekOrigin.Current) - temp_Int64;
                    var generatedAux28 = (int)temp_Int64;
                    f.ReadArray(multi2[2 * i + 1]);
                }
                f.Close();
            }

            System.Console.Out.WriteLine("Stream Verification:");
            System.Console.Out.WriteLine("  An error should be reported for double and float NaN's");
            System.Console.Out.WriteLine("  Arrays:");

            for (var i = 0; i < dim; i += 1)
            {
                if (db[i] != db2[i] && !Double.IsNaN(db[i]) && !Double.IsNaN(db2[i]))
                {
                    System.Console.Out.WriteLine("     Double error at " + i + " " + db[i] + " " + db2[i]);
                }
                if (fl[i] != fl2[i] && !Single.IsNaN(fl[i]) && !Single.IsNaN(fl2[i]))
                {
                    System.Console.Out.WriteLine("     Float error at " + i + " " + fl[i] + " " + fl2[i]);
                }
                if (in_Renamed[i] != in2[i])
                {
                    System.Console.Out.WriteLine("     Int error at " + i + " " + in_Renamed[i] + " " + in2[i]);
                }
                if (ln[i] != ln2[i])
                {
                    System.Console.Out.WriteLine("     Long error at " + i + " " + ln[i] + " " + ln2[i]);
                }
                if (sh[i] != sh2[i])
                {
                    System.Console.Out.WriteLine("     Short error at " + i + " " + sh[i] + " " + sh2[i]);
                }
                if (ch[i] != ch2[i])
                {
                    System.Console.Out.WriteLine("     Char error at " + i + " " + (int)ch[i] + " " + (int)ch2[i]);
                }
                if (by[i] != by2[i])
                {
                    System.Console.Out.WriteLine("     Byte error at " + i + " " + by[i] + " " + by2[i]);
                }
                if (bl[i] != bl2[i])
                {
                    System.Console.Out.WriteLine("     Bool error at " + i + " " + bl[i] + " " + bl2[i]);
                }
            }

            System.Console.Out.WriteLine("  Scalars:");

            // Check the scalars.
            if (bls != bls2)
            {
                System.Console.Out.WriteLine("     Bool Scalar mismatch:" + bls + " " + bls2);
            }
            if (bs != bs2)
            {
                System.Console.Out.WriteLine("     Byte Scalar mismatch:" + bs + " " + bs2);
            }
            if (cs != cs2)
            {
                System.Console.Out.WriteLine("     Char Scalar mismatch:" + (int)cs + " " + (int)cs2);
            }
            if (ss != ss2)
            {
                System.Console.Out.WriteLine("     Short Scalar mismatch:" + ss + " " + ss2);
            }
            if (is_Renamed != is2)
            {
                System.Console.Out.WriteLine("     Int Scalar mismatch:" + is_Renamed + " " + is2);
            }
            if (ls != ls2)
            {
                System.Console.Out.WriteLine("     Long Scalar mismatch:" + ls + " " + ls2);
            }
            if (fs != fs2)
            {
                System.Console.Out.WriteLine("     Float Scalar mismatch:" + fs + " " + fs2);
            }
            if (ds != ds2)
            {
                System.Console.Out.WriteLine("     Double Scalar mismatch:" + ds + " " + ds2);
            }

            System.Console.Out.WriteLine("  Multi: odd rows should match");
            for (var i = 0; i < 10; i += 1)
            {
                System.Console.Out.WriteLine("      " + i + " " + multi[i][i][i][i] + " " + multi2[i][i][i][i]);
            }
            System.Console.Out.WriteLine("Done BufferedStream Tests");
        }

        public static void bufferedFileTest(System.String filename, int numIts, double[] db, double[] db2, float[] fl, float[] fl2, long[] ln, long[] ln2, int[] in_Renamed, int[] in2, short[] sh, short[] sh2, char[] ch, char[] ch2, byte[] by, byte[] by2, bool[] bl, bool[] bl2, int[][][][] multi, int[][][][] multi2)
        {
            var dim = db.Length;

            var ds = SupportClass.Random.NextDouble() - 0.5;
            double ds2;
            var fs = (float)(SupportClass.Random.NextDouble() - 0.5);
            float fs2;
            var is_Renamed = (int)(1000000 * (SupportClass.Random.NextDouble() - 500000));
            int is2;
            var ls = (long)(100000000000L * (SupportClass.Random.NextDouble() - 50000000000L));
            long ls2;
            var ss = (short)(60000 * (SupportClass.Random.NextDouble() - 30000));
            short ss2;

            //			char cs = (char) (60000 * SupportClass.Random.NextDouble());
            var cs = SupportClass.NextChar();
            char cs2;
            var bs = (byte)(256 * SupportClass.Random.NextDouble() - 128);
            byte bs2;
            var bls = (SupportClass.Random.NextDouble() > 0.5);
            bool bls2;

            System.Console.Out.WriteLine("New libraries: nom.tam.util.BufferedFile");
            System.Console.Out.WriteLine("               Using array I/O methods.");

            var f = new BufferedFile(filename, FileAccess.ReadWrite);

            resetTime();
            for (var i = 0; i < numIts; i += 1)
            {
                f.WriteArray(db);
            }

            System.Console.Out.WriteLine("  BF  Dbl write: " + (8 * dim * numIts) / (1000 * deltaTime()));
            for (var i = 0; i < numIts; i += 1)
            {
                f.WriteArray(fl);
            }

            System.Console.Out.WriteLine("  BF  Flt write: " + (4 * dim * numIts) / (1000 * deltaTime()));
            for (var i = 0; i < numIts; i += 1)
            {
                f.WriteArray(in_Renamed);
            }

            System.Console.Out.WriteLine("  BF  Int write: " + (4 * dim * numIts) / (1000 * deltaTime()));
            for (var i = 0; i < numIts; i += 1)
            {
                f.WriteArray(ln);
            }

            System.Console.Out.WriteLine("  BF  Lng write: " + (8 * dim * numIts) / (1000 * deltaTime()));
            for (var i = 0; i < numIts; i += 1)
            {
                f.WriteArray(sh);
            }

            System.Console.Out.WriteLine("  BF  Sht write: " + (2 * dim * numIts) / (1000 * deltaTime()));
            for (var i = 0; i < numIts; i += 1)
            {
                f.WriteArray(ch);
            }

            System.Console.Out.WriteLine("  BF  Chr write: " + (2 * dim * numIts) / (1000 * deltaTime()));
            for (var i = 0; i < numIts; i += 1)
            {
                f.WriteArray(by);
            }

            System.Console.Out.WriteLine("  BF  Byt write: " + (1 * dim * numIts) / (1000 * deltaTime()));
            for (var i = 0; i < numIts; i += 1)
            {
                f.WriteArray(bl);
            }

            System.Console.Out.WriteLine("  BF  Boo write: " + (1 * dim * numIts) / (1000 * deltaTime()));

            f.Write((byte)bs);
            f.Write((System.Char)cs);
            f.Write((System.Int16)ss);
            f.Write(is_Renamed);
            f.Write(ls);
            f.Write(fs);
            f.Write(ds);
            f.Write(bls);

            f.WriteArray(multi);
            f.Flush();
            f.Seek(0, SeekOrigin.Begin);

            resetTime();
            for (var i = 0; i < numIts; i += 1)
            {
                f.ReadArray(db2);
            }

            System.Console.Out.WriteLine("  BF  Dbl read:  " + (8 * dim * numIts) / (1000 * deltaTime()));
            for (var i = 0; i < numIts; i += 1)
            {
                f.ReadArray(fl2);
            }

            System.Console.Out.WriteLine("  BF  Flt read:  " + (4 * dim * numIts) / (1000 * deltaTime()));
            for (var i = 0; i < numIts; i += 1)
            {
                f.ReadArray(in2);
            }

            System.Console.Out.WriteLine("  BF  Int read:  " + (4 * dim * numIts) / (1000 * deltaTime()));
            for (var i = 0; i < numIts; i += 1)
            {
                f.ReadArray(ln2);
            }

            System.Console.Out.WriteLine("  BF  Lng read:  " + (8 * dim * numIts) / (1000 * deltaTime()));
            for (var i = 0; i < numIts; i += 1)
            {
                f.ReadArray(sh2);
            }

            System.Console.Out.WriteLine("  BF  Sht read:  " + (2 * dim * numIts) / (1000 * deltaTime()));
            for (var i = 0; i < numIts; i += 1)
            {
                f.ReadArray(ch2);
            }

            System.Console.Out.WriteLine("  BF  Chr read:  " + (2 * dim * numIts) / (1000 * deltaTime()));
            for (var i = 0; i < numIts; i += 1)
            {
                f.ReadArray(by2);
            }

            System.Console.Out.WriteLine("  BF  Byt read:  " + (1 * dim * numIts) / (1000 * deltaTime()));
            for (var i = 0; i < numIts; i += 1)
            {
                f.ReadArray(bl2);
            }

            System.Console.Out.WriteLine("  BF  Boo read:  " + (1 * dim * numIts) / (1000 * deltaTime()));

            bs2 = (byte)f.ReadByte();
            cs2 = f.ReadChar();
            ss2 = f.ReadInt16();
            is2 = f.ReadInt32();
            ls2 = f.ReadInt64();
            fs2 = f.ReadSingle();
            ds2 = f.ReadDouble();
            bls2 = f.ReadBoolean();

            // Now read only pieces of the multidimensional array.
            for (var i = 0; i < 5; i += 1)
            {
                // Skip the odd initial indices and read the evens.
                System.Int64 temp_Int64;
                temp_Int64 = f.Position;
                temp_Int64 = f.Seek(4000) - temp_Int64;
                var generatedAux27 = (int)temp_Int64;
                f.ReadArray(multi2[2 * i + 1]);
            }

            f.Close();

            System.Console.Out.WriteLine("BufferedFile Verification:");
            System.Console.Out.WriteLine("  An error should be reported for double and float NaN's");
            System.Console.Out.WriteLine("  Arrays:");

            for (var i = 0; i < dim; i += 1)
            {
                if (db[i] != db2[i] && !Double.IsNaN(db[i]) && !Double.IsNaN(db2[i]))
                {
                    System.Console.Out.WriteLine("     Double error at " + i + " " + db[i] + " " + db2[i]);
                }
                if (fl[i] != fl2[i] && !Single.IsNaN(fl[i]) && !Single.IsNaN(fl2[i]))
                {
                    System.Console.Out.WriteLine("     Float error at " + i + " " + fl[i] + " " + fl2[i]);
                }
                if (in_Renamed[i] != in2[i])
                {
                    System.Console.Out.WriteLine("     Int error at " + i + " " + in_Renamed[i] + " " + in2[i]);
                }
                if (ln[i] != ln2[i])
                {
                    System.Console.Out.WriteLine("     Long error at " + i + " " + ln[i] + " " + ln2[i]);
                }
                if (sh[i] != sh2[i])
                {
                    System.Console.Out.WriteLine("     Short error at " + i + " " + sh[i] + " " + sh2[i]);
                }
                if (ch[i] != ch2[i])
                {
                    System.Console.Out.WriteLine("     Char error at " + i + " " + (int)ch[i] + " " + (int)ch2[i]);
                }
                if (by[i] != by2[i])
                {
                    System.Console.Out.WriteLine("     Byte error at " + i + " " + by[i] + " " + by2[i]);
                }
                if (bl[i] != bl2[i])
                {
                    System.Console.Out.WriteLine("     Bool error at " + i + " " + bl[i] + " " + bl2[i]);
                }
            }

            System.Console.Out.WriteLine("  Scalars:");

            // Check the scalars.
            if (bls != bls2)
            {
                System.Console.Out.WriteLine("     Bool Scalar mismatch:" + bls + " " + bls2);
            }
            if (bs != bs2)
            {
                System.Console.Out.WriteLine("     Byte Scalar mismatch:" + bs + " " + bs2);
            }
            if (cs != cs2)
            {
                System.Console.Out.WriteLine("     Char Scalar mismatch:" + (int)cs + " " + (int)cs2);
            }
            if (ss != ss2)
            {
                System.Console.Out.WriteLine("     Short Scalar mismatch:" + ss + " " + ss2);
            }
            if (is_Renamed != is2)
            {
                System.Console.Out.WriteLine("     Int Scalar mismatch:" + is_Renamed + " " + is2);
            }
            if (ls != ls2)
            {
                System.Console.Out.WriteLine("     Long Scalar mismatch:" + ls + " " + ls2);
            }
            if (fs != fs2)
            {
                System.Console.Out.WriteLine("     Float Scalar mismatch:" + fs + " " + fs2);
            }
            if (ds != ds2)
            {
                System.Console.Out.WriteLine("     Double Scalar mismatch:" + ds + " " + ds2);
            }

            System.Console.Out.WriteLine("  Multi: odd rows should match");
            for (var i = 0; i < 10; i += 1)
            {
                System.Console.Out.WriteLine("      " + i + " " + multi[i][i][i][i] + " " + multi2[i][i][i][i]);
            }
            System.Console.Out.WriteLine("Done BufferedFile Tests");
        }

        internal static long lastTime;

        internal static void resetTime()
        {
            lastTime = ((System.DateTime.Now.Ticks - 621355968000000000) / 10000) - (long)System.TimeZone.CurrentTimeZone.GetUtcOffset(System.DateTime.Now).TotalMilliseconds;
        }

        internal static double deltaTime()
        {
            var time = lastTime;
            lastTime = ((System.DateTime.Now.Ticks - 621355968000000000) / 10000) - (long)System.TimeZone.CurrentTimeZone.GetUtcOffset(System.DateTime.Now).TotalMilliseconds;
            return (lastTime - time) / 1000.0;
        }
    }
}