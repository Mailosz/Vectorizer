using System;
using System.Collections.Generic;

namespace VectorizerLib
{
	public interface IRasterSource<D> where D : IRegionData
	{
		int Width { get; }
		int Height { get; }
		int Length { get; }

		internal ISourceRegionIterator<D> CreateRegionIterator();
		internal D CreateRegionData();
	}

	internal interface ISourceRegionIterator<D>
	{
		void ResetIterator(D region);
		int GetPosition();
		bool Next();
		void AppendCurrentPixelValuesToRegionData(D region);
		void AppendPixelValuesToRegionData(D region, int pixel);
		void AppendPixelLocation(D region);
		PixelValue CheckCurrentPixel(D region);
	}

	internal enum PixelValue { Above, Below}
}