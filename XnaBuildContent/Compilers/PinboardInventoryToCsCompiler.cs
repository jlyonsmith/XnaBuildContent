using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt; 
using XnaBuildContent;
using System.IO;
using System.Xml;

namespace XnaBuildContent.Compilers
{
    public class PinboardInventoryToCsCompiler : IContentCompiler
    {
        #region Classes
        private class RectanglesContent
        {
            public class Class
            {
                public string ClassNamePrefix { get; set; }
                public List<string> RectangleNames { get; set; }
            }

            public string Namespace { get; set; }
            public List<Class> Classes { get; set; }
        }

        private class FileNameEqualityComparer : IEqualityComparer<ParsedPath>
        {
            #region IEqualityComparer<ParsedPath> Members

            public bool Equals(ParsedPath x, ParsedPath y)
            {
                return String.Compare(x.File.ToString(), y.File.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0;
            }

            public int GetHashCode(ParsedPath path)
            {
                return path.File.GetHashCode();
            }

            #endregion
        }

        #endregion
        
        #region IContentCompiler Members

        public string[] InputExtensions { get { return new string[] { ".pinboard" }; } }
        public string[] OutputExtensions { get { return new string[] { ".cs" }; } }
        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
        {
            IEnumerable<ParsedPath> pinboardFileNames = Target.InputFiles.Where(f => f.Extension == ".pinboard");
            IEnumerable<ParsedPath> distinctPinboardFileNames = pinboardFileNames.Distinct<ParsedPath>(new FileNameEqualityComparer());

            Dictionary<ParsedPath, PinboardFileV1> pinboards = ReadPinboardFiles(pinboardFileNames);

            ReconcilePinboards(pinboardFileNames, distinctPinboardFileNames, pinboards);

            TextWriter writer;
            ParsedPath csFileName = Target.OutputFiles.Where(f => f.Extension == ".cs").First();

            Context.Output.Message(MessageImportance.Low, "Writing output file '{0}'", csFileName);

			if (!Directory.Exists(csFileName.VolumeAndDirectory))
				Directory.CreateDirectory(csFileName.VolumeAndDirectory);

            using (writer = new StreamWriter(csFileName, false, Encoding.UTF8))
            {
                RectanglesContent rectangleData = CreateRectangleData(distinctPinboardFileNames, pinboards);

                WriteCsOutput(writer, rectangleData);
            }
        }

        #endregion

        #region Methods

        private RectanglesContent CreateRectangleData(IEnumerable<ParsedPath> distinctPinboardFiles, Dictionary<ParsedPath, PinboardFileV1> pinboards)
        {
            RectanglesContent rectData = new RectanglesContent();

			rectData.Namespace = Target.Properties.GetRequiredValue("Namespace");
            rectData.Classes = new List<RectanglesContent.Class>();

            foreach (var pinboardFile in distinctPinboardFiles)
            {
                PinboardFileV1 pinboard = pinboards[pinboardFile];

                RectanglesContent.Class rectClass = new RectanglesContent.Class();

                rectClass.ClassNamePrefix = pinboardFile.File;

                List<string> names = new List<string>();

                names.Add("Screen");

                foreach (var rectInfo in pinboard.RectInfos)
                {
                    names.Add(rectInfo.Name);
                }

                rectClass.RectangleNames = names;

                rectData.Classes.Add(rectClass);
            }

            return rectData;
        }

        private void WriteCsOutput(TextWriter writer, RectanglesContent rectangleData)
		{
			writer.WriteLine("//");
			writer.WriteLine("// This file was generated on {0}.", DateTime.Now);
			writer.WriteLine("//");
			writer.WriteLine();
			writer.WriteLine("using System;");
			writer.WriteLine("using Microsoft.Xna.Framework;");
			writer.WriteLine("using Microsoft.Xna.Framework.Graphics;");
			writer.WriteLine("using System.Text;");
			writer.WriteLine("");
			writer.WriteLine("namespace {0}", rectangleData.Namespace);
			writer.WriteLine("{");

			for (int i = 0; i < rectangleData.Classes.Count; i++)
			{
				RectanglesContent.Class classData = rectangleData.Classes[i];

				writer.WriteLine("\tpublic class {0}Rectangles", classData.ClassNamePrefix);
				writer.WriteLine("\t{");

				writer.WriteLine("\t\tprivate Rectangle[] rectangles;");
				writer.WriteLine();
				writer.WriteLine("\t\tpublic {0}Rectangles(Rectangle[] rectangles)", classData.ClassNamePrefix);
				writer.WriteLine("\t\t{");
				writer.WriteLine("\t\t\tthis.rectangles = rectangles;");
				writer.WriteLine("\t\t}");
				writer.WriteLine();

				for (int j = 0; j < classData.RectangleNames.Count; j++)
				{
					writer.WriteLine("\t\tpublic Rectangle {0} {{ get {{ return rectangles[{1}]; }} }}",
                        classData.RectangleNames[j],
                        j);
				}

				writer.WriteLine("\t}");
				writer.WriteLine();
			}

			writer.WriteLine("\tpublic static class Rectangles");
			writer.WriteLine("\t{");

			for (int i = 0; i < rectangleData.Classes.Count; i++)
			{
				writer.WriteLine("\t\tpublic static {0}Rectangles {0} {{ get; set; }}", rectangleData.Classes[i].ClassNamePrefix);
			}

			writer.WriteLine("\t}");
            writer.WriteLine("}");
        }

        private Dictionary<ParsedPath, PinboardFileV1> ReadPinboardFiles(IEnumerable<ParsedPath> pinboardFiles)
        {
            Dictionary<ParsedPath, PinboardFileV1> pinboards = new Dictionary<ParsedPath, PinboardFileV1>();

            foreach (var pinboardFile in pinboardFiles)
            {
                Context.Output.Message(MessageImportance.Low, "Reading pinboard file '{0}'", pinboardFile);

                PinboardFileV1 pinboard = null;

                try
                {
                    pinboard = PinboardFileReaderV1.ReadFile(pinboardFile);
                }
                catch (Exception e)
                {
                    throw new ContentFileException(Context.ContentFile, Target.LineNumber, String.Format("Unable to read pinboard file '{0}'", pinboardFile), e);
                }

                pinboards.Add(pinboardFile, pinboard);
            }

            return pinboards;
        }

        private void ReconcilePinboards(
            IEnumerable<ParsedPath> pinboardFiles, 
            IEnumerable<ParsedPath> distinctPinboardFiles, 
            Dictionary<ParsedPath, PinboardFileV1> pinboards)
        {
            foreach (var distinctPinboardFile in distinctPinboardFiles)
            {
                PinboardFileV1 goldPinboard = null;
                ParsedPath goldPinboardFile = null;

                foreach (var pinboardFile in pinboardFiles.Where(p => p.File == distinctPinboardFile.File))
                {
                    if (goldPinboard == null)
                    {
                        goldPinboard = pinboards[pinboardFile]; 
                        goldPinboardFile = pinboardFile;
                    }
                    else
                    {
                        PinboardFileV1 pinboard = pinboards[pinboardFile];

                        if (goldPinboard.RectInfos.Count != pinboard.RectInfos.Count)
                        {
                            throw new ContentFileException(Context.ContentFile, Target.LineNumber, string.Format("Pinboard '{0}' and '{1}' have a different number of rectangles",
                                goldPinboardFile, pinboardFile));
                        }

                        for (int i = 0; i < goldPinboard.RectInfos.Count; i++)
                        {
                            if (goldPinboard.RectInfos[i].Name != pinboard.RectInfos[i].Name)
                            {
                                throw new ContentFileException(Context.ContentFile, Target.LineNumber, string.Format("RectangleInfo named {0} at depth {1} in pinboard '{2}' is different from pinboard '{3}'",
                                    goldPinboard.RectInfos[i].Name, i, goldPinboardFile, pinboardFile));
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
