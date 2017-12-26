namespace nom.tam.fits
{
	/*
	* Copyright: Thomas McGlynn 1997-1998.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	*
	* Many thanks to David Glowacki (U. Wisconsin) for substantial
	* improvements, enhancements and bug fixes.
	*/
	using System;
	using nom.tam.util;
	/// <summary>FITS binary table header/data unit</summary>
	public class BinaryTableHDU:TableHDU
	{
		/// <summary>Check that this HDU has a valid header.</summary>
		/// <returns> <CODE>true</CODE> if this HDU has a valid header.</returns>
		virtual public bool HasHeader
		{
			get
			{
				return IsHeader(myHeader);
			}
		}
		
		private BinaryTable table;
		
		public BinaryTableHDU(Header hdr, Data datum):base((TableData) datum)
		{
			myHeader = hdr;
			myData = datum;
			table = (BinaryTable) datum;
		}
		
		/// <summary>Create data from a binary table header.</summary>
		/// <param name="header">the template specifying the binary table.</param>
		/// <exception cref=""> FitsException if there was a problem with the header.</exception>
		public static Data ManufactureData(Header header)
		{
			return new BinaryTable(header);
		}
		
		internal override Data ManufactureData()
		{
			return ManufactureData(myHeader);
		}
		
		/// <summary>Build a binary table HDU from the supplied data.</summary>
		/// <param name="table">the array used to build the binary table.</param>
		/// <exception cref=""> FitsException if there was a problem with the data.</exception>
		public static Header ManufactureHeader(Data data)
		{
			var hdr = new Header();
			data.FillHeader(hdr);
			return hdr;
		}
		
		/// <summary>Encapsulate data in a BinaryTable data type</summary>
		public static Data Encapsulate(object o)
		{
			if(o is ColumnTable)
			{
				return new BinaryTable((ColumnTable) o);
			}
			else if (o is object[][])
			{
				return new BinaryTable((object[][]) o);
			}
			else if (o is object[])
			{
				return new BinaryTable((object[]) o);
			}
			else
			{
				throw new FitsException("Unable to encapsulate object of type:" + o.GetType().FullName + " as BinaryTable");
			}
		}
		
		/// <summary>Check that this is a valid binary table header.</summary>
		/// <param name="header">to validate.</param>
		/// <returns> <CODE>true</CODE> if this is a binary table header.</returns>
		public static new bool IsHeader(Header header)
		{
            var xten = header.GetStringValue("XTENSION");
			if (xten == null)
			{
				return false;
			}
			xten = xten.Trim();
			if (xten.Equals("BINTABLE") || xten.Equals("A3DTABLE"))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		
		/* Check if this data object is consistent with a binary table.  There
		* are three options:  a column table object, an Object[][], or an Object[].
		* This routine doesn't check that the dimensions of arrays are properly
		* consistent.
		*/
		public static bool IsData(object o)
		{
			if (o is ColumnTable || o is object[][] || o is object[])
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		/// <summary>Add a column without any associated header information.</summary>
		/// <param name="data">The column data to be added.  Data should be an Object[] where
		/// type of all of the constituents is identical.  The length
		/// of data should match the other columns.  <b> Note:</b> It is
		/// valid for data to be a 2 or higher dimensionality primitive
		/// array.  In this case the column index is the first (in Java speak)
		/// index of the array.  E.g., if called with int[30][20][10], the
		/// number of rows in the table should be 30 and this column
		/// will have elements which are 2-d integer arrays with TDIM = (10,20).
		/// </param>
		/// <exception cref=""> FitsException the column could not be added.</exception>
		public override int AddColumn(object data)
		{
			var col = table.AddColumn(data);
			table.PointToColumn(NCols - 1, myHeader);
			return col;
		}
		
		// Need to tell header about the Heap before writing.
		public override void Write(ArrayDataIO ado)
		{
			var oldSize = myHeader.GetIntValue("PCOUNT");
			if (oldSize != table.HeapSize)
			{
				myHeader.AddValue("PCOUNT", table.HeapSize, "Includes Heap");
			}
			
			if (myHeader.GetIntValue("PCOUNT") == 0)
			{
				myHeader.DeleteKey("THEAP");
			}
			else
			{
				myHeader.GetIntValue("TFIELDS");
				var offset = myHeader.GetIntValue("NAXIS1") * myHeader.GetIntValue("NAXIS2") + table.HeapOffset;
				myHeader.AddValue("THEAP", offset, "");
			}
			
			base.Write(ado);
		}
		
		/// <summary>Print out some information about this HDU.</summary>
		public override void Info()
		{
			var myData = (BinaryTable) this.myData;

      Console.Out.WriteLine("  Binary Table");
			Console.Out.WriteLine("      Header Information:");
			
			var nhcol = myHeader.GetIntValue("TFIELDS", - 1);
			var nrow = myHeader.GetIntValue("NAXIS2", - 1);
			var rowsize = myHeader.GetIntValue("NAXIS1", - 1);
			
			Console.Out.Write("          " + nhcol + " fields");
			Console.Out.WriteLine(", " + nrow + " rows of length " + rowsize);
			
			for (var i = 1; i <= nhcol; i += 1)
			{
				Console.Out.Write("           " + i + ":");
				CheckField("TTYPE" + i);
				CheckField("TFORM" + i);
				CheckField("TDIM" + i);
				Console.Out.WriteLine(" ");
			}
			
			Console.Out.WriteLine("      Data Information:");
			if (myData == null || table.NRows == 0 || table.NCols == 0)
			{
				Console.Out.WriteLine("         No data present");
				if (table.HeapSize > 0)
				{
					Console.Out.WriteLine("         Heap size is: " + table.HeapSize + " bytes");
				}
			}
			else
			{
				Console.Out.WriteLine("          Number of rows=" + table.NRows);
				Console.Out.WriteLine("          Number of columns=" + table.NCols);
				if (table.HeapSize > 0)
				{
					Console.Out.WriteLine("          Heap size is: " + table.HeapSize + " bytes");
				}
				var cols = table.FlatColumns;
				for (var i = 0; i < cols.Length; i += 1)
				{
					Console.Out.WriteLine("           " + i + ":" + ArrayFuncs.ArrayDescription(cols[i]));
				}
			}
		}
	}
}
