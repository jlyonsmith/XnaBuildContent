using System;

namespace XnaBuildContent
{
	public class WavFile
	{
		public ushort AudioFormat { get; set; }
		public ushort NumChannels { get; set; }
		public uint SampleRate { get; set; }
		public uint ByteRate { get; set; }
		public ushort BlockAlign { get; set; }
		public ushort BitsPerSample { get; set; }
		public byte[] Data { get; set; }
	}
}


