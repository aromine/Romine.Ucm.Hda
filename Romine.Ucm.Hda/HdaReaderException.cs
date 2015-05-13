using System;
using System.Collections.Generic;
using System.Text;

namespace Romine.Ucm.Hda
{
    /// <summary>
    /// Exception representing a parse exception
    /// </summary>
    public class HdaReaderException : Exception
    {
        public HdaReaderException()
        {

        }

        public HdaReaderException(string message)
            : base(message)
        {

        }

        public HdaReaderException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public HdaReaderException(string message, long line, long position) : this(message, null, line, position)
        {
            
        }

        public HdaReaderException(string message, Exception innerException, long line, long position)
            : this(message, innerException)
        {
            this.Line = line;
            this.Position = position;
        }

        public long Line
        {
            get;
            set;
        }

        public long Position
        {
            get;
            set;
        }
    }
}
