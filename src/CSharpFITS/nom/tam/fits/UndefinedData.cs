// <copyright file="UndefinedData.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

namespace nom.tam.fits
{
    using System;
    using System.IO;
    using nom.tam.util;

    /* Copyright: Thomas McGlynn 1997-1999.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	*
	* Many thanks to David Glowacki (U. Wisconsin) for substantial
	* improvements, enhancements and bug fixes.
	*/

    /// <summary>This class provides a simple holder for data which is
    /// not handled by other classes.
    /// </summary>
    public class UndefinedData : Data
    {
        #region Properties

        /// <summary>Get the size in bytes of the data</summary>
        override internal int TrueSize
        {
            get
            {
                return (int)byteSize;
            }
        }

        /// <summary>Return the actual data.
        /// Note that this may return a null when
        /// the data is not readable.  It might be better
        /// to throw a FitsException, but this is
        /// a very commonly called method and we prefered
        /// not to change how users must invoke it.
        /// </summary>
        override public object DataArray
        {
            get
            {
                if (data == null)
                {
                    try
                    {
                        //FitsUtil.Reposition(input, fileOffset);
                        input.Seek(fileOffset, SeekOrigin.Begin);
                        input.Read(data);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }

                return data;
            }
        }

        #endregion Properties

        /// <summary>The size of the data
        /// </summary>
        internal long byteSize;

        internal byte[] data;

        #region Constructors

        public UndefinedData(Header h)
        {
            /// <summary>Just get a byte buffer to hold the data.
            /// </summary>
            var size = 1;
            for (var i = 0; i < h.GetIntValue("NAXIS"); i += 1)
            {
                size *= h.GetIntValue("NAXIS" + (i + 1));
            }
            size += h.GetIntValue("PCOUNT");
            if (h.GetIntValue("GCOUNT") > 1)
            {
                size *= h.GetIntValue("GCOUNT");
            }
            size *= System.Math.Abs(h.GetIntValue("BITPIX") / 8);

            data = new byte[size];
            byteSize = size;
        }

        /// <summary>Create an UndefinedData object using the specified object.</summary>
        public UndefinedData(object x)
        {
            byteSize = ArrayFuncs.ComputeSize(x);
            data = new byte[(int)byteSize];
        }

        #endregion Constructors

        /// <summary>Fill header with keywords that describe data.
        /// </summary>
        /// <param name="head">The FITS header
        ///
        /// </param>
        internal override void FillHeader(Header head)
        {
            try
            {
                head.Xtension = "UNKNOWN";
                head.Bitpix = 8;
                head.Naxes = 1;
                head.AddValue("NAXIS1", byteSize, " Number of Bytes ");
                head.AddValue("PCOUNT", 0, null);
                head.AddValue("GCOUNT", 1, null);
                head.AddValue("EXTEND", true, "Extensions are permitted"); // Just in case!
            }
            catch (HeaderCardException e)
            {
                System.Console.Error.WriteLine("Unable to create unknown header:" + e);
            }
        }

        public override void Read(ArrayDataIO i)
        {
            SetFileOffset(i);

            if (i is RandomAccess)
            {
                try
                {
                    //BinaryReader temp_BinaryReader;
                    System.Int64 temp_Int64;

                    //temp_BinaryReader = i;
                    temp_Int64 = i.Position; //temp_BinaryReader.BaseStream.Position;
                    temp_Int64 = i.Seek((int)byteSize) - temp_Int64;  //temp_BinaryReader.BaseStream.Seek((int) byteSize, SeekOrigin.Current) - temp_Int64;
                    var generatedAux = (int)temp_Int64;
                }
                catch (IOException e)
                {
                    throw new FitsException("Unable to skip over data:" + e);
                }
            }
            else
            {
                try
                {
                    i.Read(data);
                }
                catch (IOException e)
                {
                    throw new FitsException("Unable to read unknown data:" + e);
                }
            }

            var pad = FitsUtil.Padding(TrueSize);
            try
            {
                //BinaryReader temp_BinaryReader2;
                System.Int64 temp_Int65;

                //temp_BinaryReader2 = i;
                temp_Int65 = i.Position;  //temp_BinaryReader2.BaseStream.Position;
                temp_Int65 = i.Seek(pad) - temp_Int65;  //temp_BinaryReader2.BaseStream.Seek(pad, SeekOrigin.Current) - temp_Int65;
                if (temp_Int65 != pad)
                {
                    throw new FitsException("Error skipping padding");
                }
            }
            catch (IOException e)
            {
                throw new FitsException("Error reading unknown padding:" + e);
            }
        }

        public override void Write(ArrayDataIO o)
        {
            if (data == null)
            {
                var generatedAux = DataArray;
            }

            if (data == null)
            {
                throw new FitsException("Null unknown data");
            }

            try
            {
                o.Write(data);
            }
            catch (IOException e)
            {
                throw new FitsException("IO Error on unknown data write" + e);
            }

            var padding = new byte[FitsUtil.Padding(TrueSize)];
            try
            {
                o.Write(padding);
            }
            catch (IOException e)
            {
                throw new FitsException("Error writing padding: " + e);
            }
        }
    }
}