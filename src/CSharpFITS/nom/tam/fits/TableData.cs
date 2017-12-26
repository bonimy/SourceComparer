namespace nom.tam.fits
{
	using System;
	/// <summary>This class allows FITS binary and ASCII tables to be accessed via a common interface.</summary>
	
	public interface TableData
		{
			int NCols
			{
				get;
				
			}
			int NRows
			{
				get;
				
			}
			Array GetRow(int row);
        object GetColumn(int col);
        object GetElement(int row, int col);
			void SetRow(int row, Array newRow);
			void SetColumn(int col, object newCol);
			void SetElement(int row, int col, object element);
			int AddRow(Array newRow);
			int AddColumn(object newCol);
		}
}
