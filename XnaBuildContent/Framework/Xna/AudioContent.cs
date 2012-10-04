using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;
using XnaBuildContent;

namespace Microsoft.Xna.Framework.Content.Pipeline.Audio
{
	public class AudioContent
	{
		public AudioContent(WavFile wavFile)
		{
			this.FileType = AudioFileType.Wav;
			this.AudioFormat = new AudioFormat();
			this.AudioFormat.Channels = wavFile.NumChannels;
			this.AudioFormat.SampleRate = (int)wavFile.SampleRate;
			this.AudioFormat.AverageBytesPerSecond = (int)wavFile.ByteRate;
			this.AudioFormat.BlockAlign = wavFile.BlockAlign;
			this.AudioFormat.BitsPerSample = wavFile.BitsPerSample;
			this.LoopStart = 0;
			this.LoopEnd = wavFile.Data.Length / wavFile.BlockAlign;
			this.Duration = new TimeSpan(0, 0, 0, 0, (int)(((double)this.LoopEnd / (double)wavFile.SampleRate + 0.0005) * 1000));
			this.Data = wavFile.Data;
		}

		public TimeSpan Duration { get; private set; }
		public AudioFileType FileType { get; private set; }
		public AudioFormat AudioFormat { get; private set; }
		public int LoopStart { get; private set; }
		public int LoopEnd { get; private set; }
		public byte[] Data { get; private set; }
	}
}


