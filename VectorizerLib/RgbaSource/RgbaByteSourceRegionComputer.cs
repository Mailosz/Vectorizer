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
		int linestart, edge, end;




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

			AppendPixelValuesToRegionData(region, pixel);
		}

		public void AppendPixelValuesToRegionData(RgbaByteRegionData region, int pixel)
		{
			int iterator = pixel * 4;
			double[] diff = new double[4];
			for (int i = 0; i < 4; i++) 
			{ 
				diff[i] = source.bitmap[iterator + i] - region.mean[i];
				region.mean[i] += diff[i] / region.Area; 
			}

			//v1
			for (int a = 0; a < 4; a++)
			{
				for (int b = 0; b < 4; b++)
				{
					region.cov[a, b] += diff[a] * diff[b] * (region.Area - 1) / region.Area;
				}
			}

			//v2
			//for (int i = 0; i < 4; i++) { region.cov2[i,0] += source.bitmap[iterator + i] * source.bitmap[iterator + i]; }

		}

		public void AppendPixelLocation(RgbaByteRegionData region)
		{
			region.AppendPixelLocation(coli, rowi);
		}


		public int GetPosition()
		{
			return pixel;
		}

		public bool Next()
		{
			coli++;
			if (coli > edge) // next row
			{
				rowi++;
				if (rowi > end) return false;

				coli = linestart;
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
			int w = rd.X2 - rd.X1 + 1;
			linestart = rd.X1;
			edge = rd.X2;
			end = rd.Y2;
			rowoffset = source.Width - w + 1;

			pixel = rd.Y1 * source.Width + rd.X1;
			iterator = pixel * 4;
			coli = rd.X1;
			rowi = rd.Y1;
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
