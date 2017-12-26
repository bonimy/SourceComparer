// <copyright file="ByteRenderer.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

using System;
using System.Collections;
using System.IO;

namespace nom.tam.util
{
    /// <summary>
    /// Summary description for ByteRenderers.
    /// </summary>
    public abstract class ByteRenderer
    {
        public static ByteRenderer BYTE_ARRAY_RENDERER = new ByteArrayByteRenderer();
        public static ByteRenderer CHAR_ARRAY_RENDERER = new CharArrayByteRenderer();
        public static ByteRenderer STRING_ARRAY_RENDERER_TRUNCATE = new StringArrayByteRendererTruncate();
        public static ByteRenderer STRING_ARRAY_RENDERER_HEAP = new StringArrayByteRendererHeap();
        public static ByteRenderer TROOLEAN_ARRAY_RENDERER = new TrooleanArrayByteRenderer();
        public static ByteRenderer SHORT_ARRAY_RENDERER = new ShortArrayByteRenderer();
        public static ByteRenderer INT_ARRAY_RENDERER = new IntArrayByteRenderer();
        public static ByteRenderer FLOAT_ARRAY_RENDERER = new FloatArrayByteRenderer();
        public static ByteRenderer LONG_ARRAY_RENDERER = new LongArrayByteRenderer();
        public static ByteRenderer DOUBLE_ARRAY_RENDERER = new DoubleArrayByteRenderer();

        //    protected HeapStream _heapStream;
        protected Stream _heapStream;

        //public virtual HeapStream Heap
        public virtual Stream Heap
        {
            get
            {
                return _heapStream;
            }

            set
            {
                _heapStream = value;
            }
        }

        public static ByteRenderer GetByteRenderer(Type t)
        {
            return (ByteRenderer)_byteRenderers[t];
        }

        public abstract int Render(object o, ref byte[] outputBuf, int pos);

        public abstract void Write(object o, Stream s);

        public class NullByteRenderer : ByteRenderer
        {
            public override int Render(object o, ref byte[] outputBuf, int pos)
            {
                if (pos + _b.Length <= outputBuf.Length)
                {
                    Array.Copy(_b, 0, outputBuf, pos, _b.Length);

                    return _b.Length;
                }

                return 0;
            }

            public override void Write(object o, Stream s)
            {
                s.Write(_b, 0, _b.Length);
            }

            private byte[] _b = new byte[1];
        }

        public class ByteArrayByteRenderer : ByteRenderer
        {
            public override int Render(object o, ref byte[] outputBuf, int pos)
            {
                var input = (byte[])o;

                if (pos + input.Length <= outputBuf.Length)
                {
                    Array.Copy(input, 0, outputBuf, pos, input.Length);

                    return input.Length;
                }

                return 0;
            }

            public override void Write(object o, Stream s)
            {
                s.Write((byte[])o, 0, ((byte[])o).Length);
            }
        }

        public class CharArrayByteRenderer : ByteRenderer
        {
            public override int Render(object o, ref byte[] outputBuf, int pos)
            {
                var input = (char[])o;

                if (pos + input.Length <= outputBuf.Length)
                {
                    for (var i = 0; i < input.Length; ++i)
                    {
                        outputBuf[pos + i] = (byte)input[i];
                    }

                    return input.Length;
                }

                return 0;
            }

            public override void Write(object o, Stream s)
            {
                _stringStream.TargetStream = s;
                _stringStream.Write((char[])o);
            }

            private StringToByteStream _stringStream = new StringToByteStream(null);
        }

        public class StringArrayByteRendererHeap : ByteRenderer
        {
            public StringArrayByteRendererHeap()
            {
                _buf = new byte[1024];
            }

            //public override HeapStream Heap
            public override Stream Heap
            {
                get
                {
                    return _heapStream;
                }

                set
                {
                    _heapStream = value;
                    _stringStream = new StringToByteStream(_heapStream);
                }
            }

            /// <summary>
            /// Writes the contents of o[0] (o must be of type String[]) to the heap tempfile.
            /// Writes the heap offset and length of o[0] to outputBuf
            /// (which ends up in the binary table)
            /// </summary>
            /// <param name="o"></param>
            /// <param name="outputBuf"></param>
            /// <param name="pos"></param>
            /// <returns></returns>
            public override int Render(object o, ref byte[] outputBuf, int pos)
            {
                var s = ((string[])o)[0];

                // pos + two 4-byte ints (offset and length in heap)
                if (s != null && pos + 8 <= outputBuf.Length)
                {
                    //char[] c = s.ToCharArray();
                    var heapPos = (int)Heap.Position; // in C# it's a long, but FITS only allows int
                    _stringStream.Write(s);

                    #region oldcrap

                    /*
                              for(int i = 0; i < c.Length; ++i)
                              {
                                Heap.WriteByte((byte)c[i]);
                              }
                              */
                    /*

                              // output to the heap tempfile is also buffered right here.
                              int j = 0;
                              for(int i = 0; i < c.Length;)
                              {
                                for(j = 0; j < _buf.Length && i + j < c.Length; ++j)
                                {
                                  _buf[j] = (byte)c[i + j];
                                }

                                Heap.Write(_buf, 0, j);
                                i += j;
                              }
                              */

                    #endregion oldcrap

                    var bytes = BitConverter.GetBytes(s.Length);
                    Array.Reverse(bytes);
                    Array.Copy(bytes, 0, outputBuf, pos, bytes.Length);
                    bytes = BitConverter.GetBytes(s.Length > 0 ? heapPos : 0);
                    Array.Reverse(bytes);
                    Array.Copy(bytes, 0, outputBuf, pos + 4, bytes.Length);

                    return 8;
                }

                return 0;
            }

            public override void Write(object o, Stream s)
            {
                var str = ((string[])o)[0];

                // pos + two 4-byte ints (offset and length in heap)
                if (str != null)
                {
                    var heapPos = (int)Heap.Position; // in C# it's a long, but FITS only allows int
                    _stringStream.Write(str);
                    _stringStream.Flush();

                    var bytes = BitConverter.GetBytes(str.Length);
                    Array.Reverse(bytes);
                    s.Write(bytes, 0, bytes.Length);
                    if (str.Length > 0)
                    {
                        bytes = BitConverter.GetBytes(heapPos);
                    }
                    else
                    {
                        bytes = BitConverter.GetBytes((int)0);
                    }

                    //bytes = BitConverter.GetBytes(s.Length > 0 ? heapPos : 0);
                    Array.Reverse(bytes);
                    s.Write(bytes, 0, bytes.Length);
                }
            }

            protected byte[] _buf;
            protected StringToByteStream _stringStream;
        }

        public class StringArrayByteRendererTruncate : ByteRenderer
        {
            public StringArrayByteRendererTruncate() : this(128)
            {
            }

            //public StringArrayByteRendererTruncate(int arrayLength) : this(arrayLength, ' ', true, true)
            public StringArrayByteRendererTruncate(int arrayLength) : this(arrayLength, ' ', true, false)
            {
            }

            public StringArrayByteRendererTruncate(int arrayLength, char padChar, bool padLeft, bool trimLeft)
            {
                _s = new StringToByteStream(null);
                _arrayLength = arrayLength;
                _padChar = padChar;
                _padLeft = padLeft;
                _trimLeft = trimLeft;
                _emptyString = new string(_padChar, _arrayLength);
            }

            public override int Render(object o, ref byte[] outputBuf, int pos)
            {
                var s = PrepareString(((string[])o)[0]);
                var input = s.ToCharArray();

                if (pos + input.Length <= outputBuf.Length)
                {
                    for (var i = 0; i < input.Length; ++i)
                    {
                        outputBuf[pos + i] = (byte)input[i];
                    }

                    return input.Length;
                }

                return 0;
            }

            public override void Write(object o, Stream s)
            {
                _s.TargetStream = s;
                _s.Write(PrepareString(((string[])o)[0]));
            }

            protected string PrepareString(string s)
            {
                if (s == null)
                {
                    s = _emptyString;
                }
                else if (s.Length > _arrayLength)
                {
                    Console.Error.WriteLine("Warning: String value too wide for column; truncating.");
                    if (_trimLeft)
                    {
                        s = s.Substring(s.Length - _arrayLength, _arrayLength);
                    }
                    else
                    {
                        s = s.Substring(0, _arrayLength);
                    }
                }
                else if (s.Length < _arrayLength)
                {
                    if (_padLeft)
                    {
                        s = s.PadLeft(_arrayLength, _padChar);
                    }
                    else
                    {
                        s = s.PadRight(_arrayLength, _padChar);
                    }
                }

                return s;
            }

            protected StringToByteStream _s;
            protected string _emptyString;
            protected bool _trimLeft;
            protected bool _padLeft;
            protected char _padChar;
            protected int _arrayLength;
        }

        public class TrooleanArrayByteRenderer : ByteRenderer
        {
            public override int Render(object o, ref byte[] outputBuf, int pos)
            {
                var input = (Troolean[])o;

                if (pos + input.Length <= outputBuf.Length)
                {
                    for (var i = 0; i < input.Length; ++i)
                    {
                        outputBuf[pos + i] = input[i].IsNull ? (byte)0 : (byte)(input[i].Val ? 'T' : 'F');
                    }

                    return input.Length;
                }

                return 0;
            }

            public override void Write(object o, Stream s)
            {
                var input = (Troolean[])o;

                for (var i = 0; i < input.Length; ++i)
                {
                    if (input[i].IsNull)
                    {
                        s.WriteByte((byte)0);
                    }
                    else if (input[i].Val)
                    {
                        s.WriteByte((byte)'T');
                    }
                    else
                    {
                        s.WriteByte((byte)'F');
                    }
                }
            }
        }

        public class ShortArrayByteRenderer : ByteRenderer
        {
            public override int Render(object o, ref byte[] outputBuf, int pos)
            {
                var input = (short[])o;

                if (pos + (input.Length * _byteStride) <= outputBuf.Length)
                {
                    for (var i = 0; i < input.Length; ++i)
                    {
                        var bytes = BitConverter.GetBytes(input[i]);
                        Array.Reverse(bytes);
                        Array.Copy(bytes, 0, outputBuf, pos + (i * bytes.Length), bytes.Length);
                    }

                    return input.Length * _byteStride;
                }

                return 0;
            }

            public override void Write(object o, Stream s)
            {
                var input = (short[])o;

                for (var i = 0; i < input.Length; ++i)
                {
                    var bytes = BitConverter.GetBytes(input[i]);
                    Array.Reverse(bytes);
                    s.Write(bytes, 0, bytes.Length);
                }
            }

            protected static int _byteStride = (int)PrimitiveInfo.sizes[typeof(short)];
        }

        public class IntArrayByteRenderer : ByteRenderer
        {
            public override int Render(object o, ref byte[] outputBuf, int pos)
            {
                var input = (int[])o;

                if (pos + (input.Length * _byteStride) <= outputBuf.Length)
                {
                    for (var i = 0; i < input.Length; ++i)
                    {
                        var bytes = BitConverter.GetBytes(input[i]);
                        Array.Reverse(bytes);
                        Array.Copy(bytes, 0, outputBuf, pos + (i * bytes.Length), bytes.Length);
                    }

                    return input.Length * _byteStride;
                }

                return 0;
            }

            public override void Write(object o, Stream s)
            {
                var input = (int[])o;

                for (var i = 0; i < input.Length; ++i)
                {
                    var bytes = BitConverter.GetBytes(input[i]);
                    Array.Reverse(bytes);
                    s.Write(bytes, 0, bytes.Length);
                }
            }

            protected static int _byteStride = (int)PrimitiveInfo.sizes[typeof(int)];
        }

        public class FloatArrayByteRenderer : ByteRenderer
        {
            public override int Render(object o, ref byte[] outputBuf, int pos)
            {
                var input = (float[])o;

                if (pos + (input.Length * _byteStride) <= outputBuf.Length)
                {
                    for (var i = 0; i < input.Length; ++i)
                    {
                        var bytes = BitConverter.GetBytes(input[i]);
                        Array.Reverse(bytes);
                        Array.Copy(bytes, 0, outputBuf, pos + (i * bytes.Length), bytes.Length);
                    }

                    return input.Length * _byteStride;
                }

                return 0;
            }

            public override void Write(object o, Stream s)
            {
                var input = (float[])o;

                for (var i = 0; i < input.Length; ++i)
                {
                    var bytes = BitConverter.GetBytes(input[i]);
                    Array.Reverse(bytes);
                    s.Write(bytes, 0, bytes.Length);
                }
            }

            protected static int _byteStride = (int)PrimitiveInfo.sizes[typeof(float)];
        }

        public class LongArrayByteRenderer : ByteRenderer
        {
            public override int Render(object o, ref byte[] outputBuf, int pos)
            {
                var input = (long[])o;

                if (pos + (input.Length * _byteStride) <= outputBuf.Length)
                {
                    for (var i = 0; i < input.Length; ++i)
                    {
                        var bytes = BitConverter.GetBytes(input[i]);
                        Array.Reverse(bytes);
                        Array.Copy(bytes, 0, outputBuf, pos + (i * bytes.Length), bytes.Length);
                    }

                    return input.Length * _byteStride;
                }

                return 0;
            }

            public override void Write(object o, Stream s)
            {
                var input = (long[])o;

                for (var i = 0; i < input.Length; ++i)
                {
                    var bytes = BitConverter.GetBytes(input[i]);
                    Array.Reverse(bytes);
                    s.Write(bytes, 0, bytes.Length);
                }
            }

            protected static int _byteStride = (int)PrimitiveInfo.sizes[typeof(long)];
        }

        public class DoubleArrayByteRenderer : ByteRenderer
        {
            public override int Render(object o, ref byte[] outputBuf, int pos)
            {
                var input = (double[])o;

                if (pos + (input.Length * _byteStride) <= outputBuf.Length)
                {
                    for (var i = 0; i < input.Length; ++i)
                    {
                        var bytes = BitConverter.GetBytes(input[i]);
                        Array.Reverse(bytes);
                        Array.Copy(bytes, 0, outputBuf, pos + (i * bytes.Length), bytes.Length);
                    }

                    return input.Length * _byteStride;
                }

                return 0;
            }

            public override void Write(object o, Stream s)
            {
                var input = (double[])o;

                for (var i = 0; i < input.Length; ++i)
                {
                    var bytes = BitConverter.GetBytes(input[i]);
                    Array.Reverse(bytes);
                    s.Write(bytes, 0, bytes.Length);
                }
            }

            protected static int _byteStride = (int)PrimitiveInfo.sizes[typeof(double)];
        }

        protected static Hashtable _byteRenderers;

        static ByteRenderer()
        {
            _byteRenderers = new DefaultValueHashtable(new NullByteRenderer())
            {
                { typeof(byte[]), BYTE_ARRAY_RENDERER },
                { typeof(char[]), CHAR_ARRAY_RENDERER },
                //_byteRenderers.Add(typeof(String[]), STRING_ARRAY_RENDERER_TRUNCATE);
                { typeof(Troolean[]), TROOLEAN_ARRAY_RENDERER },
                { typeof(short[]), SHORT_ARRAY_RENDERER },
                { typeof(int[]), INT_ARRAY_RENDERER },
                { typeof(float[]), FLOAT_ARRAY_RENDERER },
                { typeof(long[]), LONG_ARRAY_RENDERER },
                { typeof(double[]), DOUBLE_ARRAY_RENDERER }
            };
        }
    }
}
