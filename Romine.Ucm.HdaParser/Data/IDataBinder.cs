using System;
using System.Collections.Generic;
using System.Text;

namespace Romine.Ucm.Hda.Data
{
    /// <summary>
    /// Represents a binder interface
    /// </summary>
    public interface IDataBinder
    {
        Properties LocalData { get; }
        IDictionary<string, Properties> Properties { get; }
        IDictionary<string, OptionList> OptionLists { get; }
        IDictionary<string, ResultSet> ResultSets { get; }
        IDictionary<string, string> BinderInfo { get; }
        IDictionary<string, string> FieldTypes { get; }
        IList<HdaFile> Files { get; }
        Encoding Encoding { get; set; }
        string Version { get; set; }
        System.Globalization.DateTimeFormatInfo DateTimeFormat { get; set; }
        string EndComments { get; set; }
        string PreHdaComments { get; set; }
        bool IncludeHdaFooter { get; set; }
        string NewLine { get; set; }
    }
}
