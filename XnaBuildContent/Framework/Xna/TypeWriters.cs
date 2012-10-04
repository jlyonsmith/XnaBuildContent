using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using XnaBuildContent;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
	public abstract class CoreTypeWriter<T> : ContentTypeWriter<T>
	{
		public override string GetGenericArgumentTypeName()
		{
			string name = GetTypeName(TargetType);
			
			return String.Format("{0}, mscorlib, Version=3.7.0.0, Culture=neutral, PublicKeyToken=969db8053d3322ac", name);
		}
		
		public override string GetReaderTypeName()
		{
			string name = GetShortTypeName(this.GetType()).Replace("Writer", "Reader") + GetGenericArguments();
			
			// The readers for core types are in the XNA framework assemblies, but the types are obviously not
			return String.Format("Microsoft.Xna.Framework.Content.{0}", name); 
			// ", Microsoft.Xna.Framework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553"
		}
	}
	
	public abstract class XnaTypeWriter<T> : ContentTypeWriter<T>
    {
		public override string GetGenericArgumentTypeName()
		{
			string name = GetTypeName(TargetType);

			return String.Format("{0}, Microsoft.Xna.Framework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553", name);
		}

        public override string GetReaderTypeName()
        {
            string name = GetShortTypeName(this.GetType()).Replace("Writer", "Reader") + GetGenericArguments();

            // It looks like any type reader that is in the Microsoft.Xna.Framework assembly doesn't need to be 
			// fully qualified with an assembly
            return String.Format("Microsoft.Xna.Framework.Content.{0}", name); 
			// ", Microsoft.Xna.Framework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553"
        }
    }

	public abstract class XnaGraphicsTypeWriter<T> : ContentTypeWriter<T>
	{
		public override string GetReaderTypeName()
		{
			string name = GetShortTypeName(this.GetType()).Replace("Writer", "Reader") + GetGenericArguments();
			
			return String.Format("Microsoft.Xna.Framework.Content.{0}, Microsoft.Xna.Framework.Graphics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553", name);
		}
	}
	
	public abstract class XnaAudioTypeWriter<T> : ContentTypeWriter<T>
	{
		public override string GetReaderTypeName()
		{
			string name = GetShortTypeName(this.GetType()).Replace("Writer", "Reader") + GetGenericArguments();
			
			return String.Format("Microsoft.Xna.Framework.Content.{0}, Microsoft.Xna.Framework.Audio, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553", name);
		}
	}
	
	public class CharWriter : CoreTypeWriter<Char>
	{
		public override void Write(ContentWriter writer, Char value)
		{
			writer.Write(value);
		}
	}
	
	public class Int32Writer : CoreTypeWriter<Int32>
	{
		public override void Write(ContentWriter writer, Int32 value)
		{
			writer.Write(value);
		}
	}
	
	public class SingleWriter : CoreTypeWriter<Single>
	{
		public override void Write(ContentWriter writer, Single value)
		{
			writer.Write(value);
		}
	}
	
	public class Vector3Writer : XnaTypeWriter<Vector3>
	{
		public override void Write(ContentWriter writer, Vector3 value)
		{
			writer.Write(value);
		}
	}

	public class StringWriter : CoreTypeWriter<String>
    {
        public override void Write(ContentWriter writer, string value)
        {
            writer.Write(value);
        }
    }

    public class RectangleWriter : XnaTypeWriter<Rectangle>
    {
        public override void Write(ContentWriter writer, Rectangle value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Width);
            writer.Write(value.Height);
        }
    }

    public class ArrayWriter<T> : CoreTypeWriter<T[]>
    {
        private ContentTypeWriter elementTypeWriter;

        public override void Initialize(XnbFileWriterV5 xnbWriter)
        {
            if (elementTypeWriter == null)
                elementTypeWriter = xnbWriter.GetTypeWriter(typeof(T));

            base.Initialize(xnbWriter);
        }

        public override void Write(ContentWriter writer, T[] array)
        {
            writer.Write(array.Length);

            foreach (T element in array)
            {
                writer.WriteObject<T>(element, elementTypeWriter);
            }
        }
    }

	public class ListWriter<T> : CoreTypeWriter<List<T>>
	{
		private ContentTypeWriter elementTypeWriter;
		
		public override void Initialize(XnbFileWriterV5 xnbWriter)
		{
			if (elementTypeWriter == null)
				elementTypeWriter = xnbWriter.GetTypeWriter(typeof(T));
			
			base.Initialize(xnbWriter);
		}
		
		public override void Write(ContentWriter writer, List<T> list)
		{
			writer.Write(list.Count);

			foreach (T element in list)
			{
				writer.WriteObject<T>(element, elementTypeWriter);
			}
		}
	}

	public abstract class BaseTextureWriter<T> : XnaGraphicsTypeWriter<T> where T : TextureContent
    {
        public BaseTextureWriter()
        {
        }

        public override void Write(ContentWriter output, T value)
        {
            BitmapContent content = value.Faces[0][0];

            this.WriteTextureHeader(output, content.Format, content.Width, content.Height, value.Faces.Count, value.Faces[0].Count);
            this.WriteTextureData(output, value);
        }

        protected virtual void WriteTextureData(ContentWriter output, T texture)
        {
            foreach (MipmapChain chain in texture.Faces)
            {
                foreach (BitmapContent content in chain)
                {
					byte[] pixelData = content.Data;

                    output.Write(pixelData.Length);
                    output.Write(pixelData);
                }
            }
        }

        protected abstract void WriteTextureHeader(ContentWriter output, SurfaceFormat format, int width, int height, int depth, int mipLevels);
    }

    public class Texture2DWriter : BaseTextureWriter<Texture2DContent>
    {
        protected override void WriteTextureHeader(ContentWriter output, SurfaceFormat format, int width, int height, int depth, int mipLevels)
        {
            output.Write((int)format);
            output.Write(width);
            output.Write(height);
            output.Write(mipLevels);
        }
    }

	public class SoundEffectWriter : XnaAudioTypeWriter<AudioContent>
	{
        public override void Write(ContentWriter writer, AudioContent value)
        {
			ushort pcmFormat;

			if (value.FileType != AudioFileType.Wav)
				throw new NotImplementedException("Only WAV file support currently implemented");
			else
				pcmFormat = 1;

			writer.Write((uint)18); // total size of the following WAVEFORMATEX structure
			writer.Write((ushort)pcmFormat);
			writer.Write((ushort)value.AudioFormat.Channels);
			writer.Write((uint)value.AudioFormat.SampleRate);
			writer.Write((uint)value.AudioFormat.AverageBytesPerSecond);
			writer.Write((ushort)value.AudioFormat.BlockAlign);
			writer.Write((ushort)value.AudioFormat.BitsPerSample);
			writer.Write((ushort)0); // No extra bytes

			byte[] data = value.Data;

			writer.Write(data.Length);
			writer.Write(data);

			writer.Write(value.LoopStart);
			writer.Write(value.LoopEnd);
			writer.Write(value.Duration.Milliseconds);
        }
	}

	public class SpriteFontWriter : XnaGraphicsTypeWriter<SpriteFontContent>
	{
		public override void Write(ContentWriter writer, SpriteFontContent value)
		{
			writer.WriteObject<Texture2DContent>(value.Texture);
			writer.WriteObject<List<Rectangle>>(value.Glyphs);
			writer.WriteObject<List<Rectangle>>(value.Cropping);
			writer.WriteObject<List<char>>(value.CharacterMap);
			writer.Write(value.VerticalSpacing);
			writer.Write(value.HorizontalSpacing);
			writer.WriteObject<List<Vector3>>(value.Kerning);
			writer.Write(value.DefaultCharacter.HasValue);

			if (value.DefaultCharacter.HasValue)
			{
				writer.Write(value.DefaultCharacter.Value);
			}
		}
	}
}
