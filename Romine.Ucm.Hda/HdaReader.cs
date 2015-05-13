using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using Romine.Ucm.Hda.Data;

namespace Romine.Ucm.Hda
{
    /// <summary>
    /// Provides a default implementation that uses the HdaDataBinder.  The generic class can be used to allow extensible binders.
    /// </summary>
    public class HdaReader : HdaReader<HdaDataBinder>
    {
        public HdaReader() : base() { }
        public HdaReader(Encoding encoding) : base(encoding) { }
    }

    public class HdaReader<T> where T : IDataBinder, new() 
    {
        //const string BEGIN_HDA_NO_HEADER = "@Prop";
        const string BEGIN_HDA = "<?hda";
        const string END_HDA_HEADER = "?>";
        const string HDA_END_SEC = "@end";
        const string HDA_75_ENCODING = "iso-8859-1";
        const string HDA_PROPERTIES = "@Properties";
        const string HDA_OPTIONLIST = "@OptionList";
        const string HDA_RESULTSET = "@ResultSet";
        const string HDA_FIELD_TYPES = "blFieldTypes";
        const string HDA_DATE_FORMAT = "blDateFormat";
        const string END_HDA_FOOTER = BEGIN_HDA + END_HDA_HEADER;

        static Regex HDA_HEADER_REGEX = new Regex("(\\S+)=[\"']?((?:.(?![\"']?\\s+(?:\\S+)=|[>\"']))+.)[\"']?");

        TextReader _reader;

        StringBuilder _comments = new StringBuilder();

        T _binder;

        long _line;

        public long Line
        {
            get
            {
                return _line;
            }
        }

        public HdaReader() : this(Encoding.UTF8)
        {

        }

        public HdaReader(Encoding encoding)
        {
            DefaultEncoding = encoding;
        }

        protected void ReadHeaderFromString(string hdaStr)
        {
            _reader = new StringReader(hdaStr);
            string strLine = null;
            bool shouldContinue = true;
            while (shouldContinue && (strLine = GetNextLine()) != null)
            {
                if (strLine == null)
                {
                    throw new HdaReaderException("Invalid HDA file.  Unable to find HDA start.");
                }

                if (strLine.StartsWith(BEGIN_HDA))
                {
                    ReadHdaHeader(strLine);
                    shouldContinue = false;
                }
                else if (strLine.StartsWith(HDA_PROPERTIES) || strLine.StartsWith(HDA_OPTIONLIST) || strLine.StartsWith(HDA_RESULTSET))
                {
                    _binder.Encoding = DefaultEncoding;
                    shouldContinue = false;
                }
                else
                {
                    ReadComment(strLine);
                    //throw new HdaReadException("Invalid HDA file.  Unable to find HDA start.");
                    //TODO determine: Is this valid HDA?  Should we disgard blank and/or garbled lines?
                    //TODO follow-up - Is it RIGHT? to do what we are doing.
                }
            }
        }

        public Encoding DefaultEncoding
        {
            get;
            set;
        }

        protected void ReadHeaderFromStream(Stream stream)
        {
            bool continueRead = true;
            List<byte> bytes = new List<byte>();
            int commentLines = 0;
            do
            {
                int b = stream.ReadByte();
                //TODO BOY OH BOY, we need to control endlines better CR, CR+LF, LF... What to do
                switch (b)
                {
                    case '\n':
                        if (bytes.Count == 0)
                        { //If we have bytes, then we've started reading the header, this is the end character...
                            commentLines++; //TODO validate this - \n will always exist, so either \n or \n\r
                        }
                        goto case '\r';
                    case '\r':
                        if (bytes.Count != 0)
                        {
                            //bytes.Add(Convert.ToByte(b)); //Why add a newline?  We're done at this point.
                            continueRead = false;
                        }
                        //else
                        //{
                        //    _comments.Append(Convert.ToChar(b));
                        //}
                        break;
                    case -1:
                        throw new HdaReaderException("Premature end of file while reading the header.",0,stream.Position);
                    case '\0':
                        break;//We should be parsing ASCII in the header... Ignore double byte characters, and trust that the idiot using the file isn't an idiot.
                    case '<':
                    case '@':
                        //detects the start of HDA - this parser can not support these characters before the header.
                        bytes.Add(Convert.ToByte(b));
                        break;
                    default:
                        if (bytes.Count != 0)
                        {//Invalid bytes
                        //    _comments.Append(Convert.ToChar(b));
                        //}
                        //else
                        //{ //If we're actually in the header, 
                            bytes.Add(Convert.ToByte(b));
                        }
                        break;
                }

            } while (continueRead); //Do not change me to string.IsNullOrEmpty (if we're at the end of the stream, this would result in an infinite loop!
            //We should probably make this recrusive.
            //We should probably take a look and see if UTF8 is going to be solid for reading this.
            
            string strLine = Encoding.ASCII.GetString(bytes.ToArray()).Trim();
            //Reset stream
            
            if (strLine.StartsWith(BEGIN_HDA))
            {
                ReadHdaHeader(strLine);
                
                stream.Position = 0;
                _reader = new StreamReader(stream, _binder.Encoding);
                if (commentLines > 0)
                {
                    for (int i = 0; i < commentLines; i++)
                    {
                        ReadComment(GetNextLine());
                    }
                }
                GetNextLine(); //read past first line TODO: Sync this up with any number of blank lines to start
            }
            else if (strLine.StartsWith(HDA_PROPERTIES) || strLine.StartsWith(HDA_OPTIONLIST) || strLine.StartsWith(HDA_RESULTSET))
            {
                
                //if (_binder.Encoding == null) //If we haven't already set the encoding, default to UTF-8
                //{
                    _binder.Encoding = Encoding.UTF8; //TODO Assume UTF-8?  Is this ok?
                    //_binder.BinderInfo["encoding"] = _binder.Encoding.WebName;
                //}
                stream.Position = 0;
                _reader = new StreamReader(stream, _binder.Encoding);
            }
            else
            {
                throw new HdaReaderException("Invalid HDA file.  Unable to find HDA start.");
                //TODO determine: Is this valid HDA?  Should we disgard blank and/or garbled lines?
            }
            
        }

        public virtual T Read(System.IO.Stream stream)
        {
            _binder = new T();
            if (stream.CanRead)
            {
                if (!stream.CanSeek)
                {
                    MemoryStream mstream = new MemoryStream();
                    int mbyte = stream.ReadByte();
                    while (mbyte != -1)
                    {
                        //According to stuff I've read, this should be valid.
                        mstream.WriteByte(Convert.ToByte(mbyte));
                        mbyte = stream.ReadByte();
                    }
                    stream.Close(); //TODO Determine if this is a best practice
                    stream = mstream;
                    stream.Position = 0;
                }
                //_reader = new StreamReader(stream, System.Text.Encoding.ASCII);
                //ParseHeader now has the responsibility to frame the reader
                ReadHeaderFromStream(stream);
                ReadHda();
                ReadFieldInfo();
                _reader.Close();
                _reader = null; //Clean up.
                return _binder;
            }
            else
            {
                throw new InvalidOperationException("HDA Stream must support reading");
            }
        }
        
        public virtual T Read(string hdaString)
        {
            _binder = new T();
            if (!string.IsNullOrEmpty(hdaString))
            {
                //_reader = new StreamReader(stream, System.Text.Encoding.ASCII);
                //ParseHeader now has the responsibility to frame the reader
                ReadHeaderFromString(hdaString);
                ReadHda();
                ReadFieldInfo();
                _reader.Close();
                _reader = null; //Clean up.
                return _binder;
            }
            else
            {
                throw new InvalidOperationException("Cannot parse a null or empty string.");
            }
        }

        private string GetNextLine()
        {
            _line++;
            return _reader.ReadLine();
        }

        protected void ReadFieldInfo()
        {
            if (_binder.LocalData != null)
            {
                if(_binder.LocalData.ContainsKey(HDA_FIELD_TYPES))
                {
                    string blFieldTypes = _binder.LocalData[HDA_FIELD_TYPES];
                    string[] fieldTypes = blFieldTypes.Split(',');
                    foreach (string fieldTypeDef in fieldTypes)
                    {
                        if (fieldTypeDef != string.Empty)
                        {
                            string[] fieldType = fieldTypeDef.Split(' ');
                            if (fieldType.Length == 2)
                            {
                                _binder.FieldTypes.Add(fieldType[0], fieldType[1]);
                            }
                            else
                            {
                                throw new HdaReaderException("blFieldTypes property was invalid.");
                            }
                        }
                    }
                }
                
                if (_binder.LocalData.ContainsKey(HDA_DATE_FORMAT))
                {
                    string dateFormat = _binder.LocalData[HDA_DATE_FORMAT];
                    if (!string.IsNullOrEmpty(dateFormat))
                    {
                        System.Globalization.DateTimeFormatInfo format = new System.Globalization.DateTimeFormatInfo();
                        format.FullDateTimePattern = dateFormat;
                        _binder.DateTimeFormat = format;
                    }
                }
                //That's all folks!  Not sure what to do with TODO: refreshSubjects, refreshMonikers, changedSubjects, etc
            }
        }

        protected void ReadHda()
        {
            string line = null;
            bool continueParsing = true;
            while (continueParsing && (line = GetNextLine()) != null)
            {
                //line = line.Trim(); //Why did we do this?
                //if (line == string.Empty)
                //{
                //    continue;//Ignore blank lines?
                //} //We'll stop doing this since we now support comments...

                if (string.Empty != line && line[0] == '@')
                {
                    int sectionIndex = line.IndexOf(' '); //TODO Invalid check
                    if (sectionIndex > 0) //We'll pass this lower
                    {
                        string sectionType = line.Substring(0, sectionIndex);
                        switch (sectionType)
                        {
                            case HDA_PROPERTIES:
                                ReadProperties(line);
                                break;
                            case HDA_OPTIONLIST:
                                ReadOptionList(line);
                                break;
                            case HDA_RESULTSET:
                                ReadResultSet(line);
                                break;
                            default:
                                throw new HdaReaderException("Invalid or unknown section type, or data outside a data set.", Line, 0);
                        }
                    }
                    else
                    {
                        throw new HdaReaderException("A section must have a name.", Line, line.Length);
                    }
                }
                else if (line == END_HDA_FOOTER)
                {
                    //What to do what to do... probably stop parsing
                    continueParsing = false;
                    _binder.IncludeHdaFooter = true;
                }
                else
                {
                    ReadComment(line);
                }
            }
            //And get the binder end comments
            _binder.EndComments = _comments.ToString();
            _comments.Length = 0;
        }

        private void ReadComment(string line)
        {
            if (ValidateComment(line))
            {
                _comments.AppendLine(line);
            }
            else
            {
                throw new HdaReaderException("Invalid content or comment.", Line, 0);
            }
        }

        private bool ValidateComment(string line)
        {
            //TODO: Find out if we need to do anything to validate comments...
            return true;
        }

        static string GetSectionName(string sectionStartLine)
        {
            return sectionStartLine.Substring(sectionStartLine.IndexOf(' ')).Trim();
        }

        private void ConsumeComments(Commentable commentable)
        {
            commentable.Comments = _comments.ToString();
            _comments.Length = 0;
        }

        protected virtual void ReadProperties(string line)
        {
            string name = GetSectionName(line);
            
            Properties props = new Properties();
            ConsumeComments(props);
            
            while ((line = GetNextLine()) != HDA_END_SEC)
            {
                line = HdaStringUtils.Decode(line);
                int index = line.IndexOf('=');
                if(index < 0)
                {
                    throw new HdaReaderException("Line must be property/value pair delimited by '='.", Line, line.Length);
                }
                string key = line.Substring(0, index),
                    value = line.Substring(index+1);
                props.Add(key, value);
            }
            _binder.Properties[name] = props;
            
            //}// Amazingly I found @Properties Environment
            //else
            //{
             //   throw new HdaParseException("This parser only supports LocalData as properties.", Line, line.IndexOf(name));
            //}
        }

        protected virtual void ReadOptionList(string line)
        {
            string name = GetSectionName(line);
            OptionList ol = new OptionList(name);
            ConsumeComments(ol);
            while ((line = GetNextLine()) != HDA_END_SEC)
            {
                ol.Add(HdaStringUtils.Decode(line));
            }
            _binder.OptionLists.Add(name, ol);
        }

        protected virtual void ReadResultSet(string line)
        {
            string name = GetSectionName(line);
            int numberOfColumns = 0;
            if (!int.TryParse(GetNextLine(), out numberOfColumns))
            {
                throw new HdaReaderException("Unable to read Result Set Column Count", Line, 0);
            }
            ResultSet rs = new ResultSet(numberOfColumns);
            ConsumeComments(rs);
            int i = 0;
            _binder.ResultSets.Add(name, rs);
            while ((line = GetNextLine()) != HDA_END_SEC && line != null)
            {
                line = line.Trim();
                if (i < numberOfColumns)
                {
                    i++;
                    
                    ResultSetColumn column = new ResultSetColumn();
                    
                    int index = line.IndexOf(' ');
                    if (index > -1)
                    {
                        try
                        {
                            string[] columndata = line.Substring(index+1).Split(' ');
                            column.Type = ResultSetColumn.ParseType(columndata[0].Trim());
                            if (columndata.Length == 2)
                            {
                                column.Length = int.Parse(columndata[1].Trim());
                            }
                            line = line.Substring(0, line.IndexOf(' '));
                        }
                        catch(Exception ex)
                        {
                            throw new HdaReaderException("Invalid column type or length", ex, Line, line.Length); //TODO see if we can tell what character threw it
                        }
                    }
                    column.Name = line;
                    rs.Columns.Add(column);
                }
                else
                {
                    ResultSetRow row = ReadResultSetRow(rs.Columns, line);
                    //ResultSetRow row = new ResultSetRow(rs.Columns);
                    rs.Rows.Add(row);
                }
                if (line == HDA_END_SEC)
                {
                    break;
                }
            }
        }

        protected ResultSetRow ReadResultSetRow(List<ResultSetColumn> columns, string line)
        {
            ResultSetRow row = new ResultSetRow(columns);

            int j = 1;
            try
            {
                row.AddField(HdaStringUtils.Decode(line));

                //row.AddField(line);

                while (j < columns.Count)
                {
                    j++;
                    line = HdaStringUtils.Decode(GetNextLine());
                    if (line == HDA_END_SEC)
                    {
                        break;
                    }
                    else
                    {
                        row.AddField(line);
                        //row.AddField(line.Replace("\n", "").Replace("\r", ""));
                    }
                }
            }
            catch (InvalidOperationException ioe)
            {
                throw new HdaReaderException("Error parsing ResultSet row.", ioe, Line, 0);
            }

            return row;
        }
        //TODO Review this method and pattern it after other examples of this header parsing (if they are more robust)
        protected virtual void ReadHdaHeader(string header)
        {
            _binder.BinderInfo.Clear();
            MatchCollection matches = HDA_HEADER_REGEX.Matches(header.TrimStart(BEGIN_HDA.ToCharArray()).TrimEnd(END_HDA_HEADER.ToCharArray()));
            _binder.PreHdaComments = _comments.ToString();
            _comments.Length = 0;

            foreach (Match match in matches)
            {
                if (match.Groups.Count == 3) //We need the root match, name, and value
                {
                    string name = match.Groups[1].Value,
                        value = match.Groups[2].Value;
                    _binder.BinderInfo[name] = value;
                }
                else
                {
                    throw new HdaReaderException("Invalid header in HDA File",1,0);
                }
            }


            if (_binder.BinderInfo.ContainsKey("version"))
            {
                _binder.Version = _binder.BinderInfo["version"];
            }

            if (_binder.BinderInfo.ContainsKey("encoding"))
            {
                try
                {
                    Encoding = _binder.Encoding = Encoding.GetEncoding(_binder.BinderInfo["encoding"]);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message, "ERROR");
                    //TODO: LOG HERE
                    //Assume that encoding is UTF 8 if version is 10gR3 or ISO-8859
                    _binder.Encoding = (Version ?? string.Empty).StartsWith("10") ? Encoding.UTF8 : Encoding.GetEncoding(HDA_75_ENCODING);
                }
            }
        }

        public Dictionary<string, string> dictionary = new Dictionary<string, string>();
        public string Version { get; set; }
        public System.Text.Encoding Encoding { get; set; }
        //public IDictionary<string, string> HeaderInfo { get; set; }

        
    }
}
