using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

namespace XnaBuildContent
{
    public interface IContentCompiler
    {
        string[] InputExtensions { get; }
        string[] OutputExtensions { get; }
        BuildContext Context { get; set; }
        BuildTarget Target { get; set; }
        void Compile();
    }
}

