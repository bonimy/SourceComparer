// <copyright file="CollectionTextWriter.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SourceComparer
{
    public class CollectionTextWriter : TextWriter, IReadOnlyList<TextWriter>
    {
        private IReadOnlyList<TextWriter> BaseTextWriter
        {
            get;
            set;
        }

        public int Count
        {
            get
            {
                return BaseTextWriter.Count;
            }
        }

        public override IFormatProvider FormatProvider
        {
            get
            {
                return BaseTextWriter[0].FormatProvider;
            }
        }

        public override Encoding Encoding
        {
            get
            {
                return BaseTextWriter[0].Encoding;
            }
        }

        public override string NewLine
        {
            get
            {
                return BaseTextWriter[0].NewLine;
            }

            set
            {
                BaseTextWriter[0].NewLine = value;
            }
        }

        public TextWriter this[int index]
        {
            get
            {
                return BaseTextWriter[index];
            }
        }

        public CollectionTextWriter(IEnumerable<TextWriter> baseTextWriter)
        {
            if (baseTextWriter == null)
            {
                throw new ArgumentNullException(nameof(baseTextWriter));
            }

            var textWriterList = new List<TextWriter>(baseTextWriter);
            if (textWriterList.Count == 0)
            {
                throw new ArgumentException();
            }

            BaseTextWriter = textWriterList;
        }

        public IEnumerator<TextWriter> GetEnumerator()
        {
            return BaseTextWriter.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override void Close()
        {
            foreach (var textWriter in this)
            {
                textWriter.Close();
            }
        }

        public override void Flush()
        {
            foreach (var textWriter in this)
            {
                textWriter.Flush();
            }
        }

        public override Task FlushAsync()
        {
            return new Task(Flush);
        }

        public override void Write(bool value)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(value);
            }
        }

        public override void Write(char value)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(value);
            }
        }

        public override void Write(char[] buffer)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(buffer);
            }
        }

        public override void Write(char[] buffer, int index, int count)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(buffer, index, count);
            }
        }

        public override void Write(decimal value)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(value);
            }
        }

        public override void Write(double value)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(value);
            }
        }

        public override void Write(float value)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(value);
            }
        }

        public override void Write(int value)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(value);
            }
        }

        public override void Write(long value)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(value);
            }
        }

        public override void Write(object value)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(value);
            }
        }

        public override void Write(string format, object arg0)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(format, arg0);
            }
        }

        public override void Write(string format, object arg0, object arg1)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(format, arg0, arg1);
            }
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(format, arg0, arg1, arg2);
            }
        }

        public override void Write(string format, params object[] arg)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(format, arg);
            }
        }

        public override void Write(string value)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(value);
            }
        }

        public override void Write(uint value)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(value);
            }
        }

        public override void Write(ulong value)
        {
            foreach (var textWriter in this)
            {
                textWriter.Write(value);
            }
        }

        public override Task WriteAsync(char value)
        {
            return new Task(() => Write(value));
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            return new Task(() => Write(buffer, index, count));
        }

        public override Task WriteAsync(string value)
        {
            return new Task(() => Write(value));
        }

        public override void WriteLine()
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine();
            }
        }

        public override void WriteLine(bool value)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(value);
            }
        }

        public override void WriteLine(char value)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(value);
            }
        }

        public override void WriteLine(char[] buffer)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(buffer);
            }
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(buffer, index, count);
            }
        }

        public override void WriteLine(decimal value)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(value);
            }
        }

        public override void WriteLine(double value)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(value);
            }
        }

        public override void WriteLine(float value)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(value);
            }
        }

        public override void WriteLine(int value)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(value);
            }
        }

        public override void WriteLine(long value)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(value);
            }
        }

        public override void WriteLine(object value)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(value);
            }
        }

        public override void WriteLine(string format, object arg0)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(format, arg0);
            }
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(format, arg0, arg1);
            }
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(format, arg0, arg1, arg2);
            }
        }

        public override void WriteLine(string format, params object[] arg)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(format, arg);
            }
        }

        public override void WriteLine(string value)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(value);
            }
        }

        public override void WriteLine(uint value)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(value);
            }
        }

        public override void WriteLine(ulong value)
        {
            foreach (var textWriter in this)
            {
                textWriter.WriteLine(value);
            }
        }

        public override Task WriteLineAsync()
        {
            return new Task(WriteLine);
        }

        public override Task WriteLineAsync(char value)
        {
            return new Task(() => WriteLine(value));
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            return new Task(() => WriteLine(buffer, index, count));
        }

        public override Task WriteLineAsync(string value)
        {
            return new Task(() => WriteLine(value));
        }

        public override int GetHashCode()
        {
            return BaseTextWriter.GetHashCode();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var textWriter in this)
                {
                    textWriter.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
