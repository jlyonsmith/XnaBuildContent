using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

namespace XnaBuildContent
{
    public class BuildTarget
    {
		public BuildTarget(int lineNumber, PropertyGroup targetProps, ItemGroup targetItems)
		{
			LineNumber = lineNumber;
			Properties = targetProps;
			Items = targetItems;
			Name = targetProps.GetRequiredValue("TargetName");
			InputFiles = targetItems.GetRequiredValue("TargetInputs");
			OutputFiles = targetItems.GetRequiredValue("TargetOutputs");

            Func<IList<ParsedPath>, IEnumerable<string>> extractAndSortExtensions = (files) =>
            {
                return files.Select<ParsedPath, string>(f => f.Extension).Distinct<string>().OrderBy(s => s, StringComparer.CurrentCultureIgnoreCase);
            };

            InputExtensions = extractAndSortExtensions(InputFiles);
            OutputExtensions = extractAndSortExtensions(OutputFiles);
		}

        public int LineNumber { get; private set; }
        public PropertyGroup Properties { get; private set; }
        public ItemGroup Items { get; private set; }
		public string Name { get; private set; }
		public IList<ParsedPath> InputFiles { get; private set; }
		public IEnumerable<string> InputExtensions { get; private set; }
		public IList<ParsedPath> OutputFiles { get; private set; }
		public IEnumerable<string> OutputExtensions { get; private set; }
    }
}
