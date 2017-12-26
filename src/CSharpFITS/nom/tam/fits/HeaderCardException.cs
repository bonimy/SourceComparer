// <copyright file="HeaderCardException.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

namespace nom.tam.fits
{
    /*
	* Copyright: Thomas McGlynn 1997-1998.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	* Many thanks to David Glowacki (U. Wisconsin) for substantial
	* improvements, enhancements and bug fixes.
	*/

    /* This class was contributed by David Glowacki */

    public class HeaderCardException : FitsException
    {
        public HeaderCardException() : base()
        {
        }

        public HeaderCardException(string s) : base(s)
        {
        }
    }
}
