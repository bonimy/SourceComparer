// <copyright file="TimeStampTextWriter.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.IO;
using System.Text;

namespace SourceComparer
{
    public class TimeStampTextWriter : ProxyTextWriter
    {
        public TimeStampTextWriter(TextWriter baseTextWriter) :
            base(baseTextWriter)
        {
        }

        public static void TimeStamp(TextWriter textWriter)
        {
            textWriter.Write("{0}: ", DateTime.Now);
        }

        protected override void BaseAction(Action<TextWriter> action)
        {
            var sb = new StringBuilder();
            using (var stringWriter = new StringWriter(sb))
            {
                TimeStamp(stringWriter);
                action(stringWriter);
            }

            BaseTextWriter.Write(sb);
        }
    }
}
