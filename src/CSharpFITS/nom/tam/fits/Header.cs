// <copyright file="Header.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

namespace nom.tam.fits
{
    /* Copyright: Thomas McGlynn 1997-1999.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	*
	* Many thanks to David Glowacki (U. Wisconsin) for substantial
	* improvements, enhancements and bug fixes.
	*/

    using System;
    using System.Collections;
    using System.IO;
    using nom.tam.util;

    /// <summary>This class describes methods to access and manipulate the header
    /// for a FITS HDU. This class does not include code specific
    /// to particular types of HDU.
    /// </summary>
    public class Header : FitsElement
    {
        #region Properties

        /// <summary>Can the header be rewritten without rewriting the entire file?</summary>
        public virtual bool Rewriteable
        {
            get
            {
                if (fileOffset >= 0 && (cards.Count + 35) / 36 == (oldSize + 35) / 36)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>Find the number of cards in the header</summary>
        virtual public int NumberOfCards
        {
            get
            {
                return cards.Count;
            }
        }

        /// <summary>Get the offset of this header</summary>
        virtual public long FileOffset
        {
            get
            {
                return fileOffset;
            }
        }

        /// <summary>Return the size of the data including any needed padding.</summary>
        /// <returns> the data segment size including any needed padding.</returns>
        virtual public long DataSize
        {
            get
            {
                return FitsUtil.AddPadding(TrueDataSize());
            }
        }

        /// <summary>Get the size of the header in bytes</summary>
        virtual public long Size
        {
            get
            {
                return HeaderSize();
            }
        }

        /// <summary>Is this a valid header.</summary>
        /// <returns> <CODE>true</CODE> for a valid header, <CODE>false</CODE> otherwise.</returns>
        internal bool ValidHeader
        {
            get
            {
                if (NumberOfCards < 4)
                {
                    return false;
                }
                cursor = GetCursor();
                cursor.MoveNext();
                var key = (string)cursor.Key;
                if (!key.Equals("SIMPLE") && !key.Equals("XTENSION"))
                {
                    return false;
                }
                cursor.MoveNext();
                key = (string)cursor.Key;
                if (!key.Equals("BITPIX"))
                {
                    return false;
                }
                cursor.MoveNext();
                key = (string)cursor.Key;
                if (!key.Equals("NAXIS"))
                {
                    return false;
                }
                while (cursor.MoveNext())
                {
                    key = (string)cursor.Key;
                }
                if (!key.Equals("END"))
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>Set the SIMPLE keyword to the given value.
        /// </summary>
        /// <param name="val">The boolean value -- Should be true for FITS data.
        ///
        /// </param>
        virtual public bool Simple
        {
            set
            {
                DeleteKey("SIMPLE");
                DeleteKey("XTENSION");

                // If we're flipping back to and from the primary header
                // we need to add in the EXTEND keyword whenever we become
                // a primary, because it's not permitted in the extensions
                // (at least not where it needs to be in the primary array).
                if (FindCard("NAXIS") != null)
                {
                    var nax = GetIntValue("NAXIS");

                    cursor = GetCursor();

                    if (FindCard("NAXIS" + nax) != null)
                    {
                        cursor.MoveNext();
                        var hc = (HeaderCard)((DictionaryEntry)cursor.Current).Value;
                        try
                        {
                            RemoveCard("EXTEND");
                            cursor.Add("EXTEND", new HeaderCard("EXTEND", true, "Extensions are permitted"));
                        }
                        catch (Exception)
                        {
                            // Ignore the exception
                        }
                    }
                }

                cursor = GetCursor();
                cursor.MoveNext();

                //try
                //{
                //cursor.Add("SIMPLE", new HeaderCard("SIMPLE", value, "C# FITS: " + DateTime.Now));
                cursor.Insert("SIMPLE", new HeaderCard("SIMPLE", value, "C# FITS: " + DateTime.Now));

                //}
                //catch (HeaderCardException e)
                //{
                //					Console.Error.WriteLine("Impossible exception at setSimple " + e);
                //}
            }
        }

        /// <summary>Set the XTENSION keyword to the given value.</summary>
        /// <param name="val">The name of the extension. "IMAGE" and "BINTABLE" are supported.</param>
        public System.String Xtension
        {
            set
            {
                DeleteKey("SIMPLE");
                DeleteKey("XTENSION");
                DeleteKey("EXTEND");
                cursor = GetCursor();

                //try
                //{
                //cursor.Add("XTENSION", new HeaderCard("XTENSION", value, "C# FITS: " + DateTime.Now));
                cursor.Insert("XTENSION", new HeaderCard("XTENSION", value, "C# FITS: " + DateTime.Now));

                //}
                //catch(HeaderCardException e)
                //{
                //System.Console.Error.WriteLine("Impossible exception at setXtension " + e);
                //}
            }
        }

        /// <summary>Set the BITPIX value for the header.</summary>
        /// <param name="val."> The following values are permitted by FITS conventions:
        /// <ul>
        /// <li> 8  -- signed bytes data.  Also used for tables.
        /// <li> 16 -- signed short data.
        /// <li> 32 -- signed int data.
        /// <li> -32 -- IEEE 32 bit floating point numbers.
        /// <li> -64 -- IEEE 64 bit floating point numbers.
        /// </ul>
        /// These Fits classes also support BITPIX=64 in which case data
        /// is signed 64 bit long data.</param>
        virtual public int Bitpix
        {
            set
            {
                cursor = GetCursor();
                cursor.MoveNext();

                //try
                //{
                cursor.Add("BITPIX", new HeaderCard("BITPIX", value, null));

                //}
                //catch(HeaderCardException e)
                //{
                //Console.Error.WriteLine("Impossible exception at setBitpix " + e);
                //}
            }
        }

        /// <summary>Set the value of the NAXIS keyword</summary>
        /// <param name="val">The dimensionality of the data.</param>
        virtual public int Naxes
        {
            set
            {
                cursor.Key = "BITPIX";
                if (!cursor.MoveNext())
                {
                    cursor.MovePrevious();
                }

                //try
                //{
                cursor.Add("NAXIS", new HeaderCard("NAXIS", value, "Dimensionality"));

                //}
                //catch (HeaderCardException e)
                //{
                //Console.Error.WriteLine("Impossible exception at setNaxes " + e);
                //}
            }
        }

        #endregion Properties

        #region Instance Variables

        /// <summary>The actual header data stored as a HashedList of HeaderCards.</summary>
        private HashedList cards;

        /// <summary>This cursor allows one to run through the list.</summary>
        private Cursor cursor;

        /// <summary>Offset of this Header in the FITS file</summary>
        private long fileOffset = -1;

        /// <summary>Number of cards in header last time it was read</summary>
        private int oldSize;

        /// <summary>Input descriptor last time header was read</summary>
        private ArrayDataIO input;

        #endregion Instance Variables

        #region Constructors

        private void InitBlock()
        {
            cards = new HashedList();
            cursor = cards.GetCursor(0);
        }

        /// <summary>Create an empty header</summary>
        public Header()
        {
            InitBlock();
        }

        /// <summary>Create a header and populate it from the input stream</summary>
        /// <param name="is"> The input stream where header information is expected.</param>
        public Header(ArrayDataIO is_Renamed)
        {
            InitBlock();
            Read(is_Renamed);
        }

        /// <summary>Create a header and initialize it with a vector of strings.</summary>
        /// <param name="cards">Card images to be placed in the header.</param>
        public Header(string[] newCards)
        {
            InitBlock();

            for (var i = 0; i < newCards.Length; i += 1)
            {
                var card = new HeaderCard(newCards[i]);
                if (card.Value == null)
                {
                    cards.Add(card);
                }
                else
                {
                    cards.Add(card.Key, card);
                }
            }
        }

        /// <summary>Create a header which points to the given data object.</summary>
        /// <param name="o">The data object to be described.</param>
        /// <exception cref=""> FitsException if the data was not valid for this header.</exception>
        public Header(Data o)
        {
            InitBlock();
            o.FillHeader(this);
        }

        #endregion Constructors

        /// <summary>Create the data element corresponding to the current header</summary>
        public virtual Data MakeData()
        {
            return FitsFactory.DataFactory(this);
        }

        #region Header Cards

        #region Add

        /// <summary>Adds card to the end of the HeaderCard list</summary>
        /// <param name="card">The card to be added</param>
        public virtual void AddCard(HeaderCard card)
        {
            if (card != null)
            {
                RemoveCard(card.Key);
                var c = cards.GetCursor();
                while (c.MoveNext())
                {
                    ;
                }

                c.Add(card.Key, card);
            }
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and adds the new card to the end of the HeaderCard list
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        public virtual void AddValue(string key, string val, string comment)
        {
            AddCard(new HeaderCard(key, val, comment));
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and adds the new card to the end of the HeaderCard list
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        public virtual void AddValue(string key, bool val, string comment)
        {
            AddCard(new HeaderCard(key, val, comment));
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and adds the new card to the end of the HeaderCard list
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        public virtual void AddValue(string key, int val, string comment)
        {
            AddCard(new HeaderCard(key, val, comment));
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and adds the new card to the end of the HeaderCard list
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        public virtual void AddValue(string key, float val, string comment)
        {
            AddCard(new HeaderCard(key, val, comment));
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and adds the new card to the end of the HeaderCard list
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        public virtual void AddValue(string key, long val, string comment)
        {
            AddCard(new HeaderCard(key, val, comment));
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and adds the new card to the end of the HeaderCard list
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        public virtual void AddValue(string key, double val, string comment)
        {
            AddCard(new HeaderCard(key, val, comment));
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate comment,
        /// and adds the new card to the end of the HeaderCard list
        /// </summary>
        /// <param name="comment">The comment of the new HeaderCard</param>
        public virtual void AddComment(string comment)
        {
            AddCard(new HeaderCard("COMMENT", null, comment));
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate history,
        /// and adds the new card to the end of the HeaderCard list
        /// </summary>
        /// <param name="history"></param>
        public virtual void AddHistory(string history)
        {
            AddCard(new HeaderCard("HISTORY", null, history));
        }

        #region crap

        /*

        /// <summary>Add or replace a key with the given boolean value and comment.</summary>
        /// <param name="key">The header key.</param>
        /// <param name="val">The boolean value.</param>
        /// <param name="comment">A comment to append to the card.</param>
        /// <exception cref=""> HeaderCardException If the parameters cannot build a
        /// valid FITS card.</exception>
        /// FIX THIS
        public virtual void AddValue(String key, bool val, String comment)
        {
          RemoveCard(key);
          cursor.Add(key, new HeaderCard(key, val, comment));
        }

        /// <summary>Add or replace a key with the given double value and comment.
        /// Note that float values will be promoted to doubles.</summary>
        /// <param name="key">The header key.</param>
        /// <param name="val">The double value.</param>
        /// <param name="comment">A comment to append to the card.</param>
        /// <exception cref=""> HeaderCardException If the parameters cannot build a
        /// valid FITS card.</exception>
        /// FIX THIS
        public virtual void AddValue(String key, double val, String comment)
        {
          RemoveCard(key);
          cursor.Add(key, new HeaderCard(key, val, comment));
        }

        /// <summary>Add or replace a key with the given string value and comment.</summary>
        /// <param name="key">The header key.</param>
        /// <param name="val">The string value.</param>
        /// <param name="comment">A comment to append to the card.</param>
        /// <exception cref=""> HeaderCardException If the parameters cannot build a
        /// valid FITS card.</exception>
        /// FIX THIS
        public virtual void AddValue(String key, String val, String comment)
        {
          RemoveCard(key);
          cursor.Add(key, new HeaderCard(key, val, comment));
        }

        /// <summary>Add or replace a key with the given long value and comment.
        /// Note that int's will be promoted to long's.</summary>
        /// <param name="key">The header key.</param>
        /// <param name="val">The long value.</param>
        /// <param name="comment">A comment to append to the card.</param>
        /// <exception cref=""> HeaderCardException If the parameters cannot build a
        /// valid FITS card.</exception>
        /// FIX THIS
        public virtual void AddValue(String key, long val, String comment)
        {
          RemoveCard(key);
          cursor.Add(key, new HeaderCard(key, val, comment));
        }
        */

        #endregion crap

        #endregion Add

        #region Insert

        #region Insert By Position

        /// <summary>
        /// Inserts card at the pos'th position in the HeaderCard list.
        /// If pos is out of the bounds of the list,
        /// card will be added to the end of the list.
        /// </summary>
        /// <param name="card">The HeaderCard to be inserted</param>
        /// <param name="pos">The list position into which to insert card</param>
        public virtual void InsertCard(HeaderCard card, int pos)
        {
            HeaderCard c = null;

            try
            {
                c = (HeaderCard)cards.GetElement(pos).Value;
            }
            catch (Exception)
            {
                c = null;
            }

            InsertCard(card, c);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new card in the pos'th position in the HeaderCard list.
        /// If pos is out of the bounds of the list,
        /// the new HeaderCard will be added to the end of the list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="pos">The list position into which to insert the new HeaderCard</param>
        public virtual void InsertValue(string key, string val, string comment, int pos)
        {
            InsertCard(new HeaderCard(key, val, comment), pos);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new card in the pos'th position in the HeaderCard list.
        /// If pos is out of the bounds of the list,
        /// the new HeaderCard will be added to the end of the list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="pos">The list position into which to insert the new HeaderCard</param>
        public virtual void InsertValue(string key, bool val, string comment, int pos)
        {
            InsertCard(new HeaderCard(key, val, comment), pos);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new card in the pos'th position in the HeaderCard list.
        /// If pos is out of the bounds of the list,
        /// the new HeaderCard will be added to the end of the list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="pos">The list position into which to insert the new HeaderCard</param>
        public virtual void InsertValue(string key, int val, string comment, int pos)
        {
            InsertCard(new HeaderCard(key, val, comment), pos);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new card in the pos'th position in the HeaderCard list.
        /// If pos is out of the bounds of the list,
        /// the new HeaderCard will be added to the end of the list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="pos">The list position into which to insert the new HeaderCard</param>
        public virtual void InsertValue(string key, float val, string comment, int pos)
        {
            InsertCard(new HeaderCard(key, val, comment), pos);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new card in the pos'th position in the HeaderCard list.
        /// If pos is out of the bounds of the list,
        /// the new HeaderCard will be added to the end of the list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="pos">The list position into which to insert the new HeaderCard</param>
        public virtual void InsertValue(string key, long val, string comment, int pos)
        {
            InsertCard(new HeaderCard(key, val, comment), pos);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new card in the pos'th position in the HeaderCard list.
        /// If pos is out of the bounds of the list,
        /// the new HeaderCard will be added to the end of the list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="pos">The list position into which to insert the new HeaderCard</param>
        public virtual void InsertValue(string key, double val, string comment, int pos)
        {
            InsertCard(new HeaderCard(key, val, comment), pos);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate comment,
        /// and inserts the new card in the pos'th position in the HeaderCard list.
        /// If pos is out of the bounds of the list,
        /// the new HeaderCard will be added to the end of the list.
        /// </summary>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="pos">The list position into which to insert the new HeaderCard</param>
        public virtual void InsertComment(string comment, int pos)
        {
            InsertCard(new HeaderCard("COMMENT", null, comment), pos);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate comment,
        /// and inserts the new card in the pos'th position in the HeaderCard list.
        /// If pos is out of the bounds of the list,
        /// the new HeaderCard will be added to the end of the list.
        /// </summary>
        /// <param name="history">The history of the new HeaderCard</param>
        /// <param name="pos">The list position into which to insert the new HeaderCard</param>
        public virtual void InsertHistory(string history, int pos)
        {
            InsertCard(new HeaderCard("HISTORY", null, history), pos);
        }

        #endregion Insert By Position

        #region Insert By Key

        /// <summary>
        /// Inserts card in front of the HeaderCard associated with key posKey.
        /// If there is no HeaderCard associated with posKey,
        /// card is added to the end of the HeaderCard list
        /// </summary>
        /// <param name="card">The card to be inserted</param>
        /// <param name="posKey">The key of the HeaderCard in front of which card is to be inserted</param>
        public virtual void InsertCard(HeaderCard card, string posKey)
        {
            InsertCard(card, FindCard(posKey));
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new card in front of the HeaderCard associated with key posKey.
        /// If there is no HeaderCard associated with posKey,
        /// the new card is added to the end of the HeaderCard list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="posKey">The key of the HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertValue(string key, string val, string comment, string posKey)
        {
            InsertCard(new HeaderCard(key, val, comment), posKey);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new card in front of the HeaderCard associated with key posKey.
        /// If there is no HeaderCard associated with posKey,
        /// the new card is added to the end of the HeaderCard list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="posKey">The key of the HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertValue(string key, bool val, string comment, string posKey)
        {
            InsertCard(new HeaderCard(key, val, comment), posKey);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new card in front of the HeaderCard associated with key posKey.
        /// If there is no HeaderCard associated with posKey,
        /// the new card is added to the end of the HeaderCard list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="posKey">The key of the HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertValue(string key, int val, string comment, string posKey)
        {
            InsertCard(new HeaderCard(key, val, comment), posKey);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new card in front of the HeaderCard associated with key posKey.
        /// If there is no HeaderCard associated with posKey,
        /// the new card is added to the end of the HeaderCard list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="posKey">The key of the HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertValue(string key, float val, string comment, string posKey)
        {
            InsertCard(new HeaderCard(key, val, comment), posKey);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new card in front of the HeaderCard associated with key posKey.
        /// If there is no HeaderCard associated with posKey,
        /// the new card is added to the end of the HeaderCard list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="posKey">The key of the HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertValue(string key, long val, string comment, string posKey)
        {
            InsertCard(new HeaderCard(key, val, comment), posKey);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new card in front of the HeaderCard associated with key posKey.
        /// If there is no HeaderCard associated with posKey,
        /// the new card is added to the end of the HeaderCard list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="posKey">The key of the HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertValue(string key, double val, string comment, string posKey)
        {
            InsertCard(new HeaderCard(key, val, comment), posKey);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate comment,
        /// and inserts the new card in front of the HeaderCard associated with key posKey.
        /// If there is no HeaderCard associated with posKey,
        /// the new card is added to the end of the HeaderCard list.
        /// </summary>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="posKey">The key of the HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertComment(string comment, string posKey)
        {
            InsertCard(new HeaderCard("COMMENT", null, comment), posKey);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate history,
        /// and inserts the new card in front of the HeaderCard associated with key posKey.
        /// If there is no HeaderCard associated with posKey,
        /// the new card is added to the end of the HeaderCard list.
        /// </summary>
        /// <param name="history">The history of the new HeaderCard</param>
        /// <param name="posKey">The key of the HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertHistory(string history, string posKey)
        {
            InsertCard(new HeaderCard("HISTORY", null, history), posKey);
        }

        #endregion Insert By Key

        #region Insert By HeaderCard

        /// <summary>
        /// Inserts card in front of posCard in the HeaderCard list.
        /// If posCard is null, card is added to the end of the list.
        /// </summary>
        /// <param name="card">The HeaderCard to be inserted</param>
        /// <param name="posCard">The HeaderCard in front of which card is to be inserted</param>
        public virtual void InsertCard(HeaderCard card, HeaderCard posCard)
        {
            if (card == null)
            {
                return;
            }

            if (posCard == null)
            {
                AddCard(card);
                return;
            }

            var c = cards.GetCursor();
            for (c.MoveNext();
              c.Current != null && !((DictionaryEntry)c.Current).Value.Equals(posCard) &&
              c.MoveNext();)
            {
                ;
            }

            if (c.Current == null)
            {
                AddCard(card);
            }
            else
            {
                c.Insert(card.Key, card);
            }
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new HeaderCard in front of posCard in the HeaderCard list.
        /// If posCard is null, the new HeaderCard is added to the end of the list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="posCard">The HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertValue(string key, string val, string comment, HeaderCard posCard)
        {
            InsertCard(new HeaderCard(key, val, comment), posCard);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new HeaderCard in front of posCard in the HeaderCard list.
        /// If posCard is null, the new HeaderCard is added to the end of the list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="posCard">The HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertValue(string key, bool val, string comment, HeaderCard posCard)
        {
            InsertCard(new HeaderCard(key, val, comment), posCard);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new HeaderCard in front of posCard in the HeaderCard list.
        /// If posCard is null, the new HeaderCard is added to the end of the list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="posCard">The HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertValue(string key, int val, string comment, HeaderCard posCard)
        {
            InsertCard(new HeaderCard(key, val, comment), posCard);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new HeaderCard in front of posCard in the HeaderCard list.
        /// If posCard is null, the new HeaderCard is added to the end of the list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="posCard">The HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertValue(string key, float val, string comment, HeaderCard posCard)
        {
            InsertCard(new HeaderCard(key, val, comment), posCard);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new HeaderCard in front of posCard in the HeaderCard list.
        /// If posCard is null, the new HeaderCard is added to the end of the list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="posCard">The HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertValue(string key, long val, string comment, HeaderCard posCard)
        {
            InsertCard(new HeaderCard(key, val, comment), posCard);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate key, val, and comment,
        /// and inserts the new HeaderCard in front of posCard in the HeaderCard list.
        /// If posCard is null, the new HeaderCard is added to the end of the list.
        /// </summary>
        /// <param name="key">The key of the new HeaderCard</param>
        /// <param name="val">The value of the new HeaderCard</param>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="posCard">The HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertValue(string key, double val, string comment, HeaderCard posCard)
        {
            InsertCard(new HeaderCard(key, val, comment), posCard);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate comment,
        /// and inserts the new HeaderCard in front of posCard in the HeaderCard list.
        /// If posCard is null, the new HeaderCard is added to the end of the list.
        /// </summary>
        /// <param name="comment">The comment of the new HeaderCard</param>
        /// <param name="posCard">The HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertComment(string comment, HeaderCard posCard)
        {
            InsertCard(new HeaderCard("COMMENT", null, comment), posCard);
        }

        /// <summary>
        /// Creates a new HeaderCard to accommodate comment,
        /// and inserts the new HeaderCard in front of posCard in the HeaderCard list.
        /// If posCard is null, the new HeaderCard is added to the end of the list.
        /// </summary>
        /// <param name="history">The history of the new HeaderCard</param>
        /// <param name="posCard">The HeaderCard in front of which the new HeaderCard is to be inserted</param>
        public virtual void InsertHistory(string history, HeaderCard posCard)
        {
            InsertCard(new HeaderCard("HISTORY", null, history), posCard);
        }

        #endregion Insert By HeaderCard

        #endregion Insert

        #region Remove

        /// <summary>
        /// Removes the HeaderCard at position index in the HeaderCard list.
        /// Remember that each call to this method shifts the index of subsequent cards in the list.
        /// </summary>
        /// <param name="index">The position in the HeaderCard list of the HeaderCard to be removed</param>
        public virtual void RemoveCard(int index)
        {
            RemoveCard((HeaderCard)cards.GetElement(index).Value);
        }

        /// <summary>
        /// Removes the HeaderCard associated with key
        /// </summary>
        /// <param name="key">The key of the HeaderCard to be removed</param>
        public virtual void RemoveCard(string key)
        {
            cards.Remove(key);
        }

        /// <summary>
        /// Removes card from the HeaderCard list
        /// </summary>
        /// <param name="card">The card to be removed</param>
        public virtual void RemoveCard(HeaderCard card)
        {
            if (card == null)
            {
                return;
            }

            var c = cards.GetCursor();
            for (var done = false; !done && c.MoveNext();)
            {
                var hc = (HeaderCard)((DictionaryEntry)c.Current).Value;
                if (card.Equals(hc))
                {
                    c.Remove();
                    done = true;
                }
                c.MoveNext();
            }
        }

        #endregion Remove

        /// <summary>Add a line to the header using the COMMENT style, i.e., no '='
        /// in column 9.</summary>
        /// <param name="header">The comment style header.</param>
        /// <param name="value"> A string to follow the header.</param>
        /// <exception cref=""> HeaderCardException If the parameters cannot build a
        /// valid FITS card.</exception>
        public virtual void InsertCommentStyle(string header, string value_Renamed)
        {
            // Should just truncate strings, so we should never get
            // an exception...

            //try
            //{
            cursor.Add(new HeaderCard(header, null, value_Renamed));

            //}
            //catch (HeaderCardException)
            //{
            //Console.Error.WriteLine("Impossible Exception for comment style:" + header + ":" + value_Renamed);
            //}
        }

        /*

        /// <summary>Add a COMMENT line.</summary>
        /// <param name="value">The comment.</param>
        /// <exception cref="">HeaderCardException If the parameter is not a valid FITS comment.</exception>
        public virtual void InsertComment(String value_Renamed)
        {
          InsertCommentStyle("COMMENT", value_Renamed);
        }

        /// <summary>Add a HISTORY line.</summary>
        /// <param name="value">The history record.</param>
        /// <exception cref="">HeaderCardException If the parameter is not a valid FITS comment.</exception>
        public virtual void InsertHistory(String value_Renamed)
        {
          InsertCommentStyle("HISTORY", value_Renamed);
        }
    */

        /// <summary>Delete the card associated with the given key.
        /// Nothing occurs if the key is not found.</summary>
        /// <param name="key">The header key.</param>
        /// FIX THIS
        /// either kill this or RemoveCard
        public virtual void DeleteKey(string key)
        {
            cards.Remove(key);
        }

        /// <summary>Tests if the specified keyword is present in this table.</summary>
        /// <param name="key">the keyword to be found.</param>
        /// <returns> <CODE>true<CODE> if the specified keyword is present in this table;
        /// <CODE>false<CODE> otherwise.</returns>
        public bool ContainsKey(string key)
        {
            return cards.ContainsKey(key);
        }

        /// <summary>Add a card image to the header.</summary>
        /// <param name="fcard">The card to be added.</param>
        /// FIX THIS
        protected internal virtual void AddLine(HeaderCard fcard)
        {
            if (fcard != null)
            {
                if (fcard.KeyValuePair)
                {
                    cursor.Add(fcard.Key, fcard);
                }
                else
                {
                    cursor.Add(fcard);
                }
            }
        }

        /// <summary>Add a card image to the header.</summary>
        /// <param name="card">The card to be added.</param>
        /// <exception cref=""> HeaderCardException If the card is not valid.</exception>
        /// FIX THIS
        protected internal virtual void AddLine(string card)
        {
            AddLine(new HeaderCard(card));
        }

        /// <summary>Get a cursor over the header cards</summary>
        /// KILL THIS METHOD
        public Cursor GetCursor()
        {
            //return cards.GetCursor(0);
            return cards.GetCursor();
        }

        /// <summary>Find the card associated with a given key.
        /// If found this sets the mark to the card, otherwise it
        /// unsets the mark.</summary>
        /// <param name="key">The header key.</param>
        /// <returns> <CODE>null</CODE> if the keyword could not be found;
        /// return the HeaderCard object otherwise.</returns>
        /// FIX THIS
        public virtual HeaderCard FindCard(string key)
        {
            var card = (HeaderCard)cards[key];
            if (card != null)
            {
                cursor.Key = key;
            }
            return card;
        }

        /*

        /// <summary>Find the card associated with a given key.</summary>
        /// <param name="key">The header key.</param>
        /// <returns> <CODE>null</CODE> if the keyword could not be found;
        /// return the card image otherwise.</returns>
        /// KILL THIS METHOD
        public virtual String FindKey(String key)
        {
          HeaderCard card = FindCard(key);
          if(card == null)
          {
            return null;
          }
          else
          {
            return card.ToString();
          }
        }
        */

        /// <summary>Replace the key with a new key.  Typically this is used
        /// when deleting or inserting columns so that TFORMx -> TFORMx-1</summary>
        /// <param name="oldKey">The old header keyword.</param>
        /// <param name="newKey">the new header keyword.</param>
        /// <returns> <CODE>true</CODE> if the card was replaced.</returns>
        /// <exception cref=""> HeaderCardException If <CODE>newKey</CODE> is not a
        /// valid FITS keyword.</exception>
        internal virtual bool ReplaceKey(string oldKey, string newKey)
        {
            var oldCard = FindCard(oldKey);
            if (oldCard == null)
            {
                return false;
            }
            if (!cards.ReplaceKey(oldKey, newKey))
            {
                throw new HeaderCardException("Duplicate key in replace");
            }

            oldCard.Key = newKey;

            return true;
        }

        public IEnumerator GetEnumerator()
        {
            return GetCursor();
        }

        /// <summary>Print the header to a given stream.</summary>
        /// <param name="ps">the stream to which the card images are dumped.</param>
        public void DumpHeader(TextWriter ps)
        {
            cursor = GetCursor();
            while (cursor.MoveNext())
            {
                ps.WriteLine(((DictionaryEntry)cursor.Current).Value);
            }
        }

        /// <summary>** Deprecated methods ******</summary>

        /// <summary>Get the n'th card image in the header</summary>
        /// <returns>the card image; return <CODE>null</CODE> if the n'th card does not exist.</returns>
        /// <deprecated> A cursor should be used for sequential access to the header.</deprecated>
        /// KILL THIS METHOD
        public string GetCard(int n)
        {
            if (n >= 0 && n < cards.Count)
            {
                cursor = cards.GetCursor(n);
                cursor.MoveNext();
                var c = (HeaderCard)((DictionaryEntry)cursor.Current).Value;
                return c.ToString();
            }
            return null;
        }

        /// <summary>Find the end of a set of keywords describing a column or axis
        /// (or anything else terminated by an index.  This routine leaves
        /// the header ready to add keywords after any existing keywords
        /// with the index specified.  The user should specify a
        /// prefix to a keyword that is guaranteed to be present.</summary>
        /// MAN WOULD IT BE GREAT TO GET RID OF THIS METHOD
        internal virtual Cursor PositionAfterIndex(string prefix, int col)
        {
            var colnum = "" + col;

            cursor.Key = prefix + colnum;

            string key;
            while (cursor.MoveNext())
            {
                //key = cursor.Current.Key.Trim();
                key = ((string)cursor.Key).Trim();
                if (key == null || key.Length <= colnum.Length || !key.Substring(key.Length - colnum.Length).Equals(colnum))
                {
                    break;
                }
            }
            if (cursor.MoveNext())
            {
                cursor.MovePrevious(); // Gone one too far, so skip back an element.
            }

            return cursor;
        }

        /// <summary>Get the next card in the Header using the current cursor</summary>
        /// KILL THIS METHOD
        public HeaderCard NextCard()
        {
            if (cursor == null)
            {
                return null;
            }
            if (cursor.MoveNext())
            {
                return (HeaderCard)((DictionaryEntry)cursor.Current).Value;
            }
            else
            {
                return null;
            }
        }

        #endregion Header Cards

        /// <summary>Calculate the unpadded size of the data segment from the header information.</summary>
        /// <returns> the unpadded data segment size.</returns>
        internal virtual int TrueDataSize()
        {
            if (!ValidHeader)
            {
                return 0;
            }

            var naxis = GetIntValue("NAXIS", 0);
            var bitpix = GetIntValue("BITPIX");

            var axes = new int[naxis];

            for (var axis = 1; axis <= naxis; axis += 1)
            {
                axes[axis - 1] = GetIntValue("NAXIS" + axis, 0);
            }

            var isGroup = GetBooleanValue("GROUPS", false);

            var pcount = GetIntValue("PCOUNT", 0);
            var gcount = GetIntValue("GCOUNT", 1);

            var startAxis = 0;

            if (isGroup && naxis > 1 && axes[0] == 0)
            {
                startAxis = 1;
            }

            var size = 1;
            for (var i = startAxis; i < naxis; i += 1)
            {
                size *= axes[i];
            }

            size += pcount;
            size *= gcount;

            // Now multiply by the number of bits per pixel and
            // convert to bytes.
            size *= Math.Abs(GetIntValue("BITPIX", 0)) / 8;

            return size;
        }

        /// <summary>Return the size of the header data including padding.</summary>
        /// <returns> the header size including any needed padding.</returns>
        internal virtual int HeaderSize()
        {
            if (!ValidHeader)
            {
                return 0;
            }

            return FitsUtil.AddPadding(cards.Count * 80);
        }

        #region GetXXXValue Methods

        /// <summary>Get the value associated with the key as an int.</summary>
        /// <param name="key">The header key.</param>
        /// <param name="dft">The value to be returned if the key is not found.</param>
        public virtual int GetIntValue(string key, int dft)
        {
            return (int)GetLongValue(key, (long)dft);
        }

        /// <summary>Get the <CODE>int</CODE> value associated with the given key.</summary>
        /// <param name="key">The header key.</param>
        /// <returns> The associated value or 0 if not found.</returns>
        public virtual int GetIntValue(string key)
        {
            return (int)GetLongValue(key);
        }

        /// <summary>Get the <CODE>long</CODE> value associated with the given key.</summary>
        /// <param name="key">The header key.</param>
        /// <returns> The associated value or 0 if not found.</returns>
        public virtual long GetLongValue(string key)
        {
            return GetLongValue(key, 0L);
        }

        /// <summary>Get the <CODE>long</CODE> value associated with the given key.</summary>
        /// <param name="key">The header key.</param>
        /// <param name="dft">The default value to be returned if the key cannot be found.</param>
        /// <returns> the associated value.</returns>
        public virtual long GetLongValue(string key, long dft)
        {
            var fcard = FindCard(key);
            if (fcard == null)
            {
                return dft;
            }

            try
            {
                var v = fcard.Value;
                if (v != null)
                {
                    return Int64.Parse(v);
                }
            }
            catch (FormatException)
            {
            }

            return dft;
        }

        /// <summary>Get the <CODE>float</CODE> value associated with the given key.</summary>
        /// <param name="key">The header key.</param>
        /// <param name="dft">The value to be returned if the key is not found.</param>
        public virtual float GetFloatValue(string key, float dft)
        {
            return (float)GetDoubleValue(key, dft);
        }

        /// <summary>Get the <CODE>float</CODE> value associated with the given key.</summary>
        /// <param name="key">The header key.</param>
        /// <returns> The associated value or 0.0 if not found.</returns>
        public virtual float GetFloatValue(string key)
        {
            return (float)GetDoubleValue(key);
        }

        /// <summary>Get the <CODE>double</CODE> value associated with the given key.</summary>
        /// <param name="key">The header key.</param>
        /// <returns>The associated value or 0.0 if not found.</returns>
        public virtual double GetDoubleValue(string key)
        {
            return GetDoubleValue(key, 0.0);
        }

        /// <summary>Get the <CODE>double</CODE> value associated with the given key.</summary>
        /// <param name="key">The header key.</param>
        /// <param name="dft">The default value to return if the key cannot be found.</param>
        /// <returns> the associated value.</returns>
        public virtual double GetDoubleValue(string key, double dft)
        {
            var fcard = FindCard(key);
            if (fcard == null)
            {
                return dft;
            }

            try
            {
                var v = fcard.Value;
                if (v != null)
                {
                    return Double.Parse(v);
                }
            }
            catch (FormatException)
            {
            }

            return dft;
        }

        /// <summary>Get the <CODE>boolean</CODE> value associated with the given key.</summary>
        /// <param name="The">header key.</param>
        /// <returns> The value found, or false if not found or if the keyword is not a logical keyword.</returns>
        public virtual bool GetBooleanValue(string key)
        {
            return GetBooleanValue(key, false);
        }

        /// <summary>Get the <CODE>boolean</CODE> value associated with the given key.</summary>
        /// <param name="key">The header key.</param>
        /// <param name="dft">The value to be returned if the key cannot be found
        /// or if the parameter does not seem to be a boolean.</param>
        /// <returns> the associated value.</returns>
        public virtual bool GetBooleanValue(string key, bool dft)
        {
            var fcard = FindCard(key);
            if (fcard == null)
            {
                return dft;
            }

            var val = fcard.Value;
            if (val == null)
            {
                return dft;
            }

            if (val.Equals("T"))
            {
                return true;
            }
            else if (val.Equals("F"))
            {
                return false;
            }
            else
            {
                return dft;
            }
        }

        /// <summary>Get the <CODE>String</CODE> value associated with the given key.</summary>
        /// <param name="key">The header key.</param>
        /// <returns> The associated value or null if not found or if the value is not a string.</returns>
        public virtual string GetStringValue(string key)
        {
            var fcard = FindCard(key);
            if (fcard == null || !fcard.IsStringValue)
            {
                return null;
            }

            return fcard.Value;
        }

        #endregion GetXXXValue Methods

        #region Read/Write

        /// <summary>Create a header by reading the information from the input stream.</summary>
        /// <param name="dis">The input stream to read the data from.</param>
        /// <returns> <CODE>null</CODE> if there was a problem with the header;
        /// otherwise return the header read from the input stream.</returns>
        public static Header ReadHeader(ArrayDataIO dis)
        {
            var myHeader = new Header();
            try
            {
                myHeader.Read(dis);
            }
            catch (EndOfStreamException)
            {
                // An EOF exception is thrown only if the EOF was detected
                // when reading the first card.  In this case we want
                // to return a null.
                return null;
            }

            return myHeader;
        }

        /// <summary>Read a stream for header data.</summary>
        /// <param name="dis">The input stream to read the data from.</param>
        /// <returns> <CODE>null</CODE> if there was a problem with the header;
        /// otherwise return the header read from the input stream.</returns>
        public virtual void Read(ArrayDataIO dis)
        {
            if (dis is RandomAccess)
            {
                fileOffset = FitsUtil.FindOffset(dis);
            }
            else
            {
                fileOffset = -1;
            }

            var buffer = new byte[80];
            var firstCard = true;
            var count = 0;
            var notEnd = true;

            while (notEnd)
            {
                var need = 80;

                //				try
                //				{
                for (var len = 1; need > 0 && len > 0;)
                {
                    len = dis.Read(buffer, 80 - need, need);
                    count += 1;
                    if (firstCard && len == 0 && need == 80)
                    {
                        throw new EndOfStreamException();
                    }
                    need -= len;
                }

                //				}
                //				catch(EndOfStreamException e)
                //				{
                //					// Rethrow the EOF if we are at the beginning of the header,
                //					// otherwise we have a FITS error.
                //					if(firstCard && need == 80)
                //					{
                //						throw e;
                //					}
                //					throw new TruncatedFileException(e.Message);
                //				}

                var cbuf = new string(SupportClass.ToCharArray(buffer));
                var fcard = new HeaderCard(cbuf);

                if (firstCard)
                {
                    var key = fcard.Key;

                    //Console.Out.WriteLine("key = '" + key + "'");
                    if (key == null || (!key.Equals("SIMPLE") && !key.Equals("XTENSION")))
                    {
                        throw new IOException("Not FITS format at " + fileOffset + ":" + cbuf);
                    }

                    firstCard = false;
                }

                var key2 = fcard.Key;
                if (key2 != null && cards.ContainsKey(key2))
                {
                    Console.Error.WriteLine("Warning: multiple occurrences of key:" + key2);
                }

                // save card
                AddLine(fcard);
                if (cbuf.Substring(0, (8) - (0)).Equals("END     "))
                {
                    notEnd = false;
                }
            }

            if (fileOffset >= 0)
            {
                oldSize = cards.Count;
                input = dis;
            }

            // Read to the end of the current FITS block.
            try
            {
                if (dis.CanSeek)
                {
                    dis.Seek(FitsUtil.Padding(count * 80));
                }
                else
                {
                    var pad = FitsUtil.Padding(count * 80);
                    for (var len = dis.Read(buffer, 0, Math.Min(pad, buffer.Length));
                        pad > 0 && len != -1;)
                    {
                        pad -= len;
                        len = dis.Read(buffer, 0, Math.Min(pad, buffer.Length));
                    }
                }
            }
            catch (IOException e)
            {
                throw new TruncatedFileException(e.Message);
            }
        }

        /// <summary>Write the current header (including any needed padding) to the
        /// output stream.</summary>
        /// <param name="dos">The output stream to which the data is to be written.</param>
        /// <exception cref="">FitsException if the header could not be written.</exception>
        public void Write(ArrayDataIO dos)
        {
            fileOffset = FitsUtil.FindOffset(dos);

            CheckBeginning();
            CheckEnd();

            if (cards.Count <= 0)
            {
                return;
            }

            //Cursor cursor = cards.GetCursor(0);
            var cursor = cards.GetCursor();

            try
            {
                while (cursor.MoveNext())
                {
                    var card = (HeaderCard)((DictionaryEntry)cursor.Current).Value;
                    var b = SupportClass.ToByteArray(card.ToString());
                    dos.Write(b);
                }

                var padding = new byte[FitsUtil.Padding(NumberOfCards * 80)];
                for (var i = 0; i < padding.Length; i += 1)
                {
                    padding[i] = (byte)' ';// SupportClass.Identity(' ');
                }
                dos.Write(padding);
            }
            catch (IOException e)
            {
                throw new FitsException("IO Error writing header: " + e);
            }
            try
            {
                dos.Flush();
            }
            catch (IOException)
            {
            }
        }

        /// <summary>Rewrite the header.</summary>
        public virtual void Rewrite()
        {
            var dos = (ArrayDataIO)input;

            if (Rewriteable)
            {
                //FitsUtil.Reposition(dos, fileOffset);
                dos.Seek(fileOffset, SeekOrigin.Begin);
                Write(dos);
                dos.Flush();
            }
            else
            {
                throw new FitsException("Invalid attempt to rewrite Header.");
            }
        }

        #endregion Read/Write

        /// <summary>Create a header for a null image.</summary>
        internal virtual void NullImage()
        {
            cursor = GetCursor();
            try
            {
                AddValue("SIMPLE", true, "Null Image Header");
                AddValue("BITPIX", 8, null);
                AddValue("NAXIS", 0, null);
                AddValue("EXTEND", true, "Extensions are permitted");
            }
            catch (HeaderCardException)
            {
            }
        }

        /// <summary>Set the dimension for a given axis.</summary>
        /// <param name="axis">The axis being set.</param>
        /// <param name="dim"> The dimension</param>
        public virtual void SetNaxis(int axis, int dim)
        {
            if (axis <= 0)
            {
                return;
            }
            if (axis == 1)
            {
                cursor.Key = "NAXIS";
            }
            else if (axis > 1)
            {
                cursor.Key = "NAXIS" + (axis - 1);
            }

            //if(!cursor.MoveNext())
            //{
            //				cursor.MovePrevious();
            //}
            //try
            //{
            cursor.Add("NAXIS" + axis, new HeaderCard("NAXIS" + axis, dim, null));

            //}
            //catch (HeaderCardException e)
            //{
            //Console.Error.WriteLine("Impossible exception at setNaxis " + e);
            //}
        }

        #region Header Check Methods

        /// <summary>Ensure that the header begins with
        /// a valid set of keywords.  Note that we
        /// do not check the values of these keywords.</summary>
        internal void CheckBeginning()
        {
            cursor = GetCursor();
            cursor.MoveNext();
            if (cursor.Current == null)
            {
                throw new FitsException("Empty Header");
            }
            var card = (HeaderCard)((DictionaryEntry)cursor.Current).Value;
            var key = card.Key;
            if (!key.Equals("SIMPLE") && !key.Equals("XTENSION"))
            {
                throw new FitsException("No SIMPLE or XTENSION at beginning of Header");
            }
            var isTable = false;
            var isExtension = false;
            if (key.Equals("XTENSION"))
            {
                var value_Renamed = card.Value;
                if (value_Renamed == null)
                {
                    throw new FitsException("Empty XTENSION keyword");
                }

                isExtension = true;

                if (value_Renamed.Equals("BINTABLE") || value_Renamed.Equals("A3DTABLE") || value_Renamed.Equals("TABLE"))
                {
                    isTable = true;
                }
            }

            CardCheck("BITPIX");
            CardCheck("NAXIS");

            var nax = GetIntValue("NAXIS");

            //			cursor.MoveNext();

            for (var i = 1; i <= nax; i += 1)
            {
                CardCheck("NAXIS" + i);
            }

            if (isExtension)
            {
                CardCheck("PCOUNT");
                CardCheck("GCOUNT");
                if (isTable)
                {
                    CardCheck("TFIELDS");
                }
            }
        }

        /// <summary>Check if the given key is the next one available in the header.</summary>
        private void CardCheck(string key)
        {
            if (!cursor.MoveNext())
            {
                throw new FitsException("Header terminates before " + key);
            }
            var card = (HeaderCard)((DictionaryEntry)cursor.Current).Value;
            if (!card.Key.Equals(key))
            {
                throw new FitsException("Key " + key + " not found where expected." + "Found " + card.Key);
            }
        }

        /// <summary>Ensure that the header has exactly one END keyword in
        /// the appropriate location.</summary>
        internal void CheckEnd()
        {
            // Ensure we have an END card only at the end of the header.
            cursor = GetCursor();
            HeaderCard card;

            while (cursor.MoveNext())
            {
                card = (HeaderCard)((DictionaryEntry)cursor.Current).Value;
                if (!card.KeyValuePair && card.Key.Equals("END"))
                {
                    cursor.Remove();
                }
            }
            try
            {
                cursor.Add(new HeaderCard("END", null, null));
            }
            catch (HeaderCardException)
            {
            }
        }

        #endregion Header Check Methods
    }
}