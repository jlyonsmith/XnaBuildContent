using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

#if WINDOWS

using Microsoft.Build.Framework;

namespace XnaBuildContent
{
    public class BuildContent : ITask
    {
        IBuildEngine buildEngine;
        ITaskHost taskHost;
        readonly string taskName = "BuildContent";

        [Required]
        public string ContentFile { get; set; }

        #region ITask Members

        public bool Execute()
        {
            BuildContentTool tool = new BuildContentTool(new MSBuildOutputter(buildEngine, taskName));

            tool.Parser.CommandName = taskName;
            tool.ContentFile = new ParsedPath(this.ContentFile, PathType.File);
            tool.NoLogo = true;

            try
            {
                tool.Execute();
            }
            catch (Exception e)
            {
                tool.Output.Error(e.Message);
            }

            return !tool.Output.HasOutputErrors;
        }

        public IBuildEngine BuildEngine
        {
            get
            {
                return buildEngine;
            }
            set
            {
                buildEngine = value;
            }
        }

        public ITaskHost HostObject
        {
            get
            {
                return taskHost;
            }
            set
            {
                this.taskHost = value;
            }
        }

        #endregion
    }
}

#endif // WINDOWS

