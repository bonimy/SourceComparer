// <copyright file="ProxyTextWriter.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Helper
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class ProxyTextWriter : TextWriter
    {
        protected ProxyTextWriter(TextWriter baseTextWriter)
        {
            BaseTextWriter = baseTextWriter ??
                throw new ArgumentNullException(nameof(baseTextWriter));

            base.NewLine = BaseTextWriter.NewLine;
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
                return base.NewLine;
            }

            set
            {
                base.NewLine =
                BaseTextWriter.NewLine = value;
            }
        }

        protected TextWriter BaseTextWriter
        {
            get;
        }

        public override void Write(bool value)
        {
            BaseAction(textWriter => textWriter.Write(value));
        }

        public override void Write(char value)
        {
            BaseAction(textWriter => textWriter.Write(value));
        }

        public override void Write(int value)
        {
            BaseAction(textWriter => textWriter.Write(value));
        }

        [CLSCompliant(false)]
        public override void Write(uint value)
        {
            BaseAction(textWriter => textWriter.Write(value));
        }

        public override void Write(long value)
        {
            BaseAction(textWriter => textWriter.Write(value));
        }

        [CLSCompliant(false)]
        public override void Write(ulong value)
        {
            BaseAction(textWriter => textWriter.Write(value));
        }

        public override void Write(float value)
        {
            BaseAction(textWriter => textWriter.Write(value));
        }

        public override void Write(double value)
        {
            BaseAction(textWriter => textWriter.Write(value));
        }

        public override void Write(decimal value)
        {
            BaseAction(textWriter => textWriter.Write(value));
        }

        public override void Write(object value)
        {
            BaseAction(textWriter => textWriter.Write(value));
        }

        public override void Write(char[] buffer)
        {
            BaseAction(textWriter => textWriter.Write(buffer));
        }

        public override void Write(char[] buffer, int index, int count)
        {
            BaseAction(textWriter => textWriter.Write(buffer, index, count));
        }

        public override void Write(string value)
        {
            BaseAction(textWriter => textWriter.Write(value));
        }

        public override void Write(string format, object arg0)
        {
            BaseAction(textWriter => textWriter.Write(format, arg0));
        }

        public override void Write(string format, object arg0, object arg1)
        {
            BaseAction(textWriter => textWriter.Write(format, arg0, arg1));
        }

        public override void Write(
            string format,
            object arg0,
            object arg1,
            object arg2)
        {
            BaseAction(
                textWriter => textWriter.Write(format, arg0, arg1, arg2));
        }

        public override void Write(string format, params object[] arg)
        {
            BaseAction(textWriter => textWriter.Write(format, arg));
        }

        public override void WriteLine()
        {
            BaseAction(textWriter => textWriter.WriteLine());
        }

        public override void WriteLine(bool value)
        {
            BaseAction(textWriter => textWriter.WriteLine(value));
        }

        public override void WriteLine(char value)
        {
            BaseAction(textWriter => textWriter.WriteLine(value));
        }

        public override void WriteLine(int value)
        {
            BaseAction(textWriter => textWriter.WriteLine(value));
        }

        [CLSCompliant(false)]
        public override void WriteLine(uint value)
        {
            BaseAction(textWriter => textWriter.WriteLine(value));
        }

        public override void WriteLine(long value)
        {
            BaseAction(textWriter => textWriter.WriteLine(value));
        }

        [CLSCompliant(false)]
        public override void WriteLine(ulong value)
        {
            BaseAction(textWriter => textWriter.WriteLine(value));
        }

        public override void WriteLine(float value)
        {
            BaseAction(textWriter => textWriter.WriteLine(value));
        }

        public override void WriteLine(double value)
        {
            BaseAction(textWriter => textWriter.WriteLine(value));
        }

        public override void WriteLine(decimal value)
        {
            BaseAction(textWriter => textWriter.WriteLine(value));
        }

        public override void WriteLine(object value)
        {
            BaseAction(textWriter => textWriter.WriteLine(value));
        }

        public override void WriteLine(char[] buffer)
        {
            BaseAction(textWriter => textWriter.WriteLine(buffer));
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            BaseAction(textWriter => textWriter.WriteLine(buffer, index, count));
        }

        public override void WriteLine(string value)
        {
            BaseAction(textWriter => textWriter.WriteLine(value));
        }

        public override void WriteLine(string format, object arg0)
        {
            BaseAction(textWriter => textWriter.WriteLine(format, arg0));
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            BaseAction(textWriter => textWriter.WriteLine(format, arg0, arg1));
        }

        public override void WriteLine(
            string format,
            object arg0,
            object arg1,
            object arg2)
        {
            BaseAction(
                textWriter => textWriter.WriteLine(format, arg0, arg1, arg2));
        }

        public override void WriteLine(string format, params object[] arg)
        {
            BaseAction(textWriter => textWriter.WriteLine(format, arg));
        }

        public override void Close()
        {
            BaseTextWriter.Close();
            base.Close();
        }

        public override void Flush()
        {
            BaseTextWriter.Flush();
            base.Flush();
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

        public override Task FlushAsync()
        {
            return new Task(Flush);
        }

        protected abstract void BaseAction(Action<TextWriter> action);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BaseTextWriter.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
