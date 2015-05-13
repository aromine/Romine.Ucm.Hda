using System;
using System.Collections.Generic;
using System.Text;

namespace Romine.Ucm.Hda.Data
{
    public class ResultSetColumn
    {
        public ResultSetColumn()
        {
            Type = DataType.NOT_SPECIFIED;
            Length = -1;
        }

        public string Name { get; set; }
        public DataType Type { get; set; }
        public int Length { get; set; }

        public enum DataType
        {
            BOOLEAN = 1, CHAR = 2, INT = 3, FLOAT = 4, DATE = 5, STRING = 6, BINARY = 7, NOT_SPECIFIED = -1
        }

        public static DataType ParseType(string value)
        {
            int v = int.Parse(value);
            return (ResultSetColumn.DataType)Enum.ToObject(typeof(ResultSetColumn.DataType), v);
        }
    }
    //EXTENTIONS IF WE COMPILE AGAINST 3.0
    /// <summary>
    /// Extends the DataType to parse from a string.
    /// </summary>
    public static class ResultSetColumnDataTypeExtensions
    {
        
    }
}
