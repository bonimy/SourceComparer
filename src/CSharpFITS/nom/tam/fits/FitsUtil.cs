namespace nom.tam.fits
{
	/* Copyright: Thomas McGlynn 1999.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	*/
	using System;
  using System.IO;
	using nom.tam.util;
	/// <summary>This class comprises static
	/// utility functions used throughout
	/// the FITS classes.
	/// </summary>
	public class FitsUtil
	{
		/// <summary>Reposition a random access stream to a requested offset</summary>
		public static void Reposition(object o, long offset)
		{
			if (o == null)
			{
				throw new FitsException("Attempt to reposition null stream");
			}
			if (!(o is RandomAccess) || offset < 0)
			{
				throw new FitsException("Invalid attempt to reposition stream " + o + " of type " + o.GetType().FullName + " to " + offset);
			}
			
			try
			{
				((RandomAccess)o).Seek(offset, SeekOrigin.Begin);
			}
			catch(IOException e)
			{
				throw new FitsException("Unable to repostion stream " + o + " of type " + o.GetType().FullName + " to " + offset + "   Exception:" + e);
			}
		}
		
		/// <summary>Find out where we are in a random access file</summary>
		public static long FindOffset(object o)
		{
			if(o is ArrayDataIO && ((ArrayDataIO)o).CanSeek)//if(o is RandomAccess)
			{
				//return ((RandomAccess) o).FilePointer;
        return ((ArrayDataIO)o).Position;
			}
			else
			{
				return - 1;
			}
		}
		
		/// <summary>How many bytes are needed to fill the last 2880 block?</summary>
		public static int Padding(int size)
		{
			var mod = size % 2880;
			if (mod > 0)
			{
				mod = 2880 - mod;
			}
			return mod;
		}
		
    /// <summary>How many bytes are needed to fill the last 2880 block?</summary>
    public static int Padding(long size)
    {
      var mod = (int)(size % (long)2880);
      if (mod > 0)
      {
        mod = 2880 - mod;
      }
      return mod;
    }
		
    /// <summary>Total size of blocked FITS element</summary>
		public static int AddPadding(int size)
		{
			return size + Padding(size);
		}
		
		/// <summary>Check if a filename seems to imply a compressed data stream.</summary>
		public static bool IsCompressed(string filename)
		{
			var len = filename.Length;
			return (len > 2 && (filename.Substring(len - 3).ToUpper().Equals(".gz".ToUpper())));
		}
		
		/// <summary>Get the maximum length of a String in a String array.</summary>
		public static int MaxLength(string[] o)
		{
			var max = 0;
			for (var i = 0; i < o.Length; i += 1)
			{
				if (o != null && o[i].Length > max)
				{
					max = o[i].Length;
				}
			}
			return max;
		}
		
		/// <summary>Copy an array of Strings to bytes.</summary>
		public static byte[] StringsToByteArray(string[] o, int maxLen)
		{
			var res = new byte[o.Length * maxLen];
			for (var i = 0; i < o.Length; i += 1)
			{
        var bstr = SupportClass.ToByteArray(o[i]);
				var cnt = bstr.Length;
				if (cnt > maxLen)
				{
					cnt = maxLen;
				}
				Array.Copy(bstr, 0, res, i * maxLen, cnt);
				for (var j = cnt; j < maxLen; j += 1)
				{
					res[i * maxLen + j] = (byte)SupportClass.Identity(' ');
				}
			}
			return res;
		}
		
		/// <summary>Convert bytes to Strings</summary>
		public static string[] ByteArrayToStrings(byte[] o, int maxLen)
		{
            var res = new string[o.Length / maxLen];
			for (var i = 0; i < res.Length; i += 1)
			{
				char[] tmpChar;
				tmpChar = new char[o.Length];
				o.CopyTo(tmpChar, 0);
				res[i] = new string(tmpChar, i * maxLen, maxLen).Trim();
			}
			return res;
		}
		
		
		/// <summary>Convert an array of booleans to bytes</summary>
		internal static byte[] BooleanToByte(bool[] bool_Renamed)
		{
			var byt = new byte[bool_Renamed.Length];
			for (var i = 0; i < bool_Renamed.Length; i += 1)
			{
				byt[i] = bool_Renamed[i]?(byte)SupportClass.Identity('T'):(byte)SupportClass.Identity('F');
			}
			return byt;
		}

		/// <summary>Convert an array of bytes to booleans</summary>
		internal static bool[] ByteToBoolean(byte[] byt)
		{
			var bool_Renamed = new bool[byt.Length];
			
			for(var i = 0; i < byt.Length; i += 1)
			{
				bool_Renamed[i] = (byt[i] == (byte)'T');
			}
			return bool_Renamed;
		}
	}
}
