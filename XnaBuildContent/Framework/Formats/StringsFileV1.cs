using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XnaBuildContent
{
    public class StringsFileV1
    {
        public class String
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public IList<StringsFileV1.String> Strings { get; set; }
    }
}

