using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

namespace XnaBuildContent
{
    public class ContentFileException : Exception 
    {
		public ContentFileException(string message)
			: base(message)
		{
		}
		
		public ContentFileException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
		
		public ContentFileException(ParsedPath fileName, int lineNumber) : base()
        {
            EnsureFileNameAndLineNumber(fileName, lineNumber);
        }

        public ContentFileException(ParsedPath fileName, int lineNumber, string message)
            : base(message)
        {
            EnsureFileNameAndLineNumber(fileName, lineNumber);
        }

        public ContentFileException(ParsedPath fileName, int lineNumber, string message, Exception innerException)
            : base(message, innerException)
        {
            EnsureFileNameAndLineNumber(fileName, lineNumber);
        }

        public ContentFileException(ParsedPath fileName, int lineNumber, Exception innerException)
            : base(innerException.Message, innerException)
        {
            EnsureFileNameAndLineNumber(fileName, lineNumber);
        }

        public bool HasFileName { get { return this.Data.Contains("FileName"); } }
        public bool HasLineNumber { get { return this.Data.Contains("LineNumber"); } }

        public ParsedPath FileName
        {
            get
            {
                return (ParsedPath)this.Data["FileName"];
            }

            set
            {
                this.Data["FileName"] = value;
            }
        }

        public int LineNumber
        {
            get
            {
                return (int)this.Data["LineNumber"];
            }

            set
            {
                this.Data["LineNumber"] = value;
            }
        }

        public void EnsureFileNameAndLineNumber(ParsedPath fileName, int lineNumber)
        {
            if (!HasFileName)
                this.FileName = fileName;
            
            if (!HasLineNumber)
                this.LineNumber = lineNumber;
        }
    }
}
