using System;
using System.Collections.Generic;
using System.Text;

namespace Romine.Ucm.Hda.Data
{
    /// <summary>
    /// Represents properties (name value pair) for LocalData
    /// </summary>
    public class Properties : Dictionary<string, string>, Commentable
    {
        public Properties()
            : base()
        {

        }

        new public string this[string key]
        {
            get
            {
                string returnValue = null;
                if (this.ContainsKey(key))
                {
                    returnValue = base[key];
                }
                return returnValue;
            }
            set
            {
                base[key] = value;
            }
        }

        #region Commentable Members

        public string Comments { get; set; }

        #endregion
    }
}
