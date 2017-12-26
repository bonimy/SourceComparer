// <copyright file="HashedList.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

namespace nom.tam.util
{
    /* Copyright: Thomas McGlynn 1999.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	*/

    /*
       <summary>This class implements a structure which can
       be accessed either through a hash or
       as linear list.  Only some elements may have
       a hash key.

       This class is motivated by the FITS header
       structure where a user may wish to go through
       the header element by element, or jump directly
       to a given keyword.  It assumes that all
       keys are unique.  However, all elements in the
       structure need not have a key.

       This class does only the search structure
       and knows nothing of the semantics of the
       referenced objects.

       Users may wish to access the HashedList using
       HashedListCursor which extends the Cursor
       interface to allow adding and deleting entries.
       </summary>
      */

    using System;
    using System.Collections;

    public class HashedList : IDictionary
    {
        #region Constructors

        public HashedList()
        {
            InitBlock();
        }

        private void InitBlock()
        {
            hash = new Hashtable();
        }

        #endregion Constructors

        #region Properties

        public object this[object key]
        {
            get
            {
                if (!hash.ContainsKey(key))
                {
                    return null;
                }

                return ((HashedListElement)hash[key]).Value;
            }

            set
            {
                Add(key, value);
            }
        }

        /// <summary>Is the HashedList empty?</summary>
        public bool Empty
        {
            get
            {
                return numElements == 0;
            }
        }

        #endregion Properties

        /// <summary>This class defines the elements used to construct the HashedList.
        /// External users should not need to see this class.</summary>
        public class HashedListElement
        {
            public object Key
            {
                get
                {
                    return key;
                }

                set
                {
                    key = value;
                }
            }

            public object Value
            {
                get
                {
                    return val;
                }

                set
                {
                    val = value;
                }
            }

            public HashedListElement()
            {
            }

            public HashedListElement(object key, object val)
            {
                Key = key;
                Value = val;
            }

            public HashedListElement(object key, object val,
                                     HashedListElement prev, HashedListElement next) : this(key, val)
            {
                this.prev = prev;
                this.next = next;
            }

            protected object key;
            protected object val;

            /// <summary>A pointer to the next object in the list</summary>
            internal HashedListElement next;

            /// <summary>A pointer to the previous object in the list</summary>
            internal HashedListElement prev;
        }

        /*

        /// <summary>This inner class defines a Cursor over the hashed list.
            /// A Cursor need not start at the beginning of the list.
            /// <p>
            /// This class can be used by external users to both add
            /// and delete elements from the list.  It implements the
            /// standard IEnumerator interface but also provides methods
            /// to add keyed or unkeyed elements at the current location.
            /// <p>
            /// Users may move either direction in the list using the
            /// next and prev calls.  Note that a call to prev followed
            /// by a call to next (or vice versa) will return the same
            /// element twice.
            /// <p>
            /// The class is implemented as an inner class so that it can
            /// easily access the state of the associated HashedList.
            /// </summary>
            */

        public class HashedListCursor : Cursor, System.Collections.IDictionaryEnumerator
        {
            protected class NullKey
            {
            }

            #region IDictionaryEnumerator Members

            public DictionaryEntry Entry
            {
                get
                {
                    if (current == null)
                    {
                        throw new InvalidOperationException("Attempt to retrieve from IEnumerator in wrong state.");
                    }

                    return new DictionaryEntry(current.Key ?? new NullKey(), current.Value);
                }
            }

            public object Key
            {
                get
                {
                    if (current == null)
                    {
                        throw new InvalidOperationException("Attempt to retrieve from IEnumerator in wrong state.");
                    }

                    return current.Key;
                }

                /// <summary>Point the Cursor to a particular keyed entry.  This
                /// method is not in the IEnumerator interface.</summary>
                /// <param name="">key</param>
                set
                {
                    if (Enclosing_Instance.ContainsKey(value))
                    {
                        current = (HashedListElement)Enclosing_Instance.hash[value];
                        last = current.prev;
                        beforeStart = false;
                    }
                    else
                    {
                        Reset();
                    }
                }
            }

            public object Value
            {
                get
                {
                    if (current == null)
                    {
                        throw new InvalidOperationException("Attempt to retrieve from IEnumerator in wrong state.");
                    }

                    return current.Value;
                }
            }

            #endregion IDictionaryEnumerator Members

            #region IEnumerator Methods

            public object Current
            {
                get
                {
                    return current == null ? null : (object)Entry;
                }
            }

            /// <summary>Is there another element?</summary>
            public bool MoveNext()
            {
                if (beforeStart)
                {
                    last = null;
                    current = startElement;
                    beforeStart = false;
                    return current != null;
                }
                else if (current != null)
                {
                    last = current;
                    current = current.next;
                    return current != null;
                }

                return false;
            }

            public void Reset()
            {
                beforeStart = true;
                current = null;
                last = null;
            }

            #endregion IEnumerator Methods

            #region Cursor Methods

            /// <summary>Move to the previous entry.</summary>
            public bool MovePrevious()
            {
                if (beforeStart)
                {
                    return false;
                }
                else if (current == null)
                {
                    if (last == null)
                    {
                        throw new Exception("Empty list");
                    }
                    else
                    {
                        current = last;
                        last = current.prev;
                        return true;
                    }
                }
                else
                {
                    current = current.prev;
                    last = current.prev;
                    return true;
                }
            }

            /// <summary>Remove the current entry. Note that remove can
            /// be called only after a call to next, and only once per such
            /// call.  Remove cannot be called after a call to prev.</summary>
            public void Remove()
            {
                if (current == null)
                {
                    throw new System.SystemException("Removed called in invalid cursor state");
                }
                else
                {
                    var e = current.prev;
                    Enclosing_Instance.RemoveElement(current);
                    current = e;
                    last = current?.prev;
                }
            }

            /// <summary>Add an entry at the current location. The new entry goes before
            /// the entry that would be returned in the next 'next' call, and
            /// that call will not be affected by the insertion.
            /// Note: this method is not in the IEnumerator interface.</summary>
            public void Add(object val)
            {
                var newObj = Enclosing_Instance.Add(current, null, val);
                MoveNext();
            }

            public void Insert(object key, object val)
            {
                Enclosing_Instance.Insert(current, key, val);
            }

            /// <summary>Add a keyed entry at the current location. The new entry is inserted
            /// before the entry that would be returned in the next invocation of
            /// 'next'.  The return value for that call is unaffected.
            /// Note: this method is not in the IEnumerator interface.
            /// </summary>
            public void Add(object key, object val)
            {
                var newObj = Enclosing_Instance.Add(current, key, val);
                MoveNext();
            }

            #endregion Cursor Methods

            #region Book-keeping Crap

            internal HashedListCursor(HashedList enclosingInstance, HashedListElement start)
            {
                this.enclosingInstance = enclosingInstance;
                startElement = start;
                beforeStart = true;
                current = null;
                last = null;
            }

            protected HashedList Enclosing_Instance
            {
                get
                {
                    return enclosingInstance;
                }
            }

            /*
      internal HashedListElement Current
      {
        get
        {
          return current;
        }

        /// <summary>Point the Cursor to a specific entry.</summary>
        set
        {
          current = value;
          last = null;
        }
      }
*/
            private HashedList enclosingInstance;
            protected HashedListElement startElement;

            protected bool beforeStart;

            /// <summary>The element that will be returned by next.</summary>
            private HashedListElement current;

            /// <summary>The last element that was returned by next.</summary>
            private HashedListElement last;

            #endregion Book-keeping Crap
        }

        #region Instance Variables

        /// <summary>The HashMap of keyed indices.</summary>
        private Hashtable hash;

        /// <summary>The first element of the list.</summary>
        private HashedListElement first = null;

        /// <summary>The last element of the list.</summary>
        private HashedListElement last = null;

        /// <summary>The number of elements in the list.</summary>
        private int numElements = 0;

        #endregion Instance Variables

        #region Methods

        #region Add Methods

        /// <summary>
        ///   Insert an element in front of pos.
        ///   If pos is null, add to the end of the list.
        /// </summary>
        /// <param name="pos">The element in front of which to add the new element</param>
        /// <param name="key">The key of the new element</param>
        /// <param name="val">The value of the new element</param>
        public void Insert(HashedListElement pos, object key, object val)
        {
            if (pos == null)
            {
                Add(key, val);
            }
            else
            {
                var prev = pos.prev;
                var newElement = new HashedListElement(key, val, prev, pos);
                pos.prev = newElement;

                if (prev != null)
                {
                    prev.next = newElement;
                }

                if (first == pos)
                {
                    first = newElement;
                }

                // Put a pointer in the hash.
                if (key != null)
                {
                    hash[key] = newElement;
                }

                numElements += 1;
            }
        }

        /// <summary>Add an element to the end of the list.</summary>
        public void Add(object val)
        {
            Add(null, null, val);
        }

        /// <summary>Add an element to the list.</summary>
        /// <param name="pos">The element after which the current element
        /// be placed.  If pos is null put the element at the end of the list.</param>
        /// <param name="key">The hash key for the new object.  This may be null
        /// for an unkeyed entry.</param>
        /// <param name="reference">The actual object being stored.</param>
        internal HashedListElement Add(HashedListElement pos, object key, object val)
        {
            // First check if we need to delete another reference.
            if (key != null)
            {
                // Does not do anything if key not found.
                Remove((HashedListElement)hash[key]);
            }

            var e = new HashedListElement
            {
                Key = key,
                Value = val
            };

            // Now put it in the list.
            if (pos == null)
            {
                // At the end...
                e.prev = last;
                if (last != null)
                {
                    last.next = e;
                }
                else
                {
                    // Empty list...
                    first = e;
                }

                last = e;
                e.next = null;
            }
            else
            {
                if (pos.next == null)
                {
                    // At the end...
                    e.next = null;
                    e.prev = pos;
                    pos.next = e;
                    last = e;
                }
                else
                {
                    // In the middle...
                    e.prev = pos;
                    e.next = pos.next;
                    pos.next = e;
                    e.next.prev = e;
                }
            }

            // Put a pointer in the hash.
            if (key != null)
            {
                hash[key] = e;
            }

            numElements += 1;

            return e;
        }

        #endregion Add Methods

        #region Remove Methods

        /// <summary>Remove an element from the list.
        /// This method is also called by the HashedListCursor.
        /// </summary>
        /// <param name="e">The element to be removed.</param>
        private void RemoveElement(HashedListElement e)
        {
            if (e == null)
            {
                return;
            }
            if (e.prev != null)
            {
                e.prev.next = e.next;
            }
            else
            {
                first = e.next;
            }

            if (e.next != null)
            {
                e.next.prev = e.prev;
            }
            else
            {
                last = e.prev;
            }

            if (e.Key != null)
            {
                hash.Remove(e.Key);
            }

            numElements -= 1;
        }

        #endregion Remove Methods

        #region Cursor Methods

        /// <summary>Return a Cursor over the entire list.
        /// The Cursor may be used to delete
        /// entries as well as to retrieve existing
        /// entries.  A knowledgeable user can
        /// cast this to a HashedListCursor and
        /// use it to add as well as delete entries.
        /// </summary>
        public Cursor GetCursor()
        {
            return new HashedListCursor(this, first);
        }

        public HashedListCursor GetCursor(object key)
        {
            var c = (HashedListCursor)GetCursor();
            c.Key = key;
            return c;
        }

        /// <summary>Return a Cursor starting with the n'th entry.</summary>
        public HashedListCursor GetCursor(int n)
        {
            if (n == 0 && numElements == 0)
            {
                return new HashedListCursor(this, first);
            }

            var e = GetElement(n);
            var c = (HashedListCursor)GetCursor();

            //			c.Current = e;
            c.Key = e.Key;
            return c;
        }

        #endregion Cursor Methods

        #region Element Methods

        /// <summary>Return the n'th entry from the beginning.</summary>
        //public System.Object getElement(int n)
        public HashedListElement GetElement(int n)
        {
            //HashedListElement e = getNthElement(n);
            return GetNthElement(n);

            //return e.Value;
        }

        /// <summary>Replace the key of a given element.</summary>
        /// <param name="oldKey"> The previous key.  This key must
        /// be present in the hash.</param>
        /// <param name="newKey"> The new key.  This key
        /// must not be present in the hash.</param>
        /// <returns>if the replacement was successful.</returns>
        public bool ReplaceKey(object oldKey, object newKey)
        {
            if (!hash.ContainsKey(oldKey) || hash.ContainsKey(newKey))
            {
                return false;
            }

            var e = (HashedListElement)hash[oldKey];
            hash.Remove(oldKey);
            e.Key = newKey;
            hash[newKey] = e;

            return true;
        }

        /// <summary>Get the n'th element of the list</summary>
        internal HashedListElement GetNthElement(int n)
        {
            if (n < 0)
            {
                throw new System.Exception("Invalid index");
            }
            else
            {
                var current = first;

                while (n > 0 && current.next != null)
                {
                    n -= 1;
                    current = current.next;
                }

                if (n > 0)
                {
                    throw new System.Exception("Index beyond end of list");
                }

                if (current == null)
                {
                    throw new System.Exception("Empty list");
                }

                return current;
            }
        }

        public bool ContainsValue(object val)
        {
            if (val == null)
            {
                return false;
            }

            var result = hash.ContainsValue(val);

            for (var e = first; (!result) && e != null;)
            {
                result = val.Equals(e.Value);
                e = e.next;
            }

            return result;
        }

        /// <summary>Check if the key is included in the list.</summary>
        public bool ContainsKey(object key)
        {
            return hash.ContainsKey(key);
        }

        /// <summary>Clear the collection
        /// </summary>
        public void Clear()
        {
            numElements = 0;
            first = null;
            last = null;
            hash.Clear();
        }

        #endregion Element Methods

        #endregion Methods

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetCursor();
        }

        #endregion IEnumerable Members

        #region ICollection Members

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public int Count
        {
            get
            {
                return numElements;
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new InvalidCastException("Sorry, elements of this list can't be copied into an array.");
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        #endregion ICollection Members

        #region IDictionary Members

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return (IDictionaryEnumerator)GetCursor();
        }

        /// <summary>Remove a keyed object from the list.  Unkeyed
        /// objects can be removed from the list using a
        /// HashedListCursor.</summary>
        public void Remove(object key)
        {
            if (key == null)
            {
                return;
            }

            var h = (HashedListElement)hash[key];
            if (h != null)
            {
                RemoveElement(h);
            }
        }

        public void RemoveValue(object val)
        {
            for (var c = (HashedListCursor)GetCursor(); c.MoveNext();)
            {
                if (c.Value.Equals(val))
                {
                    c.Remove();
                }
            }
        }

        public void RemoveUnkeyedObject(object val)
        {
            for (var c = (HashedListCursor)GetCursor(); c.MoveNext();)
            {
                if (c.Key == null && c.Value.Equals(val))
                {
                    c.Remove();
                }
            }
        }

        public bool Contains(object key)
        {
            return hash.Contains(key);
        }

        public ICollection Values
        {
            get
            {
                return hash.Values;
            }
        }

        /// <summary>Add a keyed element to the end of the list.</summary>
        public void Add(object key, object val)
        {
            Add(null, key, val);
        }

        public ICollection Keys
        {
            get
            {
                return hash.Keys;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        #endregion IDictionary Members
    }
}
