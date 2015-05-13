using System;
using System.Collections.Generic;
using System.Text;

namespace Romine.Ucm.Hda.Data
{
    /// <summary>
    /// Represents a result set
    /// </summary>
    public class ResultSet : Commentable
    {
        public ResultSet(int numberOfColumns)
        {
            Columns = new List<ResultSetColumn>(numberOfColumns);
            Rows = new List<ResultSetRow>();
        }

        public List<ResultSetColumn> Columns { get; set; }
        public List<ResultSetRow> Rows { get; set; }

        #region Commentable Members

        public string Comments { get; set; }
        
        #endregion
    }
}
