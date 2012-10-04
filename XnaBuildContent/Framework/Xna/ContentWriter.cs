using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace XnaBuildContent
{
    public class ContentWriter : BinaryWriter
    {
        private Dictionary<object, bool> activeObjects;
        private XnbFileWriterV5 xnbWriter;

        public ContentWriter(Stream stream, XnbFileWriterV5 xnbWriter) : base(stream)
        {
            this.xnbWriter = xnbWriter;
            this.activeObjects = new Dictionary<object, bool>();
        }

        public void WriteEncodedInt32(int value)
        {
            this.Write7BitEncodedInt(value);
        }

		public void Write(Vector3 value)
		{
			this.Write(value.X);
			this.Write(value.Y);
			this.Write(value.Z);
		}

        public void WriteObject<T>(T value)
        {
            if (value == null)
            {
                Write7BitEncodedInt(0);
            }
            else
            {
                int typeIndex;
                ContentTypeWriter typeWriter = xnbWriter.GetTypeWriter(value.GetType(), out typeIndex);

                // Index to to type writers is 1 based; 0 is NULL
                Write7BitEncodedInt(typeIndex + 1);

                if (activeObjects.ContainsKey(value))
                    throw new InvalidOperationException("Recursive object graph detected");

                activeObjects.Add(value, true);
                InvokeWriter<T>(value, typeWriter);
                activeObjects.Remove(value);
            }
        }

        internal void WriteObject<T>(T value, ContentTypeWriter writer)
        {
            if (writer.IsValueTargetType)
                InvokeWriter<T>(value, writer);
            else
                WriteObject<T>(value);
        }

        private void InvokeWriter<T>(T value, ContentTypeWriter writer)
        {
            ContentTypeWriter<T> genericWriter = writer as ContentTypeWriter<T>;

            if (genericWriter != null)
            {
                genericWriter.Write(this, value);
            }
            else
            {
                writer.Write(this, value);
            }
        }
    }
}

