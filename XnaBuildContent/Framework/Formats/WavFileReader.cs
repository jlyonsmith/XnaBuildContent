using System;
using System.IO;
using System.Linq;

namespace XnaBuildContent
{
	public class WavFileReader
	{
		public WavFileReader()
		{
		}

		public static WavFile ReadFile(string fileName)
		{
			//
			// Using .WAV file format from https://ccrma.stanford.edu/courses/422/projects/WaveFormat/
			// 

			WavFile wf = new WavFile();

			using (BinaryReader br = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read)))
			{
				char[] id = br.ReadChars(4);

				if (!id.SequenceEqual(new char[] { 'R', 'I', 'F', 'F' }))
					throw new FormatException("Missing RIFF header");

				br.ReadUInt32(); // chunk size

				id = br.ReadChars(4);

				if (!id.SequenceEqual(new char[] { 'W', 'A', 'V', 'E' }))
					throw new FormatException("Only WAVE format currently supported");

				id = br.ReadChars(4);

				if (!id.SequenceEqual(new char[] { 'f', 'm', 't', ' ' }))
					throw new FormatException("Missing 'fmt ' sub-chunk");

				int size = (int)br.ReadUInt32(); // sub-chunk size

				if (size != 16)
					throw new FormatException("Unexpected format size");

				wf.AudioFormat = br.ReadUInt16();
				wf.NumChannels = br.ReadUInt16();
				wf.SampleRate = br.ReadUInt32();
				wf.ByteRate = br.ReadUInt32();
				wf.BlockAlign = br.ReadUInt16();
				wf.BitsPerSample = br.ReadUInt16();

				id = br.ReadChars(4);

				if (!id.SequenceEqual(new char[] { 'd', 'a', 't', 'a' }))
					throw new FormatException("Missing 'data' sub-chunk");

				size = (int)br.ReadUInt32(); // sub-chunk size

				wf.Data = br.ReadBytes(size);
			}

			return wf;
		}
	}
}


