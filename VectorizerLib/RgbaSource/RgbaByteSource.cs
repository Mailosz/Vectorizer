using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorizerLib
{
	public class RgbaByteSource : IRasterSource<RgbaByteRegionData>
	{
		int width;
		public int Width => width;

		int height;
		public int Height => height;

		int length;
		public int Length => length;

		internal byte[] bitmap;
		internal int onerow;

		public RgbaByteSource(byte[] bitmap, int width)
		{
			if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));

			this.bitmap = bitmap;
			this.width = width;

			onerow = width * 4;

			if (bitmap.Length % 4 != 0 || bitmap.Length / 4 % width != 0) throw new ArgumentOutOfRangeException(nameof(width));

			this.length = bitmap.Length / 4;
			this.height = this.length / this.width;
		}

		ISourceRegionIterator<RgbaByteRegionData> IRasterSource<RgbaByteRegionData>.CreateRegionIterator()
		{
			return new RgbaByteSourceRegionComputer(this);
		}

		RgbaByteRegionData IRasterSource<RgbaByteRegionData>.CreateRegionData()
		{
			return new RgbaByteRegionData();
		}
	}
}
