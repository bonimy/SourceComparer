// <copyright file="CollectionTextWriter.cs" company="Public Domain">
//     Copyright (c) 2017 Nelson Garcia.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SourceComparer
{
    public class CollectionTextWriter : ProxyTextWriter, IEnumerable<TextWriter>
    {
        private IEnumerable<TextWriter> BaseTextWriterCollection
        {
            get;
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

        public CollectionTextWriter(
            IEnumerable<TextWriter> textWriterCollection) :
            base(GetFirst(textWriterCollection))
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

            return default(T);
        }

        protected override void BaseAction(Action<TextWriter> action)
        {
            foreach (var textWriter in this)
            {
                action(textWriter);
            }
        }

        public override void Close()
        {
            BaseAction(textWriter => textWriter.Close());
        }

        public override void Flush()
        {
            BaseAction(textWriter => textWriter.Flush());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BaseAction(textWriter => textWriter.Dispose());
            }

            base.Dispose(disposing);
        }

        public IEnumerator<TextWriter> GetEnumerator()
        {
            return BaseTextWriterCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
