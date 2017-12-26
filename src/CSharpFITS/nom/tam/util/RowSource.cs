namespace nom.tam.util
{
  using System;
  using System.Collections;
  using System.Data;

  /// <summary>
	/// Summary description for RowSource.
	/// </summary>
	public abstract class RowSource
	{
    public const int NA = -1;

    public abstract int NRows
    {
      get;
    }

    public abstract string[] ColumnNames
    {
      get;
    }

    public abstract Array[] ModelRow
    {
      get;
    }

    public abstract object[] TNULL
    {
      get;
    }

    public abstract Array[] GetNextRow(ref Array[] row);
	}

  public abstract class RowAdapter : RowSource
  {
    #region RowSource Members
    public override int NRows
    {
      get
      {
        return _nrows;
      }
    }

    public override string[] ColumnNames
    {
      get
      {
        return _columnNames;
      }
    }

    public override Array[] ModelRow
    {
      get
      {
        return _modelRow;
      }
    }

    public override object[] TNULL
    {
      get
      {
        return _tnull;
      }
    }
    #endregion

    protected RowAdapter(int nrows, string[] columnNames, Array[] modelRow, object[] tnull)
    {
      _nrows = nrows;
      _columnNames = columnNames;
      _modelRow = modelRow;
      _tnull = tnull;
    }

    protected int _nrows;
    protected string[] _columnNames;
    protected Array[] _modelRow;
    protected object[] _tnull;
  }

  public class DataReaderAdapter : RowAdapter
  {
    public DataReaderAdapter(IDataReader reader) : this(reader, RowSource.NA)
    {
    }

    public DataReaderAdapter(IDataReader reader, int nrows) :
      base(nrows, MakeColumnNames(reader), MakeModelRow(reader), MakeTNULL(reader))
    {
      _haveRow = reader.Read();
      _reader = reader;
      _rowStuffers = new RowStuffer[reader.FieldCount];
      //_rowStuffers = new RowStuffer[reader.GetSchemaTable().Columns.Count];

      for(var i = 0; i < _rowStuffers.Length; ++i)
      {
        var t = reader.GetFieldType(i);
        //_rowStuffers[i] = RowStuffer.GetRowStuffer(reader.GetFieldType(i));
        try
        {
          _rowStuffers[i] = RowStuffer.GetRowStuffer(t);
        }
        catch(Exception e)
        {
          throw e;
        }
        if(reader.GetFieldType(i) == typeof(string))
        {
            _rowStuffers[i] =
              //new RowStuffer.StringRowStuffer(reader.GetSchemaTable().Columns[i].MaxLength);
              new RowStuffer.StringRowStuffer(30);
        }
      }
    }

    public override Array[] GetNextRow(ref Array[] row)
    {
      if(!_haveRow)
      {
        return null;
      }

      if(row == null || row.Length != _reader.FieldCount)
      {
        Type t = null;
        row = new Array[_reader.FieldCount];
        for(var i = 0; i < row.Length; ++i)
        {
          t = GetFieldType(_reader, i);
          t = t == typeof(decimal) ? typeof(long) : t;
          row[i] = Array.CreateInstance(t, 1);
        }
      }

      for(var i = 0; i < row.Length; ++i)
      {
        row[i] = _rowStuffers[i].Stuff(_reader[i]);
      }
      _haveRow = _reader.Read();

      return row;
    }

    protected bool _haveRow = false;
    protected static string[] MakeColumnNames(IDataReader reader)
    {
            var result = new string[reader.FieldCount];

      for(var i = 0; i < result.Length; ++i)
      {
        result[i] = reader.GetName(i);
        if("".Equals(result[i]))
        {
          result[i] = "Column" + i;
        }
      }

      return result;
    }

    protected static Array[] MakeModelRow(IDataReader reader)
    {
      var result = new Array[reader.FieldCount];
      Type t = null;

      for(var i = 0; i < result.Length; ++i)
      {
        t = GetFieldType(reader, i);
        t = t == typeof(decimal) ? typeof(long) : t; // to handle mono bug where long => decimal!
        result[i] = Array.CreateInstance(t, 1);

        if(t == typeof(string))
        {
          result[i].SetValue(" ", 0);
        }
      }

      return result;
    }

    protected static object[] MakeTNULL(IDataReader reader)
    {
      var result = new object[reader.FieldCount];

      for(var i = 0; i < result.Length; ++i)
      {
        result[i] = RowStuffer.GetDefaultNullValue(reader.GetFieldType(i));
        if(reader.GetFieldType(i) == typeof(string))
        {
          result[i] = new string(' ', 1);
        }
      }

      return result;
    }

    protected static Type GetFieldType(IDataReader r, int field)
    {
      var result = r.GetFieldType(field);
      result = result == typeof(bool) ? typeof(Troolean) : result;
      return result;
    }

    protected IDataReader _reader;
    protected RowStuffer[] _rowStuffers;
  }

  public abstract class RowStuffer
  {
    public abstract Array Stuff(object o);
    public abstract RowStuffer GetRowStuffer();
    protected abstract object TNull
    {
      get;
    }

    protected object CheckValue(object o)
    {
      if(o == null || o.GetType() == typeof(DBNull))
      {
        return TNull;
      }

      return o;
    }

    public static RowStuffer GetRowStuffer(Type t)
    {
      return ((RowStuffer)_rowStuffers[t]).GetRowStuffer();
    }

    public class DefaultRowStuffer : RowStuffer
    {
      public override Array Stuff(object o)
      {
        return _b;
      }

      public override RowStuffer GetRowStuffer()
      {
        return new DefaultRowStuffer();
      }

      protected byte[] _b = new byte[0];
      protected override object TNull
      {
        get
        {
          return null;
        }
      }
    }

    public class ByteRowStuffer : RowStuffer
    {
      public override Array Stuff(object o)
      {
        o = CheckValue(o);
        _b[0] = (byte)o;
        return _b;
      }

      public override RowStuffer GetRowStuffer()
      {
        return new ByteRowStuffer();
      }

      protected byte[] _b = new byte[1];
      protected override object TNull
      {
        get
        {
          return (byte)0;
        }
      }
    }

    public class TrooleanRowStuffer : RowStuffer
    {
      public override Array Stuff(object o)
      {
        o = CheckValue(o);
        _b[0] = (Troolean)o;
        return _b;
      }

      public override RowStuffer GetRowStuffer()
      {
        return new TrooleanRowStuffer();
      }

      protected Troolean[] _b = new Troolean[1];
      protected Troolean _null = new Troolean(false, true);
      protected override object TNull
      {
        get
        {
          return _null;
        }
      }
    }

    public class CharRowStuffer : RowStuffer
    {
      public override Array Stuff(object o)
      {
        o = CheckValue(o);
        _c[0] = (char)o;
        return _c;
      }

      public override RowStuffer GetRowStuffer()
      {
        return new CharRowStuffer();
      }

      protected char[] _c = new char[1];
      protected override object TNull
      {
        get
        {
          return '\0';
        }
      }
    }

    public class ShortRowStuffer : RowStuffer
    {
      public override Array Stuff(object o)
      {
        o = CheckValue(o);
        _s[0] = (short)o;
        return _s;
      }

      public override RowStuffer GetRowStuffer()
      {
        return new ShortRowStuffer();
      }

      protected short[] _s = new short[1];
      protected override object TNull
      {
        get
        {
          return (short)-99;
        }
      }
    }

    public class IntRowStuffer : RowStuffer
    {
      public override Array Stuff(object o)
      {
        o = CheckValue(o);
        _i[0] = (int)o;
        return _i;
      }

      public override RowStuffer GetRowStuffer()
      {
        return new IntRowStuffer();
      }

      protected int[] _i = new int[1];
      protected override object TNull
      {
        get
        {
          return -99;
        }
      }
    }

    public class FloatRowStuffer : RowStuffer
    {
      public override Array Stuff(object o)
      {
        o = CheckValue(o);
        _f[0] = (float)o;
        return _f;
      }

      public override RowStuffer GetRowStuffer()
      {
        return new FloatRowStuffer();
      }

      protected float[] _f = new float[1];
      protected override object TNull
      {
        get
        {
          return Single.NaN;
        }
      }
    }

    public class LongRowStuffer : RowStuffer
    {
      public override Array Stuff(object o)
      {
        o = CheckValue(o);
        _l[0] = (long)o;
        return _l;
      }

      public override RowStuffer GetRowStuffer()
      {
        return new LongRowStuffer();
      }

      protected long[] _l = new long[1];
      protected override object TNull
      {
        get
        {
          return (long)-99;
        }
      }
    }

    public class DoubleRowStuffer : RowStuffer
    {
      public override Array Stuff(object o)
      {
        o = CheckValue(o);
        _d[0] = (double)o;
        return _d;
      }

      public override RowStuffer GetRowStuffer()
      {
        return new DoubleRowStuffer();
      }

      protected double[] _d = new double[1];
      protected override object TNull
      {
        get
        {
          return Double.NaN;
        }
      }
    }

    public class DecimalRowStuffer : RowStuffer
    {
      public override Array Stuff(object o)
      {
        o = CheckValue(o);
        _l[0] = Decimal.ToInt64((decimal)o);
        return _l;
      }

      public override RowStuffer GetRowStuffer()
      {
        return new DecimalRowStuffer();
      }

      protected long[] _l = new long[1];
      protected override object TNull
      {
        get
        {
          return (long)-99;
        }
      }
    }

    public class StringRowStuffer : RowStuffer
    {
      public StringRowStuffer() : this(-1)
      {
      }

      public StringRowStuffer(int arrayLength) : this(arrayLength, ' ', true)
      {
      }

      public StringRowStuffer(int arrayLength, char padChar, bool padLeft)
      {
        _arrayLength = arrayLength;
        _padChar = padChar;
        _padLeft = padLeft;
        _tnull = _arrayLength > 0 ? new string(_padChar, _arrayLength) : "";
      }

      public override Array Stuff(object o)
      {
        o = CheckValue(o);
        _s[0] = o == null ? "" : (string)o;
        return _s;
        /*
        String s = o == null ? "" : (String)o;
        if(s.Length < _arrayLength)
        {
          s = _padLeft ? s.PadLeft(_arrayLength, _padChar) : s.PadRight(_arrayLength, _padChar);
        }

        return s.ToCharArray();
        */
      }

      public override RowStuffer GetRowStuffer()
      {
        return new StringRowStuffer();
      }

      protected string[] _s = new string[1];
      protected bool _padLeft;
      protected char _padChar;
      protected int _arrayLength;
      protected object _tnull;
      protected override object TNull
      {
        get
        {
          return _tnull;
        }
      }
    }

    public static object GetDefaultNullValue(Type t)
    {
      return _defaultNullValues[t];
    }

    protected static Hashtable _rowStuffers;
    protected static Hashtable _defaultNullValues;

    static RowStuffer()
    {
            _rowStuffers = new DefaultValueHashtable(new DefaultRowStuffer())
            {
                [typeof(byte)] = new ByteRowStuffer(),
                [typeof(bool)] = new TrooleanRowStuffer(),
                [typeof(char)] = new CharRowStuffer(),
                [typeof(short)] = new ShortRowStuffer(),
                [typeof(int)] = new IntRowStuffer(),
                [typeof(float)] = new FloatRowStuffer(),
                [typeof(long)] = new LongRowStuffer(),
                [typeof(double)] = new DoubleRowStuffer(),
                [typeof(decimal)] = new DecimalRowStuffer(),
                [typeof(string)] = new StringRowStuffer()
            };

            _defaultNullValues = new Hashtable
            {
                [typeof(byte)] = (byte)0,
                [typeof(bool)] = new Troolean(false, true),
                [typeof(char)] = '\0',
                [typeof(short)] = (short)-99,
                [typeof(int)] = -99,
                [typeof(float)] = Single.NaN,
                [typeof(long)] = (long)-99,
                [typeof(double)] = Double.NaN,
                [typeof(decimal)] = (long)-99,
                [typeof(string)] = ""
            };
        }
  }
}
