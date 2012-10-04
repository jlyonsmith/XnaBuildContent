using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

namespace XnaBuildContent
{
    public class ContentFileV2
    {
		public class Item
		{
			public string Name { get; set; }
			public string Include { get; set; }
			public string Exclude { get; set; }
		}

        public class Target
        {
			public string Name { get; set; }
            public int LineNumber { get; set; }
			public string Inputs { get; set; }
			public string Outputs { get; set; }
            public List<Tuple<string, string>> Properties { get; set; }
        }

        public List<ContentFileV2.Item> Items { get; set; }
		public List<Tuple<string, string>> Properties { get; set; }
        public List<ContentFileV2.Target> Targets { get; set; }
    }
}

