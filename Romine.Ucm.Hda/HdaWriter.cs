using System;
using System.Collections.Generic;
using System.Text;
using Romine.Ucm.Hda;
using Romine.Ucm.Hda.Data;
using System.IO;

namespace Romine.Ucm.Hda
{
    public class HdaWriter
    {
        private Data.IDataBinder binder;
        private String NewLine = System.Environment.NewLine;
        
        public HdaWriter()
        {

        }

        public HdaWriter(Data.IDataBinder binder)
        {
            this.binder = binder;
        }

        public IDataBinder Binder
        {
            get
            {
                return binder;
            }
            set
            {
                this.binder = value;
            }
        }

        /// <summary>
        /// Writes to the provided stream, and then closes the stream.
        /// </summary>
        /// <param name="stream"></param>
        public void WriteToStream(System.IO.Stream stream)
        {
            WriteToStream(stream, true);
        }

        protected internal void WriteTo(System.IO.TextWriter writer)
        {
            writer.NewLine = binder.NewLine;
            writer.Write(binder.PreHdaComments);
            writer.Write("<?hda");

            foreach (string key in binder.BinderInfo.Keys)
            {
                writer.Write(" ");
                string value = binder.BinderInfo[key];
                bool quote = value.Contains(" ");

                writer.Write(key);
                writer.Write("=");

                if (quote)
                {
                    writer.Write("\"");
                }
                writer.Write(value);
                if (quote)
                {
                    writer.Write("\"");
                }
            }
            writer.WriteLine("?>");

            foreach (string key in binder.Properties.Keys)
            {
                writer.Write(binder.Properties[key].Comments);
                writer.WriteLine("@Properties {0}", HdaReader<HdaDataBinder>.Encode(key));
                foreach (string ld in binder.Properties[key].Keys)
                {
                    writer.WriteLine("{0}={1}", HdaReader<HdaDataBinder>.Encode(ld), HdaReader<HdaDataBinder>.Encode(binder.LocalData[ld]));
                }
                writer.WriteLine("@end");
            }

            foreach (string key in binder.OptionLists.Keys)
            {
                writer.Write(binder.OptionLists[key].Comments);
                writer.WriteLine("@OptionList {0}", HdaReader<HdaDataBinder>.Encode(key));
                foreach (string item in binder.OptionLists[key])
                {
                    writer.WriteLine(HdaReader<HdaDataBinder>.Encode(item));
                }
                writer.WriteLine("@end");
            }

            foreach (string key in binder.ResultSets.Keys)
            {
                ResultSet rs = binder.ResultSets[key];
                writer.Write(rs.Comments);
                writer.WriteLine("@ResultSet {0}", HdaReader<HdaDataBinder>.Encode(key));
                int count = rs.Columns.Count;
                writer.WriteLine(count);

                foreach (ResultSetColumn column in rs.Columns)
                {
                    writer.Write(HdaReader<HdaDataBinder>.Encode(column.Name));
                    if (column.Type != ResultSetColumn.DataType.NOT_SPECIFIED)
                    {
                        writer.Write(' ');
                        writer.Write((int)column.Type);
                        if (column.Length > -1)
                        {
                            writer.Write(' ');
                            writer.Write(column.Length);
                        }
                    }
                    writer.WriteLine();

                }

                foreach (ResultSetRow row in binder.ResultSets[key].Rows)
                {

                    for (int j = 0; j < count; j++)
                    {
                        writer.WriteLine(HdaReader<HdaDataBinder>.Encode(row[row.Keys[j]]));
                    }

                }
                writer.WriteLine("@end");

            }
            writer.Write(binder.EndComments);
            if (binder.IncludeHdaFooter)
            {
                writer.WriteLine("<?hda?>");
            }
            //Just for fun, since everyone else is doing it...
            //writer.WriteLine();
            writer.Flush();
        }

        /// <summary>
        /// Writes to the provided stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="closeStream">Whether or not to close the provided stream.</param>
        public void WriteToStream(System.IO.Stream stream, bool closeStream)
        {
            if (stream.CanWrite)
            {
                StreamWriter writer = new StreamWriter(stream, binder.Encoding);
                WriteTo(writer);
                if (closeStream)
                {
                    writer.Close();
                }
            }
            else
            {
                throw new InvalidOperationException("Unable to write to stream.");
            }
        }

        public System.IO.MemoryStream ToMemoryStream()
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            WriteToStream(stream, false);
            stream.Position = 0;
            return stream;
        }
    }
}
