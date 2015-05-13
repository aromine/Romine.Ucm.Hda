using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Romine.Ucm.Hda.Data
{
    /// <summary>
    /// A file object... not sure if implementation needs to be different, just theory right now
    /// </summary>
    public class HdaFile
    {
        public HdaFile(string fileName, Stream file)
        {
            FileName = fileName;
            Stream = file;
            ContentType = "application/octet-stream";
        }

        public HdaFile(FileInfo file)
            : this(file.Name, file)
        {

        }

        public HdaFile(string fileName, FileInfo file)
        {
            FileName = fileName;
            Stream = file.Open(FileMode.Open);
            ContentLength = file.Length;
            ContentType = "application/octet-stream";
        }

        public Stream Stream { get; set; }

        public string FileName { get; set; }

        public long ContentLength { get; set; }

        public string ContentType { get; set; }

    }
}
