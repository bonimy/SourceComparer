// <copyright file="RandomAccess.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

namespace nom.tam.util
{
    /// <summary>These packages define the methods which indicate that
    /// an i/o stream may be accessed in arbitrary order.
    /// The method signatures are taken from RandomAccessFile
    /// though that class does not implement this interface.
    /// </summary>
    //public interface RandomAccess : ArrayDataIO
    public abstract class RandomAccess : ArrayDataIO
    {
        /// <summary>Get the current position in the stream</summary>
        /*
		public abstract long FilePointer
		{
			get;
		}
    */
    }
}
