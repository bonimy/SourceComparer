namespace nom.tam.fits
{
    using System;
    using System.IO;
    using nom.tam.util;

    /// <summary>An ASCII table.</summary>

    public class AsciiTable : Data, TableData
    {
        #region Properties

        /// <summary>Get the ASCII table information.
        /// This will actually do the read if it had previously been deferred</summary>
        override public object DataArray
        {
            get
            {
                if (data == null)
                {
                    data = new object[nFields];

                    for (var i = 0; i < nFields; i += 1)
                    {
                        data[i] = ArrayFuncs.NewInstance(types[i], nRows);
                    }

                    if (buffer == null)
                    {
                        var newOffset = FitsUtil.FindOffset(currInput);
                        try
                        {
                            GetBuffer(nRows * rowLen, fileOffset);
                        }
                        catch (IOException e)
                        {
                            throw new FitsException("Error in deferred read -- file closed prematurely?:" + e);
                        }

                        //FitsUtil.Reposition(currInput, newOffset);
                        currInput.Seek(newOffset, SeekOrigin.Begin);
                    }

                    bp.Offset = 0;

                    int rowOffset;
                    for (var i = 0; i < nRows; i += 1)
                    {
                        rowOffset = rowLen * i;
                        for (var j = 0; j < nFields; j += 1)
                        {
                            if (!ExtractElement(rowOffset + offsets[j], lengths[j], data, j, i, nulls[j]))
                            {
                                if (isNull_Renamed_Field == null)
                                {
                                    isNull_Renamed_Field = new bool[nRows * nFields];
                                }

                                isNull_Renamed_Field[j + i * nFields] = true;
                            }
                        }
                    }
                }

                return data;
            }
        }

        /// <summary>Return the size of the data section</summary>
        override internal int TrueSize
        {
            get
            {
                return nRows * rowLen;
            }
        }

        /// <summary>Get the number of rows in the table</summary>
        virtual public int NRows
        {
            get
            {
                return nRows;
            }
        }

        /// <summary>Get the number of columns in the table</summary>
        virtual public int NCols
        {
            get
            {
                return nFields;
            }
        }

        /// <summary>Get the number of bytes in a row</summary>
        virtual public int RowLen
        {
            get
            {
                return rowLen;
            }
        }

        #endregion Properties

        #region Instance Variables

        /// <summary>The number of rows in the table</summary>
        private int nRows;

        /// <summary>The number of fields in the table</summary>
        private int nFields;

        /// <summary>The number of bytes in a row</summary>
        private int rowLen;

        /// <summary>The null string for the field</summary>
        private string[] nulls;

        /// <summary>The type of data in the field</summary>
        private Type[] types;

        /// <summary>The offset from the beginning of the row at which the field starts</summary>
        private int[] offsets;

        /// <summary>The number of bytes in the field</summary>
        private int[] lengths;

        /// <summary>The byte buffer used to read/write the ASCII table</summary>
        //		private sbyte[] buffer;
        private byte[] buffer;

        /// <summary>Markers indicating fields that are null</summary>
        private bool[] isNull_Renamed_Field;

        /// <summary>An array of arrays giving the data in the table in binary numbers</summary>
        private object[] data;

        /// <summary>The parser used to convert from buffer to data.</summary>
        internal ByteParser bp;

        /// <summary>The actual stream used to input data</summary>
        internal ArrayDataIO currInput;

        #endregion Instance Variables

        /// <summary>Create an ASCII table given a header</summary>
        public AsciiTable(Header hdr)
        {
            nRows = hdr.GetIntValue("NAXIS2");
            nFields = hdr.GetIntValue("TFIELDS");
            rowLen = hdr.GetIntValue("NAXIS1");

            types = new Type[nFields];
            offsets = new int[nFields];
            lengths = new int[nFields];
            nulls = new string[nFields];

            for (var i = 0; i < nFields; i += 1)
            {
                offsets[i] = hdr.GetIntValue("TBCOL" + (i + 1)) - 1;
                var s = hdr.GetStringValue("TFORM" + (i + 1));
                if (offsets[i] < 0 || s == null)
                {
                    throw new FitsException("Invalid Specification for column:" + (i + 1));
                }
                s = s.Trim();
                var c = s[0];
                s = s.Substring(1);
                if (s.IndexOf('.') > 0)
                {
                    s = s.Substring(0, (s.IndexOf('.')) - (0));
                }
                lengths[i] = Int32.Parse(s);

                switch (c)
                {
                case 'A':
                types[i] = typeof(string);
                break;

                case 'I':
                if (lengths[i] > 10)
                {
                    types[i] = typeof(long);
                }
                else
                {
                    types[i] = typeof(int);
                }
                break;

                case 'F':
                case 'E':
                types[i] = typeof(float);
                break;

                case 'D':
                types[i] = typeof(double);
                break;
                }

                nulls[i] = hdr.GetStringValue("TNULL" + (i + 1));
                if (nulls[i] != null)
                {
                    nulls[i] = nulls[i].Trim();
                }
            }
        }

        /// <summary>Create an empty ASCII table</summary>
        public AsciiTable()
        {
            data = new object[0];
            buffer = null;
            nFields = 0;
            nRows = 0;
            rowLen = 0;
            types = new Type[0];
            lengths = new int[0];
            offsets = new int[0];
            nulls = new string[0];
        }

        /// <summary>Read in an ASCII table.  Reading is deferred if
        /// we are reading from a random access device</summary>
        public override void Read(ArrayDataIO str)
        {
            SetFileOffset(str);
            currInput = str;

            //if(str is RandomAccess)
            if (str.CanSeek)
            {
                try
                {
                    //BinaryReader temp_BinaryReader;
                    long temp_Int64;

                    //temp_BinaryReader = str;
                    temp_Int64 = str.Position; //temp_BinaryReader.BaseStream.Position;
                    temp_Int64 = str.Seek(nRows * rowLen) - temp_Int64;//temp_BinaryReader.BaseStream.Seek(nRows * rowLen, SeekOrigin.Current) - temp_Int64;
                    var generatedAux = (int)temp_Int64;
                }
                catch (IOException e)
                {
                    throw new FitsException("Error skipping data: " + e);
                }
            }
            else
            {
                try
                {
                    GetBuffer(rowLen * nRows, 0);
                }
                catch (IOException e)
                {
                    throw new FitsException("Error reading ASCII table:" + e);
                }
            }

            try
            {
                //BinaryReader temp_BinaryReader2;
                long temp_Int65;

                //temp_BinaryReader2 = str;
                temp_Int65 = str.Position;//temp_BinaryReader2.BaseStream.Position;
                temp_Int65 = str.Seek(FitsUtil.Padding(nRows * rowLen)) - temp_Int65;

                //temp_BinaryReader2.BaseStream.Seek(FitsUtil.padding(nRows * rowLen), SeekOrigin.Current) - temp_Int65;
                var generatedAux2 = (int)temp_Int65;
            }
            catch (IOException e)
            {
                throw new FitsException("Error skipping padding:" + e);
            }
        }

        /// <summary>Read some data into the buffer.</summary>
        private void GetBuffer(int size, long offset)
        {
            if (currInput == null)
            {
                throw new IOException("No stream open to read");
            }

            buffer = new byte[size];
            if (offset != 0)
            {
                //FitsUtil.Reposition(currInput, offset);
                currInput.Seek(offset, SeekOrigin.Begin);
            }
            currInput.Read(buffer);//SupportClass.ReadInput(currInput.BaseStream, ref buffer, 0, buffer.Length);
            bp = new ByteParser(buffer);
        }

        /// <summary>Move an element from the buffer into a data array.</summary>
        /// <param name="offset"> The offset within buffer at which the element starts.</param>
        /// <param name="length"> The number of bytes in the buffer for the element.</param>
        /// <param name="array">  An array of objects, each of which is a simple array.</param>
        /// <param name="col">    Which element of array is to be modified?</param>
        /// <param name="row">    Which index into that element is to be modified?</param>
        /// <param name="nullFld">What string signifies a null element?</param>
        private bool ExtractElement(int offset, int length, Array array, int col, int row, string nullFld)
        {
            bp.Offset = offset;

            if (nullFld != null)
            {
                var s = bp.getString(length);
                if (s.Trim().Equals(nullFld))
                {
                    return false;
                }
                bp.skip(-length);
            }
            try
            {
                var el = array.GetValue(col);
                if (el is string[])
                {
                    ((string[])el)[row] = bp.getString(length);
                }
                else if (el is int[])
                {
                    ((int[])el)[row] = bp.getInt(length);
                }
                else if (el is float[])
                {
                    ((float[])el)[row] = bp.getFloat(length);
                }
                else if (el is double[])
                {
                    ((double[])el)[row] = bp.getDouble(length);
                }
                else if (el is long[])
                {
                    ((long[])el)[row] = bp.getLong(length);
                }
                else
                {
                    throw new FitsException("Invalid type for ASCII table conversion:" + el);
                }
            }
            catch (Exception e)
            {
                throw new FitsException("Error parsing data at row,col:" + row + "," + col + "  " + e);
            }
            return true;
        }

        /// <summary>Get a column of data</summary>
        public virtual object GetColumn(int col)
        {
            if (data == null)
            {
                var generatedAux = DataArray;
            }
            return data[col];
        }

        /// <summary>Get a row.  If the data has not yet been read just read this row.</summary>
        public virtual Array GetRow(int row)
        {
            if (data != null)
            {
                return SingleRow(row);
            }
            else
            {
                return ParseSingleRow(row);
            }
        }

        /// <summary>Get a single element as a one-d array.
        /// We return String's as arrays for consistency though
        /// they could be returned as a scalar.</summary>
        public virtual object GetElement(int row, int col)
        {
            if (data != null)
            {
                return SingleElement(row, col);
            }
            else
            {
                return ParseSingleElement(row, col);
            }
        }

        /// <summary>Extract a single row from a table.  This returns
        /// an array of Objects each of which is an array of length 1.</summary>
        private Array SingleRow(int row)
        {
            Array res = new Array[nFields];
            for (var i = 0; i < nFields; i += 1)
            {
                if (isNull_Renamed_Field == null || !isNull_Renamed_Field[row * nFields + i])
                {
                    res.SetValue(ArrayFuncs.NewInstance(types[i], 1), i);
                    Array.Copy((Array)data[i], row, (Array)res.GetValue(i), 0, 1);
                }
            }
            return res;
        }

        /// <summary>Extract a single element from a table.  This returns
        /// an array of length 1.</summary>
        private Array SingleElement(int row, int col)
        {
            Array res = null;
            if (isNull_Renamed_Field == null || !isNull_Renamed_Field[row * nFields + col])
            {
                res = ArrayFuncs.NewInstance(types[col], 1);
                Array.Copy((Array)data[col], row, res, 0, 1);
            }
            return res;
        }

        /// <summary>Read a single row from the table.  This returns a set of arrays of dimension 1.</summary>
        private Array ParseSingleRow(int row)
        {
            var offset = row * rowLen;
            Array res = new Array[nFields];

            try
            {
                GetBuffer(rowLen, fileOffset + row * rowLen);
            }
            catch (IOException)
            {
                throw new FitsException("Unable to read row");
            }

            for (var i = 0; i < nFields; i += 1)
            {
                res.SetValue(ArrayFuncs.NewInstance(types[i], 1), i);
                if (!ExtractElement(offsets[i], lengths[i], res, i, 0, nulls[i]))
                {
                    res.SetValue(null, i);
                }
            }

            // Invalidate buffer for future use.
            buffer = null;
            return res;
        }

        /// <summary>Read a single element from the table.  This returns an array of dimension 1.</summary>
        private Array ParseSingleElement(int row, int col)
        {
            Array res = new Array[1];
            try
            {
                GetBuffer(lengths[col], fileOffset + row * rowLen + offsets[col]);
            }
            catch (IOException)
            {
                buffer = null;
                throw new FitsException("Unable to read element");
            }
            res.SetValue(ArrayFuncs.NewInstance(types[col], 1), 0);

            if (ExtractElement(0, lengths[col], res, 0, 0, nulls[col]))
            {
                buffer = null;
                return (Array)res.GetValue(0);
            }
            else
            {
                buffer = null;
                return null;
            }
        }

        /// <summary>Write the data to an output stream.</summary>
        public override void Write(ArrayDataIO str)
        {
            // If buffer is still around we can just reuse it,
            // since nothing we've done has invalidated it.
            if (buffer == null)
            {
                if (data == null)
                {
                    throw new FitsException("Attempt to write undefined ASCII Table");
                }

                buffer = new byte[nRows * rowLen];
                bp = new ByteParser(buffer);
                for (var i = 0; i < buffer.Length; i += 1)
                {
                    buffer[i] = (byte)' ';//SupportClass.Identity(' ');
                }

                var bf = new ByteFormatter
                {
                    TruncationThrow = false,
                    TruncateOnOverflow = true
                };

                for (var i = 0; i < nRows; i += 1)
                {
                    for (var j = 0; j < nFields; j += 1)
                    {
                        var offset = i * rowLen + offsets[j];
                        var len = lengths[j];

                        try
                        {
                            if (isNull_Renamed_Field != null && isNull_Renamed_Field[i * nFields + j])
                            {
                                if (nulls[j] == null)
                                {
                                    throw new FitsException("No null value set when needed");
                                }
                                bf.format(nulls[j], buffer, offset, len);
                            }
                            else
                            {
                                if (types[j] == typeof(string))
                                {
                                    var s = (string[])data[j];
                                    bf.format(s[i], buffer, offset, len);
                                }
                                else if (types[j] == typeof(int))
                                {
                                    var ia = (int[])data[j];
                                    bf.format(ia[i], buffer, offset, len);
                                }
                                else if (types[j] == typeof(float))
                                {
                                    var fa = (float[])data[j];
                                    bf.format(fa[i], buffer, offset, len);
                                }
                                else if (types[j] == typeof(double))
                                {
                                    var da = (double[])data[j];
                                    bf.format(da[i], buffer, offset, len);
                                }
                                else if (types[j] == typeof(long))
                                {
                                    var la = (long[])data[j];
                                    bf.format(la[i], buffer, offset, len);
                                }
                            }
                        }
                        catch (TruncationException)
                        {
                            Console.Error.WriteLine("Ignoring truncation error:" + i + "," + j);
                        }
                    }
                }
            }

            // Now write the buffer.
            try
            {
                str.Write(buffer);
                var padding = new byte[FitsUtil.Padding(buffer.Length)];
                for (var i = 0; i < padding.Length; i += 1)
                {
                    padding[i] = (byte)SupportClass.Identity(' ');
                }
                if (buffer.Length > 0)
                {
                    str.Write(padding);
                }
                str.Flush();
            }
            catch (IOException)
            {
                throw new FitsException("Error writing ASCII Table data");
            }
        }

        /// <summary>Replace a column with new data.</summary>
        public virtual void SetColumn(int col, object newData)
        {
            if (data == null)
            {
                var generatedAux = DataArray;
            }
            if (col < 0 || col >= nFields || newData.GetType() != data[col].GetType() || ((Array)newData).Length != ((Array)data[col]).Length)
            {
                throw new FitsException("Invalid column/column mismatch:" + col);
            }
            data[col] = newData;

            // Invalidate the buffer.
            buffer = null;
        }

        /// <summary>Modify a row in the table</summary>
        public virtual void SetRow(int row, Array newData)
        {
            if (row < 0 || row > nRows)
            {
                throw new FitsException("Invalid row in setRow");
            }

            if (data == null)
            {
                var generatedAux = DataArray;
            }
            for (var i = 0; i < nFields; i += 1)
            {
                try
                {
                    Array.Copy((Array)newData.GetValue(i), 0, (Array)data[i], row, 1);
                }
                catch (Exception)
                {
                    throw new FitsException("Unable to modify row: incompatible data:" + row);
                }
            }

            // Invalidate the buffer
            buffer = null;
        }

        /// <summary>Modify an element in the table</summary>
        public virtual void SetElement(int row, int col, object newData)
        {
            if (data == null)
            {
                var generatedAux = DataArray;
            }
            try
            {
                Array.Copy((Array)newData, 0, (Array)data[col], row, 1);
            }
            catch (Exception)
            {
                throw new FitsException("Incompatible element:" + row + "," + col);
            }

            // Invalidate the buffer
            buffer = null;
        }

        /// <summary>Mark (or unmark) an element as null.  Note that if this FITS file is latter
        /// written out, a TNULL keyword needs to be defined in the corresponding
        /// header.  This routine does not add an element for String columns.
        /// </summary>
        public virtual void SetNull(int row, int col, bool flag)
        {
            if (flag)
            {
                if (isNull_Renamed_Field == null)
                {
                    isNull_Renamed_Field = new bool[nRows * nFields];
                }
                isNull_Renamed_Field[col + row * nFields] = true;
            }
            else if (isNull_Renamed_Field != null)
            {
                isNull_Renamed_Field[col + row * nFields] = false;
            }

            // Invalidate the buffer
            buffer = null;
        }

        /// <summary>See if an element is null.</summary>
        public virtual bool IsNull(int row, int col)
        {
            if (isNull_Renamed_Field != null)
            {
                return isNull_Renamed_Field[row * nFields + col];
            }
            else
            {
                return false;
            }
        }

        /// <summary>Add a row to the table. Users should be cautious
        /// of calling this routine directly rather than the corresponding
        /// routine in AsciiTableHDU since this routine knows nothing
        /// of the FITS header modifications required.
        /// </summary>
        public virtual int AddColumn(object newCol)
        {
            var maxLen = 0;
            if (newCol is string[])
            {
                var sa = (string[])newCol;
                for (var i = 0; i < sa.Length; i += 1)
                {
                    if (sa[i] != null && sa[i].Length > maxLen)
                    {
                        maxLen = sa[i].Length;
                    }
                }
            }
            else if (newCol is double[])
            {
                maxLen = 24;
            }
            else if (newCol is int[])
            {
                maxLen = 10;
            }
            else if (newCol is long[])
            {
                maxLen = 20;
            }
            else if (newCol is float[])
            {
                maxLen = 16;
            }
            AddColumn(newCol, maxLen);

            // Invalidate the buffer
            buffer = null;

            return nFields;
        }

        /// <summary>This version of addColumn allows the user to override
        /// the default length associated with each column type.</summary>
        public virtual int AddColumn(object newCol, int length)
        {
            if (nFields > 0 && ((Array)newCol).Length != nRows)
            {
                throw new FitsException("New column has different number of rows");
            }

            if (nFields == 0)
            {
                nRows = ((Array)newCol).Length;
            }

            var newData = new object[nFields + 1];
            var newOffsets = new int[nFields + 1];
            var newLengths = new int[nFields + 1];
            var newTypes = new Type[nFields + 1];
            var newNulls = new string[nFields + 1];

            Array.Copy(data, 0, newData, 0, nFields);
            Array.Copy(offsets, 0, newOffsets, 0, nFields);
            Array.Copy(lengths, 0, newLengths, 0, nFields);
            Array.Copy(types, 0, newTypes, 0, nFields);
            Array.Copy(nulls, 0, newNulls, 0, nFields);

            data = newData;
            offsets = newOffsets;
            lengths = newLengths;
            types = newTypes;
            nulls = newNulls;

            newData[nFields] = newCol;
            offsets[nFields] = rowLen + 1;
            lengths[nFields] = length;
            types[nFields] = ArrayFuncs.GetBaseClass(newCol);

            rowLen += length + 1;
            if (isNull_Renamed_Field != null)
            {
                var newIsNull = new bool[nRows * (nFields + 1)];

                // Fix the null pointers.
                var add = 0;
                for (var i = 0; i < isNull_Renamed_Field.Length; i += 1)
                {
                    if (i % nFields == 0)
                    {
                        add += 1;
                    }
                    if (isNull_Renamed_Field[i])
                    {
                        newIsNull[i + add] = true;
                    }
                }
                isNull_Renamed_Field = newIsNull;
            }
            nFields += 1;

            // Invalidate the buffer
            buffer = null;

            return nFields;
        }

        /// <summary>Add a row to the FITS table.</summary>
        public virtual int AddRow(Array newRow)
        {
            // If there are no fields, then this is the
            // first row.  We need to add in each of the columns
            // to get the descriptors set up.
            if (nFields == 0)
            {
                for (var i = 0; i < newRow.Length; i += 1)
                {
                    AddColumn(newRow.GetValue(i));
                }
            }
            else
            {
                for (var i = 0; i < nFields; i += 1)
                {
                    try
                    {
                        var o = ArrayFuncs.NewInstance(types[i], nRows + 1);
                        Array.Copy((Array)data[i], 0, o, 0, nRows);
                        Array.Copy((Array)newRow.GetValue(i), 0, o, nRows, 1);
                        data[i] = o;
                    }
                    catch (Exception e)
                    {
                        throw new FitsException("Error adding row:" + e);
                    }
                }
                nRows += 1;
            }

            // Invalidate the buffer
            buffer = null;

            return nRows;
        }

        /// <summary>Set the null string for a columns.
        /// This is not a public method since we
        /// want users to call the method in AsciiTableHDU
        /// and update the header also.
        /// </summary>
        internal virtual void SetNullString(int col, string newNull)
        {
            if (col >= 0 && col < nulls.Length)
            {
                nulls[col] = newNull;
            }
        }

        /// <summary>Fill in a header with information that points to this data.</summary>
        internal override void FillHeader(Header hdr)
        {
            try
            {
                hdr.Xtension = "TABLE";
                hdr.Bitpix = 8;
                hdr.Naxes = 2;
                hdr.SetNaxis(1, rowLen);
                hdr.SetNaxis(2, nRows);
                var c = (Cursor)hdr.GetEnumerator();
                c.Key = "NAXIS2";
                c.MoveNext();
                c.Add("PCOUNT", new HeaderCard("PCOUNT", 0, "No group data"));
                c.Add("GCOUNT", new HeaderCard("GCOUNT", 1, "One group"));
                c.Add("TFIELDS", new HeaderCard("TFIELDS", nFields, "Number of fields in table"));

                for (var i = 0; i < nFields; i += 1)
                {
                    AddColInfo(i, c);
                }
            }
            catch (HeaderCardException e)
            {
                Console.Error.WriteLine("ImpossibleException in fillHeader:" + e);
            }
        }

        internal virtual int AddColInfo(int col, Cursor c)
        {
            string tform = null;
            if (types[col] == typeof(string))
            {
                tform = "A" + lengths[col];
            }
            else if (types[col] == typeof(int) || types[col] == typeof(long))
            {
                tform = "I" + lengths[col];
            }
            else if (types[col] == typeof(float))
            {
                tform = "E" + lengths[col] + ".0";
            }
            else if (types[col] == typeof(double))
            {
                tform = "D" + lengths[col] + ".0";
            }
            string key;
            key = "TFORM" + (col + 1);
            c.Add(key, new HeaderCard(key, tform, null));
            key = "TBCOL" + (col + 1);
            c.Add(key, new HeaderCard(key, offsets[col] + 1, null));

            return lengths[col];
        }
    }
}