// <copyright file="CollectionTextWriter.cs" company="Public Domain">
//     Copyright (c) 2018 Nelson Garcia. All rights reserved
//     Licensed under GNU Affero General Public License.
//     See LICENSE in project root for full license information, or
//     https://www.gnu.org/licenses/#AGPL
// </copyright>

namespace Helper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class CollectionTextWriter : ProxyTextWriter, IEnumerable<TextWriter>
    {
        public CollectionTextWriter(
            IEnumerable<TextWriter> textWriterCollection)
            : base(GetFirst(textWriterCollection))
        {
            if (textWriterCollection is null)
            {
                throw new ArgumentNullException(nameof(textWriterCollection));
            }

            var list = new List<TextWriter>();
            foreach (var textWriter in textWriterCollection)
            {
                if (textWriter is null)
                {
                    throw new ArgumentException("base collection cannot have a null entry.");
                }

                list.Add(textWriter);
            }

            BaseTextWriterCollection = list;
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
                foreach (var textWriter in BaseTextWriterCollection)
                {
                    textWriter.NewLine = value;
                }
            }
        }

        private IEnumerable<TextWriter> BaseTextWriterCollection
        {
            get;
        }

        public override void Close()
        {
            BaseAction(textWriter => textWriter.Close());
        }

        public override void Flush()
        {
            BaseAction(textWriter => textWriter.Flush());
        }

        public IEnumerator<TextWriter> GetEnumerator()
        {
            return BaseTextWriterCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BaseAction(textWriter => textWriter.Dispose());
            }

            base.Dispose(disposing);
        }

        protected override void BaseAction(Action<TextWriter> action)
        {
            foreach (var textWriter in this)
            {
                action(textWriter);
            }
        }

        private static T GetFirst<T>(IEnumerable<T> collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            foreach (var item in collection)
            {
                return item;
            }

            return default;
        }
    }
}
