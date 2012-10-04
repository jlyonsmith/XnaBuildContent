using System;
using System.Collections.Generic;
using ToolBelt; 
using XnaBuildContent;

namespace XnaBuildContent.Compilers
{
	public static class PinboardFileCache
	{
		private static Dictionary<ParsedPath, PinboardFileV1> pinboardFiles;

		public static PinboardFileV1 Load(ParsedPath pinboardFileName)
		{
			PinboardFileV1 data = null;
			
			if (pinboardFiles == null)
				pinboardFiles = new Dictionary<ParsedPath, PinboardFileV1>();
			
			if (pinboardFiles.TryGetValue(pinboardFileName, out data))
			{
				return data;
			}
			
			try
			{
				data = PinboardFileReaderV1.ReadFile(pinboardFileName);
			}
			catch
			{
				throw new ContentFileException("Unable to read pinboard file '{0}'".CultureFormat(pinboardFileName));
			}
			
			pinboardFiles.Add(pinboardFileName, data);
			
			return data;
		}
	}
}

