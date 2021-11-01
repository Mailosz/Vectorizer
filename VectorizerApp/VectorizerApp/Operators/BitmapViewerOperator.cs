using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorizerApp.Operators
{
	class BitmapViewerOperator : IViewerOperator
	{
		Viewer viewer;
		internal CanvasBitmap bitmap;

		public BitmapViewerOperator(Viewer viewer, CanvasBitmap bitmap)
		{
			this.viewer = viewer;
			this.bitmap = bitmap;
		}

		public void Initialize()
		{
			viewer.SetSize(bitmap.Size);
		}

		public void Draw(DrawingArgs args)
		{
			args.Session.DrawImage(bitmap, 0f, 0f, new Windows.Foundation.Rect(0,0,bitmap.SizeInPixels.Width, bitmap.SizeInPixels.Height), 1f, CanvasImageInterpolation.NearestNeighbor);
		}
	}
}
