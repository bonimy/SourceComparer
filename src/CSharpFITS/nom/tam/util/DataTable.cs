// <copyright file="DataTable.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

namespace nom.tam.util
{
    using System;

    /* Copyright: Thomas McGlynn 1997-1998.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	*/

    /// <summary>This interface defines the properties that
    /// a generic table should have.
    /// </summary>

    public interface DataTable
    {
        int NRows
        {
            get;
        }

        int NCols
        {
            get;
        }

        object GetRow(int row);

        void SetRow(int row, object newRow);

        object GetColumn(int column);

        void SetColumn(int column, object newColumn);

        object GetElement(int row, int col);

        void SetElement(int row, int col, object newElement);
    }
}
