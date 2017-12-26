// <copyright file="DefaultValueHashtable.cs" company="Public Domain">
//     Copyright (c) 2017 Samuel Carliles.
// </copyright>

using System;
using System.Collections;

namespace nom.tam.util
{
    /// <summary>
    /// Summary description for DefaultValueHashtable.
    /// </summary>
    public class DefaultValueHashtable : Hashtable
    {
        public override object this[object key]
        {
            get
            {
                _result = base[key];
                if (_result == null)
                {
                    _result = DefaultValue;
                }

                return _result;
            }

            set
            {
                if (key == null)
                {
                    DefaultValue = value;
                }
                else
                {
                    base[key] = value;
                }
            }
        }

        public object DefaultValue
        {
            get
            {
                return _defaultValue;
            }

            set
            {
                _defaultValue = value;
            }
        }

        public DefaultValueHashtable() : this(null)
        {
        }

        public DefaultValueHashtable(object defaultValue) : base()
        {
            DefaultValue = defaultValue;
        }

        protected object _result = null;
        protected object _defaultValue = null;
    }
}
