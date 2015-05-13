using System;
using System.Collections.Generic;
using System.Text;

namespace Romine.Ucm.Hda.Data
{
    /// <summary>
    /// Represents an option list
    /// </summary>
    public class OptionList : List<string>, Commentable
    {
        public OptionList(string name)
        {
            Name = name;
        }
        public string Name { get; set; }

        #region Commentable Members

        public string Comments { get; set; }

        #endregion
    }
}
