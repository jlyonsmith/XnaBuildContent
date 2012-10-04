using System;
using System.Runtime.InteropServices;

namespace XnaBuildContent
{
    public enum SquishMethod
    {
        Default = 0,

        // Use DXT1 compression.
        Dxt1 = (1 << 0), 
	
        // Use DXT3 compression.
        Dxt3 = (1 << 1), 
	
        // Use DXT5 compression.
        Dxt5 = (1 << 2)
	}

    public enum SquishFit
    {
        Default = 0,
        
        // Use a very slow but very high quality colour compressor.
        IterativeCluster = (1 << 8),	
	
        // Use a slow but high quality colour compressor (the default).
        Cluster = (1 << 3),	
	
        // Use a fast but low quality colour compressor.
        Range = (1 << 4)
	}

    public enum SquishMetric
    {
        Default = 0,

        // Use a perceptual metric for colour error (the default).
        Perceptual = (1 << 5),

        // Use a uniform metric for colour error.
        Uniform = (1 << 6)
    }
	
    public enum SquishExtra
    {
        None = 0, 

        // Weight the colour by alpha during cluster fit (disabled by default).
        WeightColourByAlpha = (1 << 7)
    }

    public static class Squish
    {
#if WINDOWS
		// TODO: Bet we can  use the simplified DllImports below on Windows too
        private class Native
        {
            [DllImport("Squish2", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = false, PreserveSig = true)]
            public static extern void SquishCompressMasked(IntPtr rgba, int mask, IntPtr block, int flags);
            [DllImport("Squish2", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = false, PreserveSig = true)]
            public static extern void SquishDecompress(IntPtr rgba, IntPtr block, int flags);
        }
#else
		private class Native
		{
			[DllImport("Squish2")]
			public static extern void SquishCompressMasked(IntPtr rgba, int mask, IntPtr block, int flags);
			[DllImport("Squish2")]
			public static extern void SquishDecompress(IntPtr rgba, IntPtr block, int flags);
		}
#endif

        public static byte[] CompressImage(byte[] rgba, int width, int height, SquishMethod method, SquishFit fit, SquishMetric metric, SquishExtra extra)
        {
            byte[] blocks = new byte[GetStorageRequirements(width, height, method)];

            unsafe 
            {
                // initialise the block output
	            fixed (byte* pTargetBlockBase = blocks, pRgba = rgba)
                {
                    byte* pTargetBlock = pTargetBlockBase;
	                int bytesPerBlock = method == SquishMethod.Dxt1 ? 8 : 16;

	                // loop over blocks
	                for( int y = 0; y < height; y += 4 )
	                {
		                for( int x = 0; x < width; x += 4 )
		                {
                            byte* pSourceRgba = stackalloc byte[16 * 4];
                            byte* pTargetPixel = pSourceRgba;
			                int mask = 0;
			                
                            for( int py = 0; py < 4; ++py )
			                {
				                for( int px = 0; px < 4; ++px )
				                {
					                // get the source pixel in the image
					                int sx = x + px;
					                int sy = y + py;
					
					                // enable if we're in the image
					                if( sx < width && sy < height )
					                {
						                // copy the rgba value
						                byte* pSourcePixel = pRgba + 4 * (width * sy + sx);
						        
                                        for (int i = 0; i < 4; ++i)
							                *pTargetPixel++ = *pSourcePixel++;
							
						                // enable this pixel
						                mask |= (1 << (4 * py + px));
					                }
					                else
					                {
						                // Skip this pixel as its outside the image
						                pTargetPixel += 4;
					                }
				                }
			                }
			
							try
							{
	                            Native.SquishCompressMasked((IntPtr)pSourceRgba, mask, (IntPtr)pTargetBlock, 
	                                (int)method | (int)fit | (int)metric | (int)extra);
							}
							catch (DllNotFoundException)
							{
								throw new DllNotFoundException("Shared library Squish2 not found");
							}

			                // advance
			                pTargetBlock += bytesPerBlock;
		                }
	                }
                }
            }

            return blocks;
        }

        public static byte[] DecompressImage(int width, int height, byte[] blocks, SquishMethod method)
        {
            byte[] rgba = new byte[4 * width * height];

            unsafe
            {
	            fixed (byte* pSourceBlockBase = blocks, pRgba = rgba)
                {
                    byte* pSourceBlock = pSourceBlockBase;
    	            int bytesPerBlock = method == SquishMethod.Dxt1 ? 8 : 16;

	                // loop over blocks
	                for (int y = 0; y < height; y += 4)
	                {
		                for (int x = 0; x < width; x += 4 )
		                {
			                // decompress the block
			                byte* pTargetRgba = stackalloc byte[4 * 16];

                            Native.SquishDecompress((IntPtr)pTargetRgba, (IntPtr)pSourceBlock, (int)method);
			
			                // write the decompressed pixels to the correct image locations
			                byte* pSourcePixel = pTargetRgba;
			                
                            for( int py = 0; py < 4; ++py )
			                {
				                for( int px = 0; px < 4; ++px )
				                {
					                // get the target location
					                int sx = x + px;
					                int sy = y + py;
					                if( sx < width && sy < height )
					                {
						                byte* pTargetPixel = pRgba + 4*(width * sy + sx);
						
						                // copy the rgba value
						                for (int i = 0; i < 4; ++i)
							                *pTargetPixel++ = *pSourcePixel++;
					                }
					                else
					                {
						                // skip this pixel as its outside the image
						                pSourcePixel += 4;
					                }
				                }
			                }
			
			                // advance
			                pSourceBlock += bytesPerBlock;
		                }
	                }
                }
            }

            return rgba;
        }

        public static int GetStorageRequirements(int width, int height, SquishMethod method)
        {
	        int blockcount = ((width + 3) / 4 ) * ((height + 3) / 4);
	        int blocksize = (method == SquishMethod.Dxt1) ? 8 : 16;
	        
            return blockcount * blocksize;	
        }

    }
}
