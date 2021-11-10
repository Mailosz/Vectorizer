using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorizerLib
{
	internal class RgbaByteSourceRegionComputer : ISourceRegionIterator<RgbaByteRegionData>
	{
		RgbaByteSource source;
		int iterator = 0;
		int pixel = 0;
		int rowi;
		int coli;
		int rowoffset;
		int w, h;




		internal RgbaByteSourceRegionComputer(RgbaByteSource source)
		{
			this.source = source;
		}

		public PixelValue CheckPixel(RgbaByteRegionData region)
		{
			throw new NotImplementedException();
		}

		public void AppendCurrentPixelValuesToRegionData(RgbaByteRegionData region)
		{
			for (int i = 0; i < 4; i++) { region.mean[i] += source.bitmap[iterator + i]; }

			//v1
			for (int a = 0; a < 4; a++)
			{
				for (int b = 0; b < 4; b++)
				{
					region.cov[a, b] += source.bitmap[iterator + a] * source.bitmap[iterator + b];
				}
			}

			//v2
			//for (int i = 0; i < 4; i++) { region.cov2[i,0] += source.bitmap[iterator + i] * source.bitmap[iterator + i]; }


		}


		public void AppendPixelLocation(RgbaByteRegionData computer)
		{
			region.area++;
			if (region.X1 > coli) region.X1 = coli;
			else if (region.X2 < coli) region.X2 = coli;
			if (region.Y1 > rowi) region.Y1 = rowi;
			else if (region.Y2 < rowi) region.Y2 = rowi;
		}

		public int GetPosition()
		{
			return pixel;
		}

		public bool Next()
		{
			coli++;
			if (coli == w) // next row
			{
				rowi++;
				if (rowi == h) return false;

				coli = 0;
				pixel += rowoffset;
				iterator = pixel * 4;
			}
			else
			{
				pixel += 1;
				iterator += 4;
			}
			return true;
		}

		public void ResetIterator(RgbaByteRegionData rd)
		{
			//number of pixels to skip on the end of a row
			w = rd.X2 - rd.X1 + 1;
			h = rd.Y2 - rd.Y1 + 1;
			rowoffset = source.Width - w + 1;

			pixel = rd.Y1 * source.Width + rd.X1;
			iterator = pixel * 4;
			coli = 0;
			rowi = 0;
		}


		public PixelValue CheckCurrentPixel(RgbaByteRegionData rd)
		{
			double pixelValue = 0;
			for (int i = 0; i < 4; i++)
			{
				pixelValue += source.bitmap[iterator + i] * rd.eigenVector[i];
			}

			if (pixelValue > rd.testValue)
			{
				return PixelValue.Above;
			}
			else
			{
				return PixelValue.Below;
			}
		}

	}
}
