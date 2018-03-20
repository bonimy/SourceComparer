// <copyright file="TimeStampTextWriter.cs" company="Public Domain">
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

    public class TimeStampTextWriter : ProxyTextWriter
    {
        public TimeStampTextWriter(TextWriter baseTextWriter)
            : base(baseTextWriter)
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
