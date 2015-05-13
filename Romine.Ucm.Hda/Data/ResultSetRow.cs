using System;
using System.Collections.Generic;
using System.Text;

namespace Romine.Ucm.Hda.Data
{
    /// <summary>
    /// Represents a logical Resulset Row.
    /// </summary>
    public class ResultSetRow : List<string>
    {
        List<ResultSetColumn> _columns = null;

        public ResultSetRow(List<ResultSetColumn> columns)
        {
            _columns = columns;
            //_fields = new string[columns.Count];
        }

        public void AddField(string field)
        {
            if (this.Count > _columns.Count)
            {
                throw new InvalidOperationException(string.Format("Field index was out of range.  ResultSet has {0} columns", _columns.Count));
            }
            else
            {
                this.Add(field);
            }
        }

        public List<ResultSetColumn> Keys
        {
            get
            {
                return _columns;
            }
        }

        public string this[ResultSetColumn key]
        {
            get
            {
                int index = _columns.IndexOf(key);
                if (index > -1)
                {
                    return this[index];
                }
                return null;
            }
            set
            {
                int index = _columns.IndexOf(key);
                if (index > -1)
                {
                    this[index] = value;
                }
            }
        }
    }
}
