// <copyright file="BufferedDataStream.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

namespace nom.tam.util
{
    /* Copyright: Thomas McGlynn 1997-1999.
    * This code may be used for any purpose, non-commercial
    * or commercial so long as this copyright notice is retained
    * in the source code or included in or referred to in any
    * derived software.
    */

    /// <summary>
    /// Summary description for BufferedDataStream.
    /// </summary>
    //public class BufferedDataStream : ArrayDataIO
    public class BufferedDataStream : RandomAccess
    {
        #region Constructors

        public BufferedDataStream(Stream s) : this(s, -1)
        {
        }

        public BufferedDataStream(Stream s, int bufLength)
        {
            //      Console.Error.WriteLine(s.CanWrite);
            _s = bufLength < 0 ? new BufferedStream(s) : new BufferedStream(s, bufLength);
            _in = new BinaryReader(_s);
            if (_s.CanWrite)
            {
                _out = new BinaryWriter(_s);
                _outBuf = new byte[0];
            }
            _garbageBuf = new byte[1024];
        }

        #endregion Constructors

        #region ArrayDataIO Members

        #region Properties

        public override bool CanRead
        {
            get
            {
                return _s.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                /*
                if(_s is ConfigStream)
                {
                  return ((ConfigStream)_s).CanSeek;
                }
                */
                return _s.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return _s.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return _s.Length;
            }
        }

        public override long Position
        {
            get
            {
                return _s.Position;
            }

            set
            {
                _s.Position = value;
            }
        }

        protected override Stream BaseStream
        {
            get
            {
                return _s;
            }
        }

        #endregion Properties

        #region Class Variables

        protected static int sbyteByteStride = 1;
        protected static int boolByteStride = BitConverter.GetBytes(true).Length;
        protected static int charByteStride = BitConverter.GetBytes('a').Length;
        protected static int shortByteStride = BitConverter.GetBytes((short)0).Length;
        protected static int intByteStride = BitConverter.GetBytes(0).Length;
        protected static int longByteStride = BitConverter.GetBytes((long)0).Length;
        protected static int floatByteStride = BitConverter.GetBytes(0.0f).Length;
        protected static int doubleByteStride = BitConverter.GetBytes(0.0).Length;

        #endregion Class Variables

        #region Read Methods

        public override bool ReadBoolean()
        {
            return _in.ReadBoolean();
        }

        //public override byte ReadByte()
        public override int ReadByte()
        {
            return _in.ReadByte();
        }

        public override sbyte ReadSByte()
        {
            return _in.ReadSByte();
        }

        public override char ReadChar()
        {
            var buf = new char[1];
            Read(buf);
            return buf[0];
        }

        public override short ReadInt16()
        {
            var buf = new short[1];
            Read(buf);
            return buf[0];
        }

        public override int ReadInt32()
        {
            var buf = new int[1];
            Read(buf);
            return buf[0];
        }

        public override long ReadInt64()
        {
            var buf = new long[1];
            Read(buf);
            return buf[0];
        }

        public override float ReadSingle()
        {
            var buf = new float[1];
            Read(buf);
            return buf[0];
        }

        public override double ReadDouble()
        {
            var buf = new double[1];
            Read(buf);
            return buf[0];
        }

        /// <summary>Read an object.  An EOF will be signaled if the
        /// object cannot be fully read.  The getPrimitiveArrayCount()
        /// method may then be used to get a minimum number of bytes read.
        /// </summary>
        /// <param name="o"> The object to be read.  This object should
        /// be a primitive (possibly multi-dimensional) array.
        ///
        /// </param>
        /// <returns>s  The number of bytes read.
        ///
        /// </returns>
        public override int ReadArray(object o)
        {
            primitiveArrayCount = 0;
            return primitiveArrayRecurse(o);
        }

        /// <summary>Read recursively over a multi-dimensional array.</summary>
        /// <returns> The number of bytes read.</returns>
        protected int primitiveArrayRecurse(object o)
        {
            if (o == null)
            {
                return primitiveArrayCount;
            }

            if (!o.GetType().IsArray)
            {
                throw new IOException("Invalid object passed to BufferedDataStream.ReadArray:" + o.GetType().FullName);
            }

            // Is this a multidimensional array?  If so process recursively.
            //if(o.GetType().GetArrayRank() > 1 || ((Array)o).GetValue(0).GetType().IsArray)
            if (ArrayFuncs.GetDimensions(o).Length > 1)
            {
                if (ArrayFuncs.IsArrayOfArrays(o))
                {
                    var i = ((Array)o).GetEnumerator();
                    for (var ok = i.MoveNext(); ok; ok = i.MoveNext())
                    {
                        primitiveArrayRecurse(i.Current);
                    }
                }
                else
                {
                    throw new IOException("Rectangular array read not supported.");
                }
            }
            else
            {
                // This is a one-d array.  Process it using our special functions.
                var t = o.GetType().GetElementType();
                if (typeof(bool).Equals(t))
                {
                    primitiveArrayCount += Read((bool[])o, 0, ((bool[])o).Length);
                }
                else if (typeof(byte).Equals(t))
                {
                    primitiveArrayCount += Read((byte[])o, 0, ((byte[])o).Length);
                }
                else if (typeof(sbyte).Equals(t))
                {
                    primitiveArrayCount += Read((sbyte[])o, 0, ((sbyte[])o).Length);
                }
                else if (typeof(char).Equals(t))
                {
                    primitiveArrayCount += Read((char[])o, 0, ((char[])o).Length);
                }
                else if (typeof(short).Equals(t))
                {
                    primitiveArrayCount += Read((short[])o, 0, ((short[])o).Length);
                }
                else if (typeof(int).Equals(t))
                {
                    primitiveArrayCount += Read((int[])o, 0, ((int[])o).Length);
                }
                else if (typeof(long).Equals(t))
                {
                    primitiveArrayCount += Read((long[])o, 0, ((long[])o).Length);
                }
                else if (typeof(float).Equals(t))
                {
                    primitiveArrayCount += Read((float[])o, 0, ((float[])o).Length);
                }
                else if (typeof(double).Equals(t))
                {
                    primitiveArrayCount += Read((double[])o, 0, ((double[])o).Length);
                }
                else
                {
                    throw new IOException("Invalid object passed to BufferedDataStream.ReadArray: " + o.GetType().FullName);
                }
            }

            return primitiveArrayCount;
        }

        public override int Read(byte[] buf)
        {
            return Read(buf, 0, buf.Length);
        }

        public override int Read(sbyte[] buf)
        {
            return Read(buf, 0, buf.Length);
        }

        public override int Read(bool[] buf)
        {
            return Read(buf, 0, buf.Length);
        }

        public override int Read(short[] buf)
        {
            return Read(buf, 0, buf.Length);
        }

        public override int Read(char[] buf)
        {
            return Read(buf, 0, buf.Length);
        }

        public override int Read(int[] buf)
        {
            return Read(buf, 0, buf.Length);
        }

        public override int Read(long[] buf)
        {
            return Read(buf, 0, buf.Length);
        }

        public override int Read(float[] buf)
        {
            return Read(buf, 0, buf.Length);
        }

        public override int Read(double[] buf)
        {
            return Read(buf, 0, buf.Length);
        }

        public override int Read(byte[] buf, int offset, int size)
        {
            var result = 0;

            try
            {
                var tbuf = _in.ReadBytes(size);
                Buffer.BlockCopy(tbuf, 0, buf, offset, tbuf.Length);
                result = tbuf.Length;
            }
            catch (Exception)
            {
                result = 0;
            }

            return result;
        }

        public override int Read(sbyte[] buf, int offset, int size)
        {
            var result = 0;

            try
            {
                var tbuf = _in.ReadBytes(size);
                for (var i = 0; i < tbuf.Length; ++i)
                {
                    buf[i + offset] = (sbyte)tbuf[i];
                }
                result = tbuf.Length;
            }
            catch (Exception)
            {
                result = 0;
            }

            return 0;
        }

        public override int Read(bool[] buf, int offset, int size)
        {
            var nRead = 0;

            try
            {
                var tbuf = _in.ReadBytes(boolByteStride * size);
                for (var b = 0; b < tbuf.Length; ++nRead, b += boolByteStride)
                {
                    buf[nRead + offset] = BitConverter.ToBoolean(tbuf, b);
                }
            }
            catch (Exception)
            {
                nRead = 0;
            }

            return nRead;
        }

        public override int Read(char[] buf, int offset, int size)
        {
            var nRead = 0;

            try
            {
                byte bPrime = 0;
                var tbuf = _in.ReadBytes(charByteStride * size);
                for (var b = 0; b < tbuf.Length; ++nRead, b += charByteStride)
                {
                    bPrime = tbuf[b];
                    tbuf[b] = tbuf[b + 1];
                    tbuf[b + 1] = bPrime;
                    buf[nRead + offset] = BitConverter.ToChar(tbuf, b);
                }
            }
            catch (Exception)
            {
                nRead = 0;
            }

            return nRead;
        }

        public override int Read(short[] buf, int offset, int size)
        {
            var nRead = 0;

            try
            {
                byte bPrime = 0;
                var tbuf = _in.ReadBytes(shortByteStride * size);
                for (var b = 0; b < tbuf.Length; ++nRead, b += shortByteStride)
                {
                    bPrime = tbuf[b];
                    tbuf[b] = tbuf[b + 1];
                    tbuf[b + 1] = bPrime;
                    buf[nRead + offset] = BitConverter.ToInt16(tbuf, b);
                }
            }
            catch (Exception)
            {
                nRead = 0;
            }

            return nRead;
        }

        public override int Read(int[] buf, int offset, int size)
        {
            var nRead = 0;

            try
            {
                byte bPrime = 0;
                var tbuf = _in.ReadBytes(intByteStride * size);
                for (var b = 0; b < tbuf.Length; ++nRead, b += intByteStride)
                {
                    bPrime = tbuf[b];
                    tbuf[b] = tbuf[b + 3];
                    tbuf[b + 3] = bPrime;
                    bPrime = tbuf[b + 1];
                    tbuf[b + 1] = tbuf[b + 2];
                    tbuf[b + 2] = bPrime;
                    buf[nRead + offset] = BitConverter.ToInt32(tbuf, b);
                }
            }
            catch (Exception)
            {
                nRead = 0;
            }

            return nRead;
        }

        public override int Read(long[] buf, int offset, int size)
        {
            var nRead = 0;

            try
            {
                byte bPrime = 0;
                var tbuf = _in.ReadBytes(longByteStride * size);
                for (var b = 0; b < tbuf.Length; ++nRead, b += longByteStride)
                {
                    bPrime = tbuf[b];
                    tbuf[b] = tbuf[b + 7];
                    tbuf[b + 7] = bPrime;
                    bPrime = tbuf[b + 1];
                    tbuf[b + 1] = tbuf[b + 6];
                    tbuf[b + 6] = bPrime;
                    bPrime = tbuf[b + 2];
                    tbuf[b + 2] = tbuf[b + 5];
                    tbuf[b + 5] = bPrime;
                    bPrime = tbuf[b + 3];
                    tbuf[b + 3] = tbuf[b + 4];
                    tbuf[b + 4] = bPrime;
                    buf[nRead + offset] = BitConverter.ToInt64(tbuf, b);
                }
            }
            catch (Exception)
            {
                nRead = 0;
            }

            return nRead;
        }

        public override int Read(float[] buf, int offset, int size)
        {
            var nRead = 0;

            try
            {
                byte bPrime = 0;
                var tbuf = _in.ReadBytes(floatByteStride * size);
                for (var b = 0; b < tbuf.Length; ++nRead, b += floatByteStride)
                {
                    bPrime = tbuf[b];
                    tbuf[b] = tbuf[b + 3];
                    tbuf[b + 3] = bPrime;
                    bPrime = tbuf[b + 1];
                    tbuf[b + 1] = tbuf[b + 2];
                    tbuf[b + 2] = bPrime;
                    buf[nRead + offset] = BitConverter.ToSingle(tbuf, b);
                }
            }
            catch (Exception)
            {
                //        Console.Out.WriteLine(e);
                nRead = 0;
            }

            return nRead;
        }

        public override int Read(double[] buf, int offset, int size)
        {
            var nRead = 0;

            try
            {
                byte bPrime = 0;
                var tbuf = _in.ReadBytes(doubleByteStride * size);
                for (var b = 0; b < tbuf.Length; ++nRead, b += doubleByteStride)
                {
                    bPrime = tbuf[b];
                    tbuf[b] = tbuf[b + 7];
                    tbuf[b + 7] = bPrime;
                    bPrime = tbuf[b + 1];
                    tbuf[b + 1] = tbuf[b + 6];
                    tbuf[b + 6] = bPrime;
                    bPrime = tbuf[b + 2];
                    tbuf[b + 2] = tbuf[b + 5];
                    tbuf[b + 5] = bPrime;
                    bPrime = tbuf[b + 3];
                    tbuf[b + 3] = tbuf[b + 4];
                    tbuf[b + 4] = bPrime;
                    buf[nRead + offset] = BitConverter.ToDouble(tbuf, b);
                }
            }
            catch (Exception)
            {
                nRead = 0;
            }

            return nRead;
        }

        #endregion Read Methods

        #region Write Methods

        public override void Write(byte x)
        {
            _out.Write(x);
        }

        public override void Write(sbyte x)
        {
            _out.Write(x);
        }

        public override void Write(bool x)
        {
            _out.Write(x);
        }

        public override void Write(char x)
        {
            var tbuf = BitConverter.GetBytes(x);
            var bPrime = tbuf[0];
            tbuf[0] = tbuf[1];
            tbuf[1] = bPrime;

            _out.Write(tbuf);
        }

        public override void Write(short x)
        {
            var tbuf = BitConverter.GetBytes(x);
            var bPrime = tbuf[0];
            tbuf[0] = tbuf[1];
            tbuf[1] = bPrime;

            _out.Write(tbuf);
        }

        public override void Write(int x)
        {
            var tbuf = BitConverter.GetBytes(x);
            var bPrime = tbuf[0];
            tbuf[0] = tbuf[3];
            tbuf[3] = bPrime;
            bPrime = tbuf[1];
            tbuf[1] = tbuf[2];
            tbuf[2] = bPrime;

            _out.Write(tbuf);
        }

        public override void Write(long x)
        {
            var tbuf = BitConverter.GetBytes(x);
            var bPrime = tbuf[0];
            tbuf[0] = tbuf[7];
            tbuf[7] = bPrime;
            bPrime = tbuf[1];
            tbuf[1] = tbuf[6];
            tbuf[6] = bPrime;
            bPrime = tbuf[2];
            tbuf[2] = tbuf[5];
            tbuf[5] = bPrime;
            bPrime = tbuf[3];
            tbuf[3] = tbuf[4];
            tbuf[4] = bPrime;

            _out.Write(tbuf);
        }

        public override void Write(float x)
        {
            var tbuf = BitConverter.GetBytes(x);
            var bPrime = tbuf[0];
            tbuf[0] = tbuf[3];
            tbuf[3] = bPrime;
            bPrime = tbuf[1];
            tbuf[1] = tbuf[2];
            tbuf[2] = bPrime;

            _out.Write(tbuf);
        }

        public override void Write(double x)
        {
            var tbuf = BitConverter.GetBytes(x);
            var bPrime = tbuf[0];
            tbuf[0] = tbuf[7];
            tbuf[7] = bPrime;
            bPrime = tbuf[1];
            tbuf[1] = tbuf[6];
            tbuf[6] = bPrime;
            bPrime = tbuf[2];
            tbuf[2] = tbuf[5];
            tbuf[5] = bPrime;
            bPrime = tbuf[3];
            tbuf[3] = tbuf[4];
            tbuf[4] = bPrime;

            _out.Write(tbuf);
        }

        /// <summary>This routine provides efficient writing of arrays of any primitive type.
        /// The String class is also handled but it is an error to invoke this
        /// method with an object that is not an array of these types.  If the
        /// array is multidimensional, then it calls itself recursively to write
        /// the entire array.  Strings are written using the standard
        /// 1 byte format (i.e., as in writeBytes).
        /// *
        /// If the array is an array of objects, then writePrimitiveArray will
        /// be called for each element of the array.
        /// *
        /// </summary>
        /// <param name="o"> The object to be written.  It must be an array of a primitive
        /// type, Object, or String.</param>
        public override void WriteArray(object o)
        {
            if (!o.GetType().IsArray)
            {
                throw new IOException("Invalid object passed to BufferedDataStream.Write" + o.GetType().FullName);
            }
            var type = o.GetType();
            var rank = ((Array)o).Rank;

            // Is this a multidimensional array?  If so process recursively.
            if (ArrayFuncs.GetDimensions(o).Length > 1)

            //if(o.GetType().GetArrayRank() > 1 || (((Array)o).GetValue(0)).GetType().IsArray)
            {
                if (ArrayFuncs.IsArrayOfArrays(o))
                {
                    var i = ((Array)o).GetEnumerator();
                    for (var ok = i.MoveNext(); ok; ok = i.MoveNext())
                    {
                        WriteArray(i.Current);
                    }
                }
                else
                {
                    throw new IOException("Rectangular array write not supported.");
                }
            }
            else
            {
                // This is a one-d array.  Process it using our special functions.
                var t = o.GetType().GetElementType();

                if (typeof(bool).Equals(t))
                {
                    Write((bool[])o, 0, ((bool[])o).Length);
                }
                else if (typeof(byte).Equals(t))
                {
                    Write((byte[])o, 0, ((byte[])o).Length);
                }
                else if (typeof(sbyte).Equals(t))
                {
                    Write((sbyte[])o, 0, ((sbyte[])o).Length);
                }
                else if (typeof(char).Equals(t))
                {
                    Write((char[])o, 0, ((char[])o).Length);
                }
                else if (typeof(short).Equals(t))
                {
                    Write((short[])o, 0, ((short[])o).Length);
                }
                else if (typeof(int).Equals(t))
                {
                    Write((int[])o, 0, ((int[])o).Length);
                }
                else if (typeof(long).Equals(t))
                {
                    Write((long[])o, 0, ((long[])o).Length);
                }
                else if (typeof(float).Equals(t))
                {
                    Write((float[])o, 0, ((float[])o).Length);
                }
                else if (typeof(double).Equals(t))
                {
                    Write((double[])o, 0, ((double[])o).Length);
                }
                else if (typeof(string).Equals(t))
                {
                    Write((string[])o, 0, ((string[])o).Length);
                }
                else
                {
                    throw new IOException("Invalid object passed to BufferedDataStream.WriteArray: " + o.GetType().FullName);
                }
            }
        }

        public override void Write(byte[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        public override void Write(sbyte[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        public override void Write(bool[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        public override void Write(short[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        public override void Write(char[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        public override void Write(int[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        public override void Write(long[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        public override void Write(float[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        public override void Write(double[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        public override void Write(string[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        public override void Write(byte[] buf, int offset, int size)
        {
            _out.Write(buf, offset, size);
        }

        public override void Write(sbyte[] buf, int offset, int size)
        {
            if (_outBuf.Length < size)
            {
                _outBuf = new byte[size];
            }
            for (var i = 0; i < size; ++i)
            {
                _outBuf[i] = (byte)buf[offset + i];
            }
            _out.Write(_outBuf, 0, size);
        }

        public override void Write(bool[] buf, int offset, int size)
        {
            if (_outBuf.Length < boolByteStride * size)
            {
                _outBuf = new byte[boolByteStride * size];
            }
            for (int nWritten = 0, b = 0; nWritten < size; ++nWritten, b += boolByteStride)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(buf[nWritten + offset]), 0, _outBuf, b, boolByteStride);
            }
            _out.Write(_outBuf, 0, boolByteStride * size);
        }

        public override void Write(char[] buf, int offset, int size)
        {
            if (_outBuf.Length < charByteStride * size)
            {
                _outBuf = new byte[charByteStride * size];
            }
            byte bPrime = 0;
            for (int nWritten = 0, b = 0; nWritten < size; ++nWritten, b += charByteStride)
            {
                var tbuf = BitConverter.GetBytes(buf[nWritten + offset]);
                bPrime = tbuf[0];
                tbuf[0] = tbuf[1];
                tbuf[1] = bPrime;
                Buffer.BlockCopy(tbuf, 0, _outBuf, b, charByteStride);
            }
            _out.Write(_outBuf, 0, charByteStride * size);
        }

        public override void Write(short[] buf, int offset, int size)
        {
            if (_outBuf.Length < shortByteStride * size)
            {
                _outBuf = new byte[shortByteStride * size];
            }
            byte bPrime = 0;
            for (int nWritten = 0, b = 0; nWritten < size; ++nWritten, b += shortByteStride)
            {
                var tbuf = BitConverter.GetBytes(buf[nWritten + offset]);
                bPrime = tbuf[0];
                tbuf[0] = tbuf[1];
                tbuf[1] = bPrime;
                Buffer.BlockCopy(tbuf, 0, _outBuf, b, shortByteStride);
            }
            _out.Write(_outBuf, 0, shortByteStride * size);
        }

        public override void Write(int[] buf, int offset, int size)
        {
            if (_outBuf.Length < intByteStride * size)
            {
                _outBuf = new byte[intByteStride * size];
            }
            byte bPrime = 0;
            for (int nWritten = 0, b = 0; nWritten < size; ++nWritten, b += intByteStride)
            {
                var tbuf = BitConverter.GetBytes(buf[nWritten + offset]);
                bPrime = tbuf[0];
                tbuf[0] = tbuf[3];
                tbuf[3] = bPrime;
                bPrime = tbuf[1];
                tbuf[1] = tbuf[2];
                tbuf[2] = bPrime;
                Buffer.BlockCopy(tbuf, 0, _outBuf, b, intByteStride);
            }
            _out.Write(_outBuf, 0, intByteStride * size);
        }

        public override void Write(long[] buf, int offset, int size)
        {
            if (_outBuf.Length < longByteStride * size)
            {
                _outBuf = new byte[longByteStride * size];
            }
            byte bPrime = 0;
            for (int nWritten = 0, b = 0; nWritten < size; ++nWritten, b += longByteStride)
            {
                var tbuf = BitConverter.GetBytes(buf[nWritten + offset]);
                bPrime = tbuf[0];
                tbuf[0] = tbuf[7];
                tbuf[7] = bPrime;
                bPrime = tbuf[1];
                tbuf[1] = tbuf[6];
                tbuf[6] = bPrime;
                bPrime = tbuf[2];
                tbuf[2] = tbuf[5];
                tbuf[5] = bPrime;
                bPrime = tbuf[3];
                tbuf[3] = tbuf[4];
                tbuf[4] = bPrime;
                Buffer.BlockCopy(tbuf, 0, _outBuf, b, longByteStride);
            }
            _out.Write(_outBuf, 0, longByteStride * size);
        }

        public override void Write(float[] buf, int offset, int size)
        {
            if (_outBuf.Length < floatByteStride * size)
            {
                _outBuf = new byte[floatByteStride * size];
            }
            byte bPrime = 0;
            for (int nWritten = 0, b = 0; nWritten < size; ++nWritten, b += floatByteStride)
            {
                var tbuf = BitConverter.GetBytes(buf[nWritten + offset]);
                bPrime = tbuf[0];
                tbuf[0] = tbuf[3];
                tbuf[3] = bPrime;
                bPrime = tbuf[1];
                tbuf[1] = tbuf[2];
                tbuf[2] = bPrime;
                Buffer.BlockCopy(tbuf, 0, _outBuf, b, floatByteStride);
            }
            _out.Write(_outBuf, 0, floatByteStride * size);
        }

        public override void Write(double[] buf, int offset, int size)
        {
            if (_outBuf.Length < doubleByteStride * size)
            {
                _outBuf = new byte[doubleByteStride * size];
            }
            byte bPrime = 0;
            for (int nWritten = 0, b = 0; nWritten < size; ++nWritten, b += doubleByteStride)
            {
                var tbuf = BitConverter.GetBytes(buf[nWritten + offset]);
                bPrime = tbuf[0];
                tbuf[0] = tbuf[7];
                tbuf[7] = bPrime;
                bPrime = tbuf[1];
                tbuf[1] = tbuf[6];
                tbuf[6] = bPrime;
                bPrime = tbuf[2];
                tbuf[2] = tbuf[5];
                tbuf[5] = bPrime;
                bPrime = tbuf[3];
                tbuf[3] = tbuf[4];
                tbuf[4] = bPrime;
                Buffer.BlockCopy(tbuf, 0, _outBuf, b, doubleByteStride);
            }
            _out.Write(_outBuf, 0, doubleByteStride * size);
        }

        public override void Write(string[] buf, int offset, int size)
        {
            for (var i = offset; i < offset + size; ++i)
            {
                _out.Write(SupportClass.ToByteArray(buf[i]));
            }
        }

        #endregion Write Methods

        #region Other Methods

        /*
        public override long FilePointer
        {
          get
          {
            return 0;
          }
        }
    */

        public override void Flush()
        {
            _out.Flush();
        }

        public override long Seek(long distance)
        {
            return Seek(distance, SeekOrigin.Current);
        }

        public override long Seek(long distance, SeekOrigin origin)
        {
            if (_s.CanSeek)
            {
                var oldPos = _s.Position;
                var newPos = _s.Seek(distance, origin);
                return newPos - oldPos;
            }
            else
            {
                long dist = 0;
                for (var len = _s.Read(_garbageBuf, 0, (int)Math.Min(distance, _garbageBuf.Length));
                    distance > 0 && len != -1;)
                {
                    dist += len;
                    distance -= len;
                    len = _s.Read(_garbageBuf, 0, (int)Math.Min(distance, _garbageBuf.Length));
                }
                return dist;
            }
        }

        public override void SetLength(long val)
        {
            _s.SetLength(val);
        }

        public override void Close()
        {
            _in.Close();
            _out.Close();
            _s.Close();
            _outBuf = new byte[0];
        }

        #endregion Other Methods

        #endregion ArrayDataIO Members

        #region Instance Variables

        protected Stream _s;
        protected BinaryReader _in;
        protected BinaryWriter _out;
        protected byte[] _outBuf;
        protected byte[] _garbageBuf;
        protected int primitiveArrayCount;

        #endregion Instance Variables
    }
}