using System;
using System.Drawing.Imaging;
using System.Drawing;

namespace XnaBuildContent
{
	public class PngFileReader
	{
		public PngFileReader()
		{
		}

		public static PngFile ReadFile(string fileName)
		{
			PngFile pngFile = new PngFile();

			Bitmap bitmap = (Bitmap)Bitmap.FromFile(fileName);

			if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
				throw new NotSupportedException("PNG must be 32 bit RGBA format");

			pngFile.Width = bitmap.Width;
			pngFile.Height = bitmap.Height;
			pngFile.RgbaData = GetRgbaData(bitmap);

			return pngFile;
		}

        private unsafe static byte[] GetRgbaData(Bitmap bitmap)
        {
            byte[] rgba = new byte[4 * bitmap.Width * bitmap.Height];
            BitmapData bitmapData = null;

            try
            {
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            rgba[(y * bitmap.Width + x) * 4 + i] = ((byte*)bitmapData.Scan0)[y * bitmapData.Stride + x * 4 + i];
                        }
                    }
                }
            }
            finally
            {
                if (bitmapData != null)
                    bitmap.UnlockBits(bitmapData);
            }

            return rgba;
        }
	}
}


