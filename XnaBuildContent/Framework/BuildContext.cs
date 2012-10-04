using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using ToolBelt;

namespace XnaBuildContent
{
    public class BuildContext
    {
        public BuildContext(OutputHelper output, ParsedPath contentFile)
        {
            this.Output = output;
            this.ContentFile = contentFile;
        }

        public ParsedPath ContentFile { get; set; }
        public OutputHelper Output { get; private set; }
		public ItemGroup Items { get; private set; }
    }
}

