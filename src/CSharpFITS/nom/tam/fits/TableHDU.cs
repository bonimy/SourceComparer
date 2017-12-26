// <copyright file="TableHDU.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

namespace nom.tam.fits
{
    using System;

    /// <summary>This class allows FITS binary and ASCII tables to
    /// be accessed via a common interface.
    ///
    /// Bug Fix: 3/28/01 to findColumn.
    /// </summary>

    public abstract class TableHDU : BasicHDU
    {
        /// <summary>Get the number of columns for this table</summary>
        /// <returns> The number of columns in the table.</returns>
        virtual public int NCols
        {
            get
            {
                return table.NCols;
            }
        }

        /// <summary>Get the number of rows for this table</summary>
        /// <returns> The number of rows in the table.</returns>
        virtual public int NRows
        {
            get
            {
                return table.NRows;
            }
        }

        virtual public int CurrentColumn
        {
            set
            {
                myHeader.PositionAfterIndex("TFORM", (value + 1));
            }
        }

        private TableData table;

        //private int currentColumn;

        internal TableHDU(TableData td)
        {
            table = td;
        }

        public virtual Array GetRow(int row)
        {
            return table.GetRow(row);
        }

        public virtual object GetColumn(string colName)
        {
            return GetColumn(FindColumn(colName));
        }

        public virtual object GetColumn(int col)
        {
            return table.GetColumn(col);
        }

        public virtual object GetElement(int row, int col)
        {
            return table.GetElement(row, col);
        }

        public virtual void SetRow(int row, Array newRow)
        {
            table.SetRow(row, newRow);
        }

        public virtual void SetColumn(string colName, object newCol)
        {
            SetColumn(FindColumn(colName), newCol);
        }

        public virtual void SetColumn(int col, object newCol)
        {
            table.SetColumn(col, newCol);
        }

        public virtual void SetElement(int row, int col, object element)
        {
            table.SetElement(row, col, element);
        }

        public virtual int AddRow(Array newRow)
        {
            var row = table.AddRow(newRow);
            myHeader.AddValue("NAXIS2", row, null);
            return row;
        }

        public virtual int FindColumn(string colName)
        {
            for (var i = 0; i < NCols; i += 1)
            {
                var val = myHeader.GetStringValue("TTYPE" + (i + 1));
                if (val != null && val.Trim().Equals(colName))
                {
                    return i;
                }
            }
            return -1;
        }

        public abstract int AddColumn(object data);

        /// <summary>Get the name of a column in the table.</summary>
        /// <param name="index">The 0-based column index.</param>
        /// <returns> The column name.</returns>
        /// <exception cref=""> FitsException if an invalid index was requested.</exception>
        public virtual string GetColumnName(int index)
        {
            var ttype = myHeader.GetStringValue("TTYPE" + (index + 1));
            if (ttype != null)
            {
                ttype = ttype.Trim();
            }
            return ttype;
        }

        public virtual void SetColumnName(int index, string name, string comment)
        {
            if (NCols > index && index >= 0)
            {
                myHeader.PositionAfterIndex("TFORM", index + 1);
                myHeader.AddValue("TTYPE" + (index + 1), name, comment);
            }
        }

        /// <summary>Get the FITS type of a column in the table.</summary>
        /// <returns> The FITS type.</returns>
        /// <exception cref=""> FitsException if an invalid index was requested.</exception>
        public virtual string GetColumnFormat(int index)
        {
            var flds = myHeader.GetIntValue("TFIELDS", 0);
            if (index < 0 || index >= flds)
            {
                throw new FitsException("Bad column index " + index + " (only " + flds + " columns)");
            }

            return myHeader.GetStringValue("TFORM" + (index + 1)).Trim();
        }
    }
}
