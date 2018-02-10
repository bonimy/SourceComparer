using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nom.tam.fits;
using nom.tam.util;
using nom.tam.image;

namespace SourceComparer
{
    public static class FitsWriter
    {
        private static ColumnTable CreateColumnTable(ColumnList columnList, bool multithreaded, bool verbose)
        {
            if (columnList == null)
            {
                throw new ArgumentNullException(nameof(columnList));
            }

            var arrays = new object[columnList.Count];
            var sizes = new int[arrays.Length];
            if (multithreaded)
            {
                Parallel.For(0, arrays.Length, Iteration);
            }
            else
            {
                for (var i = 0; i < arrays.Length; i++)
                {
                    Iteration(i);
                }
            }

            return new ColumnTable(arrays, sizes);

            void Iteration(int index)
            {
                var (array, size) = GetColumnData(columnList[index]);
                arrays[index] = array;
                sizes[index] = size;
            }
        }

        public static void WriteFits(string path, SourceList sourceList, bool multithreaded, bool verbose)
        {
            if (sourceList == null)
            {
                throw new ArgumentNullException(nameof(sourceList));
            }

            var columnList = new ColumnList(sourceList, multithreaded, verbose);
            var columnTable = CreateColumnTable(columnList, multithreaded, verbose);
            var table = new BinaryTable(columnTable);
            var names = GetNames(sourceList);
            var header = BinaryTableHDU.ManufactureHeader(table);
            header.SetNaxis(2, sourceList.Count);
            var hdu = new BinaryTableHDU(header, table);
            var s = new BufferedDataStream(new FileStream(path, FileMode.Create));

            header.Write(s);

            s.Flush();
            s.Close();
        }

        private static string[] GetNames(SourceList sourceList)
        {
            if (sourceList == null)
            {
                throw new ArgumentNullException(nameof(sourceList));
            }

            var names = new string[sourceList.NameDictionary.Count];
            for (var i = 0; i < names.Length; i++)
            {
                names[i] = sourceList.NameDictionary.Keys[i];
            }

            return names;
        }

        private static (object array, int size) GetColumnData(Column column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            object objects;
            switch (column.Format)
            {
                case ColumnFormat.Integer:
                    objects = GetColumn(column, null, Int32.MinValue);
                    break;

                case ColumnFormat.String:
                    objects = GetColumn(column, null, String.Empty);
                    break;

                case ColumnFormat.Double:
                    objects = GetColumn(column, null, Double.NaN);
                    break;

                case ColumnFormat.Angle:
                    objects = GetColumn(column, x => ((Angle)x).Radians, Double.NaN);
                    break;

                case ColumnFormat.ModifiedJulianDate:
                    objects = GetColumn(
                        column,
                        x => ((JulianDate)(DateTime)x).ToModifiedJulianDate(),
                        Double.NaN);
                    break;

                default:
                    throw new Exception();
            }

            return (objects, column.Count);
        }

        private static T[] GetColumn<T>(Column column, Func<object, T> converter, T nul)
        {
            var objects = new T[column.Count];
            for (var i = 0; i < objects.Length; i++)
            {
                if (column[i] != null)
                {
                    if (converter == null)
                    {
                        objects[i] = (T)column[i];
                    }
                    else
                    {
                        objects[i] = converter(column[i]);
                    }
                }
                else
                {
                    objects[i] = nul;
                }
            }

            return objects;
        }
    }
}
