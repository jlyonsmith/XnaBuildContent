using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Audio
{
	public class AudioFormat
	{
		public int AverageBytesPerSecond { get; set; }
		public int BitsPerSample { get; set; }
		public int Channels { get; set; }
		public int BlockAlign { get; set; }
		public int SampleRate { get; set; }
	}
}


