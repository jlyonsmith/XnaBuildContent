using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ToolBelt;
using XnaBuildContent;
using System.Xml;

namespace BuildContent
{
    class Program
    {
        public static int Main(string[] args)
        {
            BuildContentTool tool = new BuildContentTool(new ConsoleOutputter());

            try
            {
				((IProcessCommandLine)tool).ProcessCommandLine(args);

				tool.Execute();
            }
            catch (Exception e)
            {
                tool.Output.Error(e.Message);
            }

            return tool.ExitCode;
        }
    }
}

