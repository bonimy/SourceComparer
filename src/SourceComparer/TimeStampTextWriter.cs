// <copyright file="TimeStampTextWriter.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SourceComparer
{
    public class TimeStampTextWriter : TextWriter
    {
        private TextWriter BaseTextWriter
        {
            get;
            set;
        }

        public TimeStampTextWriter(TextWriter baseTextWriter)
        {
            BaseTextWriter = baseTextWriter ?? throw new ArgumentNullException(nameof(baseTextWriter));
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
                BaseTextWriter.NewLine = value;
            }
        }

        public static void TimeStamp(TextWriter textWriter)
        {
            textWriter.Write("{0}: ", DateTime.Now);
        }

        public override void Close()
        {
            BaseTextWriter.Close();
        }

        public override void Flush()
        {
            BaseTextWriter.Flush();
        }

        public override Task FlushAsync()
        {
            return new Task(Flush);
        }

        public override void Write(bool value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(char value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(char[] buffer)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(buffer);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(buffer, index, count);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(decimal value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(double value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(float value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(int value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(long value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(object value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(string format, object arg0)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(format, arg0);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(format, arg0, arg1);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(format, arg0, arg1);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(string format, params object[] arg)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(format, arg);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(string value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(uint value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void Write(ulong value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.Write(value);
            }

            BaseTextWriter.Write(sb);
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
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine();
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(bool value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(char value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(char[] buffer)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(buffer);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(buffer, index, count);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(decimal value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(double value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(float value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(int value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(long value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(object value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(string format, object arg0)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(format, arg0);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(format, arg0, arg1);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(format, arg0, arg1, arg2);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(format, arg);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(string value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(uint value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(value);
            }

            BaseTextWriter.Write(sb);
        }

        public override void WriteLine(ulong value)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                TimeStamp(writer);
                writer.WriteLine(value);
            }

            BaseTextWriter.Write(sb);
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
                BaseTextWriter.Dispose();
            }

            base.Dispose(disposing);
        }

        public override bool Equals(object obj)
        {
            return BaseTextWriter.Equals(obj);
        }
    }
}
