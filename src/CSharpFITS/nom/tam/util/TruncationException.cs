// <copyright file="TruncationException.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

namespace nom.tam.util
{
    using System;

    public class TruncationException : Exception
    {
        public TruncationException() : base()
        {
        }

        public TruncationException(string msg) : base(msg)
        {
        }
    }
}
