using System;
using System.Collections.Generic;
using System.IO;
using Romine.Ucm.Hda;

namespace Romine.Ucm.Hda.Data
{
    /// <summary>
    /// An implementation of the binder
    /// </summary>
    public class HdaDataBinder : IDataBinder
    {
        public HdaDataBinder()
        {
            
            OptionLists = new Dictionary<string, OptionList>();
            ResultSets = new Dictionary<string, ResultSet>();
            FieldTypes = new Dictionary<string, string>();
            Properties = new Dictionary<string, Properties>();
            Files = new List<HdaFile>();
            BinderInfo = new Dictionary<string, string>();
            Encoding = System.Text.Encoding.UTF8; //We'll default to UTF-8 since that's 10gR3+'s default.
        }
        System.Text.Encoding _encoding;
        public IDictionary<string, Properties> Properties { get; private set; }
        public Properties LocalData
        {
            get
            {
                if (Properties.ContainsKey("LocalData"))
                {
                    return Properties["LocalData"];
                }
                else
                {
                    return null;
                    //return new Properties();
                }
            }
        }
        public IDictionary<string, OptionList> OptionLists { get; private set; }
        public IDictionary<string, ResultSet> ResultSets { get; private set; }
        public IDictionary<string, string> BinderInfo { get; private set; }
        public IDictionary<string, string> FieldTypes { get; private set; }
        public System.Globalization.DateTimeFormatInfo DateTimeFormat { get; set; }        
        public string Version { get; set; }
        public IList<HdaFile> Files { get; private set; }
        public string EndComments { get; set; }
        public string PreHdaComments { get; set; }
        public string NewLine { get; set; }
        public bool IncludeHdaFooter { get; set; }

        public System.Text.Encoding Encoding
        {
            get
            {
                return _encoding;
            }
            set
            {
                _encoding = value;
                if (_encoding != null)
                {
                    //Update the binder info
                    BinderInfo["encoding"] = _encoding.WebName;
                }
            }
        }

        /// <summary>
        /// Returns a DataBinder string, including all header info.
        /// </summary>
        /// <returns>A string representation of a DataBinder</returns>
        public override string ToString()
        {
            StringWriter writer = new StringWriter();
            new HdaWriter(this).WriteTo(writer);
            return writer.ToString();
        }
    }
}
