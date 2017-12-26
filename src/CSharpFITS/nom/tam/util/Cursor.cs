// <copyright file="Cursor.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

namespace nom.tam.util
{
    using System.Collections;

    /// <summary>This interface extends the IEnumerator interface
    /// to allow insertion of data and move to previous entries
    /// in a collection.
    /// </summary>
    public interface Cursor : IEnumerator
    {
        /// <summary>Point the list at a particular element.
        /// Point to the end of the list if the key is not found.
        /// </summary>
        object Key
        {
            get;
            set;
        }

        /// <summary>Move to the previous element</summary>
        bool MovePrevious();

        /// <summary>Add an unkeyed element to the collection.
        /// The new element is placed such that it will be called
        /// by a prev() call, but not a next() call.
        /// </summary>
        void Add(object val);

        void Insert(object key, object val);

        /// <summary>Add a keyed element to the collection.
        /// The new element is placed such that it will be called
        /// by a prev() call, but not a next() call.
        /// </summary>
        void Add(object key, object val);

        /// <summary>Remove the current object from the collection</summary>
        void Remove();
    }
}
