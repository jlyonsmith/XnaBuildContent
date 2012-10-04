using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt; 
using XnaBuildContent;
using System.IO;

namespace XnaBuildContent.Compilers
{
	/// <summary>
	/// Strings to .xnb and .cs file compiler.
	/// 
	/// Supports the following properties:
	/// 
	/// Namespace - the namespace for the class.  Must be specified.
	/// ClassName - the name of the class. Optional; defaults to the file name plus 'Strings'.
	/// </summary>
    public class StringsToXnbAndCsCompiler : IContentCompiler
    {
        #region Classes
        class StringsContent
        {
            public class String
            {
                public string Name { get; set; }
                public string Value { get; set; }
                public int ArgCount { get; set; }
            }

            public string Namespace { get; set; }
            public string ClassName { get; set; }
            public List<StringsContent.String> Strings { get; set; }
        }

        #endregion
        
        #region IContentCompiler Members

        public string[] InputExtensions
        {
            get { return new string[] { ".strings" }; }
        }

        public string[] OutputExtensions
        {
            get { return new string[] { ".xnb", ".cs" }; }
        }

        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
        {
            ParsedPath stringsFileName = Target.InputFiles.Where(f => f.Extension == ".strings").First();
            ParsedPath xnbFileName = Target.OutputFiles.Where(f => f.Extension == ".xnb").First();
            ParsedPath csFileName = Target.OutputFiles.Where(f => f.Extension == ".cs").First();

			string className;

			Target.Properties.GetOptionalValue("ClassName", out className, stringsFileName.File + "Strings");

            StringsContent stringsData = CreateStringsData(className, StringsFileReaderV1.ReadFile(stringsFileName));

            string[] strings = stringsData.Strings.Select(s => s.Value).ToArray();

			if (!Directory.Exists(xnbFileName.VolumeAndDirectory))
				Directory.CreateDirectory(xnbFileName.VolumeAndDirectory);

            XnbFileWriterV5.WriteFile(strings, xnbFileName);

			if (!Directory.Exists(csFileName.VolumeAndDirectory))
				Directory.CreateDirectory(csFileName.VolumeAndDirectory);

            using (TextWriter writer = new StreamWriter(csFileName))
            {
                WriteCsOutput(writer, stringsData);
            }
        }

        #endregion

        private StringsContent CreateStringsData(string className, StringsFileV1 stringsFile)
        {
            StringsContent stringsData = new StringsContent();

            stringsData.ClassName = className;
            stringsData.Strings = new List<StringsContent.String>();

			string namespaceName;

			Target.Properties.GetRequiredValue("Namespace", out namespaceName);

            stringsData.Namespace = namespaceName;

            foreach (var s in stringsFile.Strings)
            {
                StringsContent.String d = new StringsContent.String();

                d.Name = s.Name;
                d.Value = s.Value;

                // Count the args in the string
                int n = 0;

                for (int i = 0; i < d.Value.Length - 1; i++)
                {
                    if (d.Value[i] == '{' && d.Value[i + 1] != '{')
                    {
                        n++;
                    }
                }

                d.ArgCount = n;

                stringsData.Strings.Add(d);
            }

            return stringsData;
        }

        private void WriteCsOutput(TextWriter writer, StringsContent stringsData)
        {
            writer.WriteLine("//");
            writer.WriteLine("// This file was generated on {0}.", DateTime.Now);
            writer.WriteLine("//");
            writer.WriteLine();
            writer.WriteLine("using System;");
            writer.WriteLine("");
            writer.WriteLine("namespace {0}", stringsData.Namespace);
            writer.WriteLine("{");
			writer.WriteLine("\tpublic class {0}", stringsData.ClassName);
			writer.WriteLine("\t{");
			writer.WriteLine("\t\tprivate string[] strings;");
			writer.WriteLine("");
			writer.WriteLine("\t\tpublic {0}(string[] strings)", stringsData.ClassName);
			writer.WriteLine("\t\t{");
			writer.WriteLine("\t\t\tthis.strings = strings;");
			writer.WriteLine("\t\t}");
			writer.WriteLine();

            for (int i = 0; i < stringsData.Strings.Count; i++)
            {
                StringsContent.String s = stringsData.Strings[i];

                if (s.ArgCount == 0)
                {
                    writer.WriteLine("\t\tpublic string {0} {{ get {{ return strings[{1}]; }} }}",
                        s.Name, i);
                }
                else
                {
                    StringBuilder sb1 = new StringBuilder();
                    StringBuilder sb2 = new StringBuilder();

                    for (int j = 0; j < s.ArgCount; j++)
                    {
                        sb1.Append("arg" + j.ToString());
                        sb2.Append("object arg" + j.ToString());

                        if (j < s.ArgCount - 1)
                        {
                            sb1.Append(", ");
                            sb2.Append(", ");
                        }
                    }

                    writer.WriteLine("\t\tpublic string {0}({1}) {{ return String.Format(strings[{2}], {3}); }}",
                        s.Name, sb2.ToString(), i, sb1.ToString());
                }
            }

            writer.WriteLine("\t}");
            writer.WriteLine("}");
        }
    }
}
