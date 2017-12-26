// <copyright file="ConfigStream.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

using System;
using System.IO;

namespace nom.tam.util
{
    /// <summary>
    /// Summary description for ConfigStream.
    /// </summary>
    public class ConfigStream : AdapterStream
    {
        public override bool CanSeek
        {
            get
            {
                if (_canSeek)
                {
                    return base.CanSeek;
                }

                return false;
            }
        }

        public ConfigStream(Stream s) : base(s)
        {
        }

        public void SetCanSeek(bool canSeek)
        {
            _canSeek = canSeek;
        }

        private bool _canSeek;
    }
}
