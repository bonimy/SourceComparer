// <copyright file="FitsDate.cs" company="Public Domain">
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
	*
	* This class was contributed by D. Glowacki.
	*/

    using System;

    public class FitsDate
    {
        /// <summary>Return the current date in FITS date format</summary>
        public static string FitsDateString
        {
            get
            {
                return GetFitsDateString(DateTime.Now, true);
            }
        }

        private int year = -1;
        private int month = -1;
        private int mday = -1;
        private int hour = -1;
        private int minute = -1;
        private int second = -1;
        private int millisecond = -1;

        private DateTime date = (DateTime)((object)null);

        /// <summary> Convert a FITS date string to a Java <CODE>Date</CODE> object.</summary>
        /// <param name="dStr	the">FITS date</param>
        /// <returns>	either <CODE>null</CODE> or a Date object</returns>
        /// <exception cref=""> FitsException	if <CODE>dStr</CODE> does not contain a valid FITS date.</exception>
        public FitsDate(string dStr)
        {
            // if the date string is null, we are done
            if (dStr == null)
            {
                return;
            }

            // if the date string is empty, we are done
            dStr = dStr.Trim();
            if (dStr.Length == 0)
            {
                return;
            }

            // if string contains at least 8 characters...
            var len = dStr.Length;
            if (len >= 8)
            {
                int first;

                // ... and there is a "/" in the string...
                first = dStr.IndexOf('-');
                if (first == 4 && first < len)
                {
                    // ... this must be an new-style date
                    BuildNewDate(dStr, first, len);

                    // no "/" found; maybe it is an old-style date...
                }
                else
                {
                    first = dStr.IndexOf('/');
                    if (first > 1 && first < len)
                    {
                        // ... this must be an old-style date
                        BuildOldDate(dStr, first, len);
                    }
                }
            }

            if (year == -1)
            {
                throw new FitsException("Bad FITS date string \"" + dStr + '"');
            }
        }

        private void BuildOldDate(string dStr, int first, int len)
        {
            //int middle = dStr.IndexOf((System.Char) '/', first + 1);
            var middle = SupportClass.StringIndexOf(dStr, '/', first + 1);
            if (middle > first + 2 && middle < len)
            {
                try
                {
                    year = Int32.Parse(dStr.Substring(middle + 1)) + 1900;
                    month = Int32.Parse(dStr.Substring(first + 1, (middle) - (first + 1)));
                    mday = Int32.Parse(dStr.Substring(0, (first) - (0)));
                }
                catch (FormatException)
                {
                    year = month = mday = -1;
                }
            }
        }

        private void ParseTime(string tStr)
        {
            var first = tStr.IndexOf(':');
            if (first < 0)
            {
                throw new FitsException("Bad time");
            }

            var len = tStr.Length;

            //int middle = tStr.IndexOf((System.Char) ':', first + 1);
            var middle = SupportClass.StringIndexOf(tStr, ':', first + 1);
            if (middle > first + 2 && middle < len)
            {
                if (middle + 3 < len && tStr[middle + 3] == '.')
                {
                    var d = Double.Parse(tStr.Substring(middle + 3));
                    millisecond = (int)(d * 1000);

                    len = middle + 3;
                }

                try
                {
                    hour = Int32.Parse(tStr.Substring(0, (first) - (0)));
                    minute = Int32.Parse(tStr.Substring(first + 1, (middle) - (first + 1)));
                    second = Int32.Parse(tStr.Substring(middle + 1, (len) - (middle + 1)));
                }
                catch (FormatException)
                {
                    hour = minute = second = millisecond = -1;
                }
            }
        }

        private void BuildNewDate(string dStr, int first, int len)
        {
            // find the middle separator
            //int middle = dStr.IndexOf((System.Char) '-', first + 1);
            var middle = SupportClass.StringIndexOf(dStr, '-', first + 1);
            if (middle > first + 2 && middle < len)
            {
                try
                {
                    // if this date string includes a time...
                    if (middle + 3 < len && dStr[middle + 3] == 'T')
                    {
                        // ... try to parse the time
                        try
                        {
                            ParseTime(dStr.Substring(middle + 4));
                        }
                        catch (FitsException)
                        {
                            throw new FitsException("Bad time in FITS date string \"" + dStr + "\"");
                        }

                        // we got the time; mark the end of the date string
                        len = middle + 3;
                    }

                    // parse date string
                    year = Int32.Parse(dStr.Substring(0, (first) - (0)));
                    month = Int32.Parse(dStr.Substring(first + 1, (middle) - (first + 1)));
                    mday = Int32.Parse(dStr.Substring(middle + 1, (len) - (middle + 1)));
                }
                catch (FormatException)
                {
                    // yikes, something failed; reset everything
                    year = month = mday = hour = minute = second = millisecond = -1;
                }
            }
        }

        /// <summary>Get a Java Date object corresponding to this FITS date.</summary>
        /// <returns> The Java Date object.</returns>
        public virtual DateTime ToDate()
        {
            if (((object)date) == null && year != -1)
            {
                date = hour == -1 ?
                  new DateTime(year, month, mday, 0, 0, 0, 0) :
                  new DateTime(year, month, mday, hour, minute, second,
                               millisecond == -1 ? 0 : millisecond);
            }

            return date;
        }

        /// <summary>Create FITS format date string Java Date object.</summary>
        /// <param name="epoch">The epoch to be converted to FITS format.</param>
        public static string GetFitsDateString(DateTime epoch)
        {
            return GetFitsDateString(epoch, true);
        }

        /// <summary>Create FITS format date string. Note that the date is not rounded.</summary>
        /// <param name="epoch">The epoch to be converted to FITS format.</param>
        /// <param name="timeOfDay">Should time of day information be included?</param>
        public static string GetFitsDateString(DateTime epoch, bool timeOfDay)
        {
            try
            {
                var fitsDate = new System.Text.StringBuilder();
                if (timeOfDay)
                {
                    fitsDate.AppendFormat("{0:s}", epoch);
                    fitsDate.Append("." + epoch.Millisecond);
                }
                else
                {
                    fitsDate.AppendFormat("{0:D4}", epoch.Year);
                    fitsDate.Append("-");
                    fitsDate.AppendFormat("{0:D2}", epoch.Month);
                    fitsDate.Append("-");
                    fitsDate.AppendFormat("{0:D2}", epoch.Day);
                }

                return new string(fitsDate.ToString().ToCharArray());
            }
            catch (Exception)
            {
                return new string("".ToCharArray());
            }
        }

        public override string ToString()
        {
            if (year == -1)
            {
                return "";
            }

            var buf = new System.Text.StringBuilder(23);
            buf.Append(year);
            buf.Append('-');
            if (month < 10)
            {
                buf.Append('0');
            }
            buf.Append(month);
            buf.Append('-');
            if (mday < 10)
            {
                buf.Append('0');
            }
            buf.Append(mday);

            if (hour != -1)
            {
                buf.Append('T');
                if (hour < 10)
                {
                    buf.Append('0');
                }

                buf.Append(hour);
                buf.Append(':');

                if (minute < 10)
                {
                    buf.Append('0');
                }

                buf.Append(minute);
                buf.Append(':');

                if (second < 10)
                {
                    buf.Append('0');
                }
                buf.Append(second);

                if (millisecond != -1)
                {
                    buf.Append('.');

                    if (millisecond < 100)
                    {
                        if (millisecond < 10)
                        {
                            buf.Append("00");
                        }
                        else
                        {
                            buf.Append('0');
                        }
                    }
                    buf.Append(millisecond);
                }
            }

            return buf.ToString();
        }

        public static void TestArgs(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                try
                {
                    var fd = new FitsDate(args[i]);
                    Console.Out.WriteLine("\"" + args[i] + "\" => " + fd + " => " + fd.ToDate());
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Date \"" + args[i] + "\" threw " + e.GetType().FullName + "(" + e.Message + ")");
                }
            }
        }

        public static void Autotest()
        {
            var good = new string[6];
            good[0] = "20/09/79";
            good[1] = "1997-07-25";
            good[2] = "1987-06-05T04:03:02.01";
            good[3] = "1998-03-10T16:58:34";
            good[4] = null;
            good[5] = "        ";
            TestArgs(good);

            var badOld = new string[4];
            badOld[0] = "20/09/";
            badOld[1] = "/09/79";
            badOld[2] = "09//79";
            badOld[3] = "20/09/79/";
            TestArgs(badOld);

            var badNew = new string[4];
            badNew[0] = "1997-07";
            badNew[1] = "-07-25";
            badNew[2] = "1997--07-25";
            badNew[3] = "1997-07-25-";
            TestArgs(badNew);

            var badMisc = new string[4];
            badMisc[0] = "5-Aug-1992";
            badMisc[1] = "28/02/91 16:32:00";
            badMisc[2] = "18-Feb-1993";
            badMisc[3] = "nn/nn/nn";
            TestArgs(badMisc);
        }

        [STAThread]
        public static void Test(string[] args)
        {
            if (args.Length == 0)
            {
                Autotest();
            }
            else
            {
                TestArgs(args);
            }
        }
    }
}
