using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SourceComparer
{
    public class VerboseTextWriter : TextWriter
    {
        private TextWriter BaseTextWriter
        {
            get;
        }

        public bool Verbose
        {
            get;
            set;
        }

        public override IFormatProvider FormatProvider
        {
            get
            {
                return BaseTextWriter.FormatProvider;
            }
        }

        public override Encoding Encoding
        {
            get
            {
                return BaseTextWriter.Encoding;
            }
        }

        public override string NewLine
        {
            get
            {
                return BaseTextWriter.NewLine;
            }

            set
            {
                base.NewLine = value;
            }
        }

        public VerboseTextWriter(TextWriter baseTextWriter) : this(baseTextWriter, true)
        {
        }

        public VerboseTextWriter(TextWriter baseTextWriter, bool verbose)
        {
            BaseTextWriter = baseTextWriter ??
                    throw new ArgumentNullException(nameof(baseTextWriter));

            Verbose = verbose;
        }

        public override void Close()
        {
            base.Close();
        }

        public override void Flush()
        {
            base.Flush();
        }

        public override Task FlushAsync()
        {
            return base.FlushAsync();
        }

        public override void Write(bool value)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(value);
        }

        public override void Write(char value)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(value);
        }

        public override void Write(char[] buffer)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(buffer);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(buffer, index, count);
        }

        public override void Write(decimal value)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(value);
        }

        public override void Write(double value)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(value);
        }

        public override void Write(float value)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(value);
        }

        public override void Write(int value)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(value);
        }

        public override void Write(long value)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(value);
        }

        public override void Write(object value)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(value);
        }

        public override void Write(string format, object arg0)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(format, arg0);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(format, arg0, arg1);
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(format, arg0, arg1, arg2);
        }

        public override void Write(string format, params object[] arg)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(format, arg);
        }

        public override void Write(string value)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(value);
        }

        public override void Write(uint value)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(value);
        }

        public override void Write(ulong value)
        {
            if (!Verbose)
            {
                return;
            }

            base.Write(value);
        }

        public override Task WriteAsync(char value)
        {
            if (!Verbose)
            {
                return Task.CompletedTask;
            }

            return base.WriteAsync(value);
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            if (!Verbose)
            {
                return Task.CompletedTask;
            }

            return base.WriteAsync(buffer, index, count);
        }

        public override Task WriteAsync(string value)
        {
            if (!Verbose)
            {
                return Task.CompletedTask;
            }

            return base.WriteAsync(value);
        }

        public override void WriteLine()
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine();
        }

        public override void WriteLine(bool value)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(value);
        }

        public override void WriteLine(char value)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(value);
        }

        public override void WriteLine(char[] buffer)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(buffer);
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(buffer, index, count);
        }

        public override void WriteLine(decimal value)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(value);
        }

        public override void WriteLine(double value)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(value);
        }

        public override void WriteLine(float value)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(value);
        }

        public override void WriteLine(int value)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(value);
        }

        public override void WriteLine(long value)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(value);
        }

        public override void WriteLine(object value)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(value);
        }

        public override void WriteLine(string format, object arg0)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(format, arg0);
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(format, arg0, arg1);
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(format, arg0, arg1, arg2);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(format, arg);
        }

        public override void WriteLine(string value)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(value);
        }

        public override void WriteLine(uint value)
        {
            if (!Verbose)
            {
                return;
            }

            base.WriteLine(value);
        }

        public override void WriteLine(ulong value)
        {
            base.WriteLine(value);
        }

        public override Task WriteLineAsync()
        {
            if (!Verbose)
            {
                return Task.CompletedTask;
            }

            return base.WriteLineAsync();
        }

        public override Task WriteLineAsync(char value)
        {
            if (!Verbose)
            {
                return Task.CompletedTask;
            }

            return base.WriteLineAsync(value);
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            if (!Verbose)
            {
                return Task.CompletedTask;
            }

            return base.WriteLineAsync(buffer, index, count);
        }

        public override Task WriteLineAsync(string value)
        {
            if (!Verbose)
            {
                return Task.CompletedTask;
            }

            return base.WriteLineAsync(value);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BaseTextWriter.Dispose();
            }
        }
    }
}
